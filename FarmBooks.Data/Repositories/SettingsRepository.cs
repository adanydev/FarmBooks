using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class SettingsRepository
{
    private readonly DbConnectionFactory _db;

    public SettingsRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task SaveAsync(string key, string? value)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO ApplicationSettings (Key, Value, UpdatedAt)
        VALUES (@Key, @Value, @UpdatedAt)
        ON CONFLICT(Key) DO UPDATE SET
            Value = excluded.Value,
            UpdatedAt = excluded.UpdatedAt;
        """, new
        {
            Key = key,
            Value = value,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<string?> GetAsync(string key)
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<string?>("""
        SELECT Value
        FROM ApplicationSettings
        WHERE Key = @Key;
        """, new { Key = key });
    }

    public async Task<IReadOnlyList<ApplicationSetting>> ListAsync()
    {
        using var connection = _db.CreateConnection();

        var settings = await connection.QueryAsync<ApplicationSetting>("""
        SELECT *
        FROM ApplicationSettings
        ORDER BY Key;
        """);

        return settings.ToList();
    }
}