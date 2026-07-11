using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class ExpenseRepository
{
    private readonly DbConnectionFactory _db;

    public ExpenseRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(Expense expense)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO Expenses
            (
                ExpenseId,
                ExpenseDate,
                PaidDate,
                SourceType,
                DocumentNumber,
                BusinessName,
                Description,
                Total,
                VatApplicability,
                VatEntryMethod,
                VATC,
                VATS,
                IsVatClassificationConfirmed,
                Notes,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            )
            VALUES
            (
                @ExpenseId,
                @ExpenseDate,
                @PaidDate,
                @SourceType,
                @DocumentNumber,
                @BusinessName,
                @Description,
                @Total,
                @VatApplicability,
                @VatEntryMethod,
                @VATC,
                @VATS,
                @IsVatClassificationConfirmed,
                @Notes,
                @CreatedAt,
                @UpdatedAt,
                @DeletedAt
            );
            """,
            expense
        );
    }

    public async Task<Expense?> GetAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                ExpenseId,
                ExpenseDate,
                PaidDate,
                SourceType,
                DocumentNumber,
                BusinessName,
                Description,
                CAST(Total AS REAL) AS Total,
                VatApplicability,
                VatEntryMethod,
                CAST(VATC AS REAL) AS VATC,
                CAST(VATS AS REAL) AS VATS,
                IsVatClassificationConfirmed,
                Notes,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM Expenses
            WHERE ExpenseId = @expenseId
              AND DeletedAt IS NULL;
            """;

        return await connection.QuerySingleOrDefaultAsync<Expense>(sql, new { expenseId });
    }

    public async Task<IReadOnlyList<Expense>> ListAsync()
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                ExpenseId,
                ExpenseDate,
                PaidDate,
                SourceType,
                DocumentNumber,
                BusinessName,
                Description,

                CAST(Total AS REAL) AS Total,

                VatApplicability,
                VatEntryMethod,

                CAST(VATC AS REAL) AS VATC,
                CAST(VATS AS REAL) AS VATS,

                IsVatClassificationConfirmed,
                Notes,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM Expenses
            WHERE DeletedAt IS NULL
            ORDER BY PaidDate DESC, ExpenseDate DESC;
            """;

        var expenses = await connection.QueryAsync<Expense>(sql);

        return expenses.ToList();
    }

    public async Task UpdateAsync(Expense expense)
    {
        using var connection = _db.CreateConnection();

        expense.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET
                ExpenseDate = @ExpenseDate,
                PaidDate = @PaidDate,
                SourceType = @SourceType,
                DocumentNumber = @DocumentNumber,
                BusinessName = @BusinessName,
                Description = @Description,
                Total = @Total,
                VatApplicability = @VatApplicability,
                VatEntryMethod = @VatEntryMethod,
                VATC = @VATC,
                VATS = @VATS,
                IsVatClassificationConfirmed =
                    @IsVatClassificationConfirmed,
                Notes = @Notes,
                UpdatedAt = @UpdatedAt
            WHERE ExpenseId = @ExpenseId
              AND DeletedAt IS NULL;
            """,
            expense
        );
    }

    public async Task SoftDeleteAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET DeletedAt = @Now,
                UpdatedAt = @Now
            WHERE ExpenseId = @ExpenseId
              AND DeletedAt IS NULL;
            """,
            new { ExpenseId = expenseId, Now = DateTime.UtcNow }
        );
    }

    public async Task RestoreAsync(string expenseId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET DeletedAt = NULL,
                UpdatedAt = @Now
            WHERE ExpenseId = @ExpenseId;
            """,
            new { ExpenseId = expenseId, Now = DateTime.UtcNow }
        );
    }
}
