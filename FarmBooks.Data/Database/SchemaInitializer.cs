using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace FarmBooks.Data.Database;

public sealed class SchemaInitializer
{
    private readonly DbConnectionFactory _db;
    private readonly ILogger<SchemaInitializer> _logger;

    public SchemaInitializer(DbConnectionFactory db, ILogger<SchemaInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        using var connection = _db.CreateConnection();

        _logger.LogInformation("Opening database connection for schema initialization.");

        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await CreateTransactionsTableAsync(connection, transaction);
            await CreateAccountingCodesTableAsync(connection, transaction);
            await CreateTransactionLineItemsTableAsync(connection, transaction);
            await CreateBankAccountsTableAsync(connection, transaction);
            await CreateBankStatementsTableAsync(connection, transaction);
            await CreateBankTransactionsTableAsync(connection, transaction);
            await CreateTransactionMatchesTableAsync(connection, transaction);
            await CreateTransactionMatchIndexesAsync(connection, transaction);
            await CreateImportBatchesTableAsync(connection, transaction);
            await CreateImportBatchRowsTableAsync(connection, transaction);
            await CreateApplicationSettingsTableAsync(connection, transaction);
            await CreateBackupRecordsTableAsync(connection, transaction);
            await CreateTransactionDocumentsTableAsync(connection, transaction);
            await CreateAuditLogEntriesTableAsync(connection, transaction);

            transaction.Commit();

            _logger.LogInformation("Database schema initialization completed successfully.");
        }
        catch (Exception exception)
        {
            transaction.Rollback();

            _logger.LogError(exception, "Database schema initialization failed.");

            throw;
        }
    }

    private Task CreateTransactionsTableAsync(IDbConnection connection, IDbTransaction transaction)
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateTransactionsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS Transactions
            (
                TransactionId TEXT PRIMARY KEY,
                ReceiptDate TEXT NULL,
                PaymentDate TEXT NULL,
                SourceType INTEGER NOT NULL,
                DocumentNumber TEXT NULL,
                BusinessName TEXT NULL,
                Description TEXT NULL,
                Total NUMERIC NOT NULL,

                VatApplicability INTEGER NOT NULL DEFAULT 0,
                VatEntryMethod INTEGER NOT NULL DEFAULT 0,
                VATC NUMERIC NULL,
                VATS NUMERIC NULL,
                IsVatClassificationConfirmed INTEGER NOT NULL DEFAULT 0,
                
                StatementOrder INTEGER NOT NULL,

                Notes TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL
            );
            """
        );
    }

    private Task CreateAccountingCodesTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateAccountingCodesTableAsync),
            """
            CREATE TABLE IF NOT EXISTS AccountingCodes
            (
                CodeId TEXT PRIMARY KEY,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Description TEXT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """
        );
    }

    private Task CreateTransactionLineItemsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateTransactionLineItemsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS TransactionLineItems
            (
                TransactionLineItemId TEXT PRIMARY KEY,
                TransactionId TEXT NOT NULL,
                CodeId TEXT NULL,
                Description TEXT NULL,
                Total NUMERIC NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL,

                FOREIGN KEY (TransactionId)
                    REFERENCES Transactions(TransactionId),

                FOREIGN KEY (CodeId)
                    REFERENCES AccountingCodes(CodeId)
            );
            """
        );
    }

    private Task CreateBankAccountsTableAsync(IDbConnection connection, IDbTransaction transaction)
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateBankAccountsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS BankAccounts
            (
                BankAccountId TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                BankName TEXT NULL,
                OpeningBalance NUMERIC NOT NULL DEFAULT 0,
                OpeningBalanceDate TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL
            );
            """
        );
    }

    private Task CreateBankStatementsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateBankStatementsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS BankStatements
            (
                BankStatementId TEXT PRIMARY KEY,
                BankAccountId TEXT NOT NULL,
                StatementStartDate TEXT NOT NULL,
                StatementEndDate TEXT NOT NULL,
                OpeningBalance NUMERIC NULL,
                ClosingBalance NUMERIC NOT NULL,
                StatementNumber TEXT NULL,
                Notes TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL,

                FOREIGN KEY (BankAccountId)
                    REFERENCES BankAccounts(BankAccountId)
            );
            """
        );
    }

    private Task CreateBankTransactionsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateBankTransactionsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS BankTransactions
            (
                BankTransactionId TEXT PRIMARY KEY,
                BankAccountId TEXT NOT NULL,
                BankStatementId TEXT NULL,
                ReceiptDate TEXT NOT NULL,
                Description TEXT NULL,
                MoneyIn NUMERIC NOT NULL DEFAULT 0,
                MoneyOut NUMERIC NOT NULL DEFAULT 0,
                BalanceAfterTransaction NUMERIC NULL,
                Reference TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL,

                FOREIGN KEY (BankAccountId)
                    REFERENCES BankAccounts(BankAccountId),

                FOREIGN KEY (BankStatementId)
                    REFERENCES BankStatements(BankStatementId),

                CHECK
                (
                    (MoneyIn > 0 AND MoneyOut = 0)
                    OR
                    (MoneyOut > 0 AND MoneyIn = 0)
                    OR
                    (MoneyIn = 0 AND MoneyOut = 0)
                )
            );
            """
        );
    }

    private Task CreateTransactionMatchesTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateTransactionMatchesTableAsync),
            """
            CREATE TABLE IF NOT EXISTS TransactionMatches
            (
                TransactionMatchId TEXT PRIMARY KEY,
                TransactionId TEXT NOT NULL,
                BankTransactionId TEXT NOT NULL,
                MatchedAt TEXT NOT NULL,
                Notes TEXT NULL,
                CreatedAt TEXT NOT NULL,
                DeletedAt TEXT NULL,

                FOREIGN KEY (TransactionId)
                    REFERENCES Transactions(TransactionId),

                FOREIGN KEY (BankTransactionId)
                    REFERENCES BankTransactions(BankTransactionId)
            );
            """
        );
    }

    private Task CreateTransactionMatchIndexesAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateTransactionMatchIndexesAsync),
            """
            CREATE UNIQUE INDEX IF NOT EXISTS
                UX_TransactionMatches_Transaction_Active
            ON TransactionMatches (TransactionId)
            WHERE DeletedAt IS NULL;

            CREATE UNIQUE INDEX IF NOT EXISTS
                UX_TransactionMatches_BankTransaction_Active
            ON TransactionMatches (BankTransactionId)
            WHERE DeletedAt IS NULL;
            """
        );
    }

    private Task CreateImportBatchesTableAsync(IDbConnection connection, IDbTransaction transaction)
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateImportBatchesTableAsync),
            """
            CREATE TABLE IF NOT EXISTS ImportBatches
            (
                ImportBatchId TEXT PRIMARY KEY,
                SourceFile TEXT NOT NULL,
                Status TEXT NOT NULL,
                Notes TEXT NULL,
                CreatedAt TEXT NOT NULL,
                CompletedAt TEXT NULL
            );
            """
        );
    }

    private Task CreateImportBatchRowsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateImportBatchRowsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS ImportBatchRows
            (
                ImportBatchRowId TEXT PRIMARY KEY,
                ImportBatchId TEXT NOT NULL,
                RowNumber INTEGER NOT NULL,
                EntityType TEXT NOT NULL,
                RawJson TEXT NOT NULL,
                ValidationErrors TEXT NULL,
                ImportedEntityId TEXT NULL,
                CreatedAt TEXT NOT NULL,

                FOREIGN KEY (ImportBatchId)
                    REFERENCES ImportBatches(ImportBatchId)
            );
            """
        );
    }

    private Task CreateApplicationSettingsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateApplicationSettingsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS ApplicationSettings
            (
                Key TEXT PRIMARY KEY,
                Value TEXT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """
        );
    }

    private Task CreateBackupRecordsTableAsync(IDbConnection connection, IDbTransaction transaction)
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateBackupRecordsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS BackupRecords
            (
                BackupRecordId TEXT PRIMARY KEY,
                FilePath TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                WasSuccessful INTEGER NOT NULL,
                Notes TEXT NULL
            );
            """
        );
    }

    private Task CreateTransactionDocumentsTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateTransactionDocumentsTableAsync),
            """
            CREATE TABLE IF NOT EXISTS TransactionDocuments
            (
                TransactionDocumentId TEXT PRIMARY KEY,
                TransactionId TEXT NOT NULL,
                FileName TEXT NOT NULL,
                MimeType TEXT NOT NULL,
                DocumentBlob BLOB NOT NULL,
                ThumbnailBlob BLOB NULL,
                DocumentType TEXT NOT NULL,
                UploadedAt TEXT NOT NULL,
                DeletedAt TEXT NULL,

                FOREIGN KEY (TransactionId)
                    REFERENCES Transactions(TransactionId)
            );
            """
        );
    }

    private Task CreateAuditLogEntriesTableAsync(
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return ExecuteScriptAsync(
            connection,
            transaction,
            nameof(CreateAuditLogEntriesTableAsync),
            """
            CREATE TABLE IF NOT EXISTS AuditLogEntries
            (
                AuditLogEntryId TEXT PRIMARY KEY,
                EntityType TEXT NOT NULL,
                EntityId TEXT NOT NULL,
                Action TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                OldValuesJson TEXT NULL,
                NewValuesJson TEXT NULL
            );
            """
        );
    }

    private async Task ExecuteScriptAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        string scriptName,
        string sql
    )
    {
        _logger.LogDebug("Executing database schema script {ScriptName}.", scriptName);

        await connection.ExecuteAsync(sql, transaction: transaction);
    }
}
