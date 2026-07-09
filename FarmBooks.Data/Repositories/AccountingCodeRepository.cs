using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class AccountingCodeRepository
{
    private readonly DbConnectionFactory _db;

    public AccountingCodeRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(AccountingCode code)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        INSERT INTO AccountingCodes (
            CodeId, Code, Name, Description, IsActive, CreatedAt, UpdatedAt
        )
        VALUES (
            @CodeId, @Code, @Name, @Description, @IsActive, @CreatedAt, @UpdatedAt
        );
        """, code);
    }

    public async Task<AccountingCode?> GetAsync(string codeId)
    {
        using var connection = _db.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<AccountingCode>("""
        SELECT *
        FROM AccountingCodes
        WHERE CodeId = @CodeId;
        """, new { CodeId = codeId });
    }

    public async Task<IReadOnlyList<AccountingCode>> ListActiveAsync()
    {
        using var connection = _db.CreateConnection();

        var codes = await connection.QueryAsync<AccountingCode>("""
        SELECT *
        FROM AccountingCodes
        WHERE IsActive = 1
        ORDER BY Code;
        """);

        return codes.ToList();
    }

    public async Task<IReadOnlyList<AccountingCode>> ListAllAsync()
    {
        using var connection = _db.CreateConnection();

        var codes = await connection.QueryAsync<AccountingCode>("""
        SELECT *
        FROM AccountingCodes
        ORDER BY IsActive DESC, Code;
        """);

        return codes.ToList();
    }

    public async Task UpdateAsync(AccountingCode code)
    {
        using var connection = _db.CreateConnection();

        code.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync("""
        UPDATE AccountingCodes
        SET Code = @Code,
            Name = @Name,
            Description = @Description,
            IsActive = @IsActive,
            UpdatedAt = @UpdatedAt
        WHERE CodeId = @CodeId;
        """, code);
    }

    public async Task DisableAsync(string codeId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE AccountingCodes
        SET IsActive = 0,
            UpdatedAt = @Now
        WHERE CodeId = @CodeId;
        """, new
        {
            CodeId = codeId,
            Now = DateTime.UtcNow
        });
    }

    public async Task ReactivateAsync(string codeId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        UPDATE AccountingCodes
        SET IsActive = 1,
            UpdatedAt = @Now
        WHERE CodeId = @CodeId;
        """, new
        {
            CodeId = codeId,
            Now = DateTime.UtcNow
        });
    }
}