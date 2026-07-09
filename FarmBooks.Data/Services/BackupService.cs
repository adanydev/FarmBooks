using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Data.Services;

public sealed class BackupService
{
    private readonly string _databasePath;
    private readonly BackupRepository _backupRepository;
    private readonly SettingsService _settingsService;

    public BackupService(
        string databasePath,
        BackupRepository backupRepository,
        SettingsService settingsService)
    {
        _databasePath = databasePath;
        _backupRepository = backupRepository;
        _settingsService = settingsService;
    }

    public async Task<string> BackupDatabaseAsync(string? backupFolder = null)
    {
        backupFolder ??= await _settingsService.GetAsync("BackupFolder");

        if (string.IsNullOrWhiteSpace(backupFolder))
        {
            backupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "FarmBooks Backups");
        }

        Directory.CreateDirectory(backupFolder);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        var destinationPath = Path.Combine(
            backupFolder,
            $"FarmBooks_{timestamp}.sqlite");

        try
        {
            File.Copy(_databasePath, destinationPath, overwrite: false);

            var record = new BackupRecord
            {
                BackupRecordId = Guid.NewGuid().ToString(),
                FilePath = destinationPath,
                CreatedAt = DateTime.UtcNow,
                WasSuccessful = true,
                Notes = "Manual backup created."
            };

            await _backupRepository.AddAsync(record);
            await _settingsService.SaveAsync("LastBackupDate", DateTime.UtcNow.ToString("O"));
            await _settingsService.SaveAsync("BackupFolder", backupFolder);

            return destinationPath;
        }
        catch (Exception ex)
        {
            await _backupRepository.AddAsync(new BackupRecord
            {
                BackupRecordId = Guid.NewGuid().ToString(),
                FilePath = destinationPath,
                CreatedAt = DateTime.UtcNow,
                WasSuccessful = false,
                Notes = ex.Message
            });

            throw;
        }
    }

    public bool ValidateBackup(string backupPath)
    {
        return File.Exists(backupPath)
            && new FileInfo(backupPath).Length > 0;
    }

    public Task<BackupRecord?> GetLastSuccessfulBackupAsync()
    {
        return _backupRepository.GetLastSuccessfulAsync();
    }

    public Task<IReadOnlyList<BackupRecord>> ListBackupsAsync()
    {
        return _backupRepository.ListAsync();
    }
}