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

        return await connection.QuerySingleOrDefaultAsync<Expense>(
            """
            SELECT *
            FROM Expenses
            WHERE ExpenseId = @ExpenseId
              AND DeletedAt IS NULL;
            """,
            new { ExpenseId = expenseId }
        );
    }

    public async Task<IReadOnlyList<Expense>> ListAsync()
    {
        using var connection = _db.CreateConnection();

        var expenses = await connection.QueryAsync<Expense>(
            """
            SELECT *
            FROM Expenses
            WHERE DeletedAt IS NULL
            ORDER BY ExpenseDate DESC, CreatedAt DESC;
            """
        );

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
