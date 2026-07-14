using Dapper;
using FarmBooks.Core.Models;
using FarmBooks.Data.Database;

namespace FarmBooks.Data.Repositories;

public sealed class TransactionRepository
{
    private readonly DbConnectionFactory _db;

    public TransactionRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(Transaction transaction)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO Transactions
            (
                TransactionId,
                ReceiptDate,
                PaymentDate,
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
                StatementOrder,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            )
            VALUES
            (
                @TransactionId,
                @ReceiptDate,
                @PaymentDate,
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
                @StatementOrder,
                @CreatedAt,
                @UpdatedAt,
                @DeletedAt
            );
            """,
            transaction
        );
    }

    public async Task<Transaction?> GetAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                TransactionId,
                ReceiptDate,
                PaymentDate,
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
                StatementOrder,
                Notes,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM Transactions
            WHERE TransactionId = @transactionId
              AND DeletedAt IS NULL;
            """;

        return await connection.QuerySingleOrDefaultAsync<Transaction>(sql, new { transactionId });
    }

    public async Task<IReadOnlyList<Transaction>> ListAsync()
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                TransactionId,
                ReceiptDate,
                PaymentDate,
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
                StatementOrder,
                Notes,
                CreatedAt,
                UpdatedAt,
                DeletedAt
            FROM Transactions
            WHERE DeletedAt IS NULL
            ORDER BY PaymentDate DESC,
            StatementOrder ASC,
            CreatedAt ASC;
            """;

        var transactions = await connection.QueryAsync<Transaction>(sql);

        return transactions.ToList();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        using var connection = _db.CreateConnection();

        transaction.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync(
            """
            UPDATE Transactions
            SET
                ReceiptDate = @ReceiptDate,
                PaymentDate = @PaymentDate,
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
                StatementOrder = @StatementOrder,
                Notes = @Notes,
                UpdatedAt = @UpdatedAt
            WHERE TransactionId = @TransactionId
              AND DeletedAt IS NULL;
            """,
            transaction
        );
    }

    public async Task SoftDeleteAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE Transactions
            SET DeletedAt = @Now,
                UpdatedAt = @Now
            WHERE TransactionId = @TransactionId
              AND DeletedAt IS NULL;
            """,
            new { TransactionId = transactionId, Now = DateTime.UtcNow }
        );
    }

    public async Task RestoreAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE Transactions
            SET DeletedAt = NULL,
                UpdatedAt = @Now
            WHERE TransactionId = @TransactionId;
            """,
            new { TransactionId = transactionId, Now = DateTime.UtcNow }
        );
    }

    public async Task<IReadOnlyList<Transaction>> ListForPaymentDateAsync(DateTime paymentDate)
    {
        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT
                TransactionId,
                ReceiptDate,
                PaymentDate,
                StatementOrder,
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
            FROM Transactions
            WHERE date(PaymentDate) = date(@paymentDate)
              AND DeletedAt IS NULL
            ORDER BY StatementOrder ASC, CreatedAt ASC;
            """;

        var transactions = await connection.QueryAsync<Transaction>(sql, new { paymentDate });

        return transactions.ToList();
    }

    public async Task<int> GetNextStatementOrderAsync(DateTime? paymentDate)
    {
        if (paymentDate is null)
            return 0;

        using var connection = _db.CreateConnection();

        const string sql = """
            SELECT COALESCE(MAX(StatementOrder), 0) + 1
            FROM Transactions
            WHERE date(PaymentDate) = date(@paymentDate)
              AND DeletedAt IS NULL;
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new { paymentDate });
    }

    public async Task UpdateStatementOrdersAsync(
        string firstTransactionId,
        int firstOrder,
        string secondTransactionId,
        int secondOrder
    )
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var databaseTransaction = connection.BeginTransaction();

        try
        {
            const string sql = """
                UPDATE Transactions
                SET StatementOrder = @statementOrder,
                    UpdatedAt = @now
                WHERE TransactionId = @transactionId
                  AND DeletedAt IS NULL;
                """;

            var now = DateTime.UtcNow;

            await connection.ExecuteAsync(
                sql,
                new
                {
                    transactionId = firstTransactionId,
                    statementOrder = firstOrder,
                    now,
                },
                databaseTransaction
            );

            await connection.ExecuteAsync(
                sql,
                new
                {
                    transactionId = secondTransactionId,
                    statementOrder = secondOrder,
                    now,
                },
                databaseTransaction
            );

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
    }
}
