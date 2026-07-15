using System.Data;
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

    public async Task CreateAsync(Transaction transaction, int? insertAtStatementOrder = null)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var databaseTransaction = connection.BeginTransaction();

        try
        {
            if (insertAtStatementOrder is not null)
            {
                await ShiftStatementOrdersDownAsync(
                    connection,
                    databaseTransaction,
                    transaction.PaymentDate,
                    insertAtStatementOrder.Value
                );

                transaction.StatementOrder = insertAtStatementOrder.Value;
            }
            else
            {
                transaction.StatementOrder = await GetNextStatementOrderAsync(
                    connection,
                    databaseTransaction,
                    transaction.PaymentDate
                );
            }

            const string sql = """
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
                """;

            await connection.ExecuteAsync(sql, transaction, databaseTransaction);

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
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
            ORDER BY
                PaymentDate DESC,
                StatementOrder ASC,
                CreatedAt ASC;
            """;

        var transactions = await connection.QueryAsync<Transaction>(sql);

        return transactions.ToList();
    }

    public async Task UpdateAsync(
        Transaction transaction,
        DateTime? originalPaymentDate,
        int originalStatementOrder
    )
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var databaseTransaction = connection.BeginTransaction();

        try
        {
            var paymentDateChanged = !AreSamePaymentDate(
                originalPaymentDate,
                transaction.PaymentDate
            );

            if (paymentDateChanged)
            {
                await CloseStatementOrderGapAsync(
                    connection,
                    databaseTransaction,
                    originalPaymentDate,
                    originalStatementOrder
                );

                transaction.StatementOrder = await GetNextStatementOrderAsync(
                    connection,
                    databaseTransaction,
                    transaction.PaymentDate
                );
            }
            else
            {
                transaction.StatementOrder = originalStatementOrder;
            }

            transaction.UpdatedAt = DateTime.UtcNow;

            const string sql = """
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
                """;

            await connection.ExecuteAsync(sql, transaction, databaseTransaction);

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var databaseTransaction = connection.BeginTransaction();

        try
        {
            var now = DateTime.UtcNow;

            const string deleteSql = """
                UPDATE Transactions
                SET
                    DeletedAt = @now,
                    UpdatedAt = @now
                WHERE TransactionId = @transactionId
                  AND DeletedAt IS NULL;
                """;

            var affectedRows = await connection.ExecuteAsync(
                deleteSql,
                new { transactionId = transaction.TransactionId, now },
                databaseTransaction
            );

            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Transaction was not found or was already deleted."
                );
            }

            await CloseStatementOrderGapAsync(
                connection,
                databaseTransaction,
                transaction.PaymentDate,
                transaction.StatementOrder
            );

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
    }

    public async Task RestoreAsync(string transactionId)
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE Transactions
            SET
                DeletedAt = NULL,
                UpdatedAt = @now
            WHERE TransactionId = @transactionId;
            """,
            new { transactionId, now = DateTime.UtcNow }
        );
    }

    public async Task<IReadOnlyList<Transaction>> ListForPaymentDateAsync(DateTime? paymentDate)
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
            WHERE
                (
                    (@paymentDate IS NULL AND PaymentDate IS NULL)
                    OR date(PaymentDate) = date(@paymentDate)
                )
                AND DeletedAt IS NULL
            ORDER BY
                StatementOrder ASC,
                CreatedAt ASC;
            """;

        var transactions = await connection.QueryAsync<Transaction>(sql, new { paymentDate });

        return transactions.ToList();
    }

    public async Task NormalizeStatementOrdersAsync(DateTime? paymentDate)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var databaseTransaction = connection.BeginTransaction();

        try
        {
            const string selectSql = """
                SELECT TransactionId
                FROM Transactions
                WHERE
                    (
                        (@paymentDate IS NULL AND PaymentDate IS NULL)
                        OR date(PaymentDate) = date(@paymentDate)
                    )
                    AND DeletedAt IS NULL
                ORDER BY
                    StatementOrder ASC,
                    CreatedAt ASC,
                    TransactionId ASC;
                """;

            var transactionIds = (
                await connection.QueryAsync<string>(
                    selectSql,
                    new { paymentDate },
                    databaseTransaction
                )
            ).ToList();

            const string updateSql = """
                UPDATE Transactions
                SET
                    StatementOrder = @statementOrder,
                    UpdatedAt = @now
                WHERE TransactionId = @transactionId
                  AND DeletedAt IS NULL;
                """;

            var now = DateTime.UtcNow;

            for (var index = 0; index < transactionIds.Count; index++)
            {
                await connection.ExecuteAsync(
                    updateSql,
                    new
                    {
                        transactionId = transactionIds[index],

                        statementOrder = index + 1,
                        now,
                    },
                    databaseTransaction
                );
            }

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
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
                SET
                    StatementOrder = @statementOrder,
                    UpdatedAt = @now
                WHERE TransactionId = @transactionId
                  AND DeletedAt IS NULL;
                """;

            var now = DateTime.UtcNow;

            // Temporarily move the first transaction outside
            // the normal positive sequence.
            await connection.ExecuteAsync(
                sql,
                new
                {
                    transactionId = firstTransactionId,
                    statementOrder = -1,
                    now,
                },
                databaseTransaction
            );

            // Give the second transaction the first
            // transaction's old order.
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

            // Give the first transaction the second
            // transaction's old order.
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

            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
    }

    private static async Task<int> GetNextStatementOrderAsync(
        IDbConnection connection,
        IDbTransaction databaseTransaction,
        DateTime? paymentDate
    )
    {
        const string sql = """
            SELECT COALESCE(MAX(StatementOrder), 0) + 1
            FROM Transactions
            WHERE
                (
                    (@paymentDate IS NULL AND PaymentDate IS NULL)
                    OR date(PaymentDate) = date(@paymentDate)
                )
                AND DeletedAt IS NULL;
            """;

        return await connection.ExecuteScalarAsync<int>(
            sql,
            new { paymentDate },
            databaseTransaction
        );
    }

    private static async Task ShiftStatementOrdersDownAsync(
        IDbConnection connection,
        IDbTransaction databaseTransaction,
        DateTime? paymentDate,
        int startingOrder
    )
    {
        const string sql = """
            UPDATE Transactions
            SET
                StatementOrder = StatementOrder + 1,
                UpdatedAt = @now
            WHERE
                (
                    (@paymentDate IS NULL AND PaymentDate IS NULL)
                    OR date(PaymentDate) = date(@paymentDate)
                )
                AND StatementOrder >= @startingOrder
                AND DeletedAt IS NULL;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                paymentDate,
                startingOrder,
                now = DateTime.UtcNow,
            },
            databaseTransaction
        );
    }

    private static async Task CloseStatementOrderGapAsync(
        IDbConnection connection,
        IDbTransaction databaseTransaction,
        DateTime? paymentDate,
        int removedOrder
    )
    {
        const string sql = """
            UPDATE Transactions
            SET
                StatementOrder = StatementOrder - 1,
                UpdatedAt = @now
            WHERE
                (
                    (@paymentDate IS NULL AND PaymentDate IS NULL)
                    OR date(PaymentDate) = date(@paymentDate)
                )
                AND StatementOrder > @removedOrder
                AND DeletedAt IS NULL;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                paymentDate,
                removedOrder,
                now = DateTime.UtcNow,
            },
            databaseTransaction
        );
    }

    private static bool AreSamePaymentDate(DateTime? first, DateTime? second)
    {
        if (first is null || second is null)
            return first is null && second is null;

        return first.Value.Date == second.Value.Date;
    }
}
