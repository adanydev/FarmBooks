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
}