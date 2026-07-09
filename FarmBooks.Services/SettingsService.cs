using FarmBooks.Core.Models;
using FarmBooks.Data.Repositories;

namespace FarmBooks.Services;

public sealed class SettingsService
{
    private readonly SettingsRepository _settings;

    public SettingsService(SettingsRepository settings)
    {
        _settings = settings;
    }

    public Task SaveAsync(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Setting key is required.");

        return _settings.SaveAsync(key.Trim(), value);
    }

    public Task<string?> GetAsync(string key)
    {
        return _settings.GetAsync(key);
    }

    public Task<IReadOnlyList<ApplicationSetting>> ListAsync()
    {
        return _settings.ListAsync();
    }
}