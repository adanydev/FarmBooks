using Dapper;

namespace FarmBooks.Data.Database;

public sealed class SchemaInitializer
{
    private readonly DbConnectionFactory _db;

    public SchemaInitializer(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        using var connection = _db.CreateConnection();

        await connection.ExecuteAsync("""
        CREATE TABLE IF NOT EXISTS Expenses (
            ExpenseId TEXT PRIMARY KEY,
            ExpenseDate TEXT NOT NULL,
            PaidDate TEXT NULL,
            SourceType INTEGER NOT NULL,
            DocumentNumber TEXT NULL,
            BusinessName TEXT NULL,
            Description TEXT NULL,
            Total NUMERIC NOT NULL CHECK (Total >= 0),
            VATC NUMERIC NULL,
            VATS NUMERIC NULL,
            Notes TEXT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            DeletedAt TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS AccountingCodes (
            CodeId TEXT PRIMARY KEY,
            Code TEXT NOT NULL,
            Name TEXT NOT NULL,
            Description TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS ExpenseLineItems (
            ExpenseLineItemId TEXT PRIMARY KEY,
            ExpenseId TEXT NOT NULL,
            CodeId TEXT NULL,
            Description TEXT NULL,
            Total NUMERIC NOT NULL CHECK (Total >= 0),
            VATTreatment TEXT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            DeletedAt TEXT NULL,

            FOREIGN KEY (ExpenseId) REFERENCES Expenses(ExpenseId),
            FOREIGN KEY (CodeId) REFERENCES AccountingCodes(CodeId)
        );

        CREATE TABLE IF NOT EXISTS BankAccounts (
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

        CREATE TABLE IF NOT EXISTS BankStatements (
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

            FOREIGN KEY (BankAccountId) REFERENCES BankAccounts(BankAccountId)
        );

        CREATE TABLE IF NOT EXISTS BankTransactions (
            BankTransactionId TEXT PRIMARY KEY,
            BankAccountId TEXT NOT NULL,
            BankStatementId TEXT NULL,
            TransactionDate TEXT NOT NULL,
            Description TEXT NULL,
            MoneyIn NUMERIC NOT NULL DEFAULT 0,
            MoneyOut NUMERIC NOT NULL DEFAULT 0,
            BalanceAfterTransaction NUMERIC NULL,
            Reference TEXT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            DeletedAt TEXT NULL,

            FOREIGN KEY (BankAccountId) REFERENCES BankAccounts(BankAccountId),
            FOREIGN KEY (BankStatementId) REFERENCES BankStatements(BankStatementId),

            CHECK (
                (MoneyIn > 0 AND MoneyOut = 0)
                OR
                (MoneyOut > 0 AND MoneyIn = 0)
                OR
                (MoneyIn = 0 AND MoneyOut = 0)
            )
        );

        CREATE TABLE IF NOT EXISTS ExpenseMatches (
            ExpenseMatchId TEXT PRIMARY KEY,
            ExpenseId TEXT NOT NULL,
            BankTransactionId TEXT NOT NULL,
            MatchedAt TEXT NOT NULL,
            Notes TEXT NULL,
            CreatedAt TEXT NOT NULL,
            DeletedAt TEXT NULL,

            FOREIGN KEY (ExpenseId) REFERENCES Expenses(ExpenseId),
            FOREIGN KEY (BankTransactionId) REFERENCES BankTransactions(BankTransactionId)
        );

        CREATE UNIQUE INDEX IF NOT EXISTS UX_ExpenseMatches_Expense_Active
        ON ExpenseMatches (ExpenseId)
        WHERE DeletedAt IS NULL;

        CREATE UNIQUE INDEX IF NOT EXISTS UX_ExpenseMatches_BankTransaction_Active
        ON ExpenseMatches (BankTransactionId)
        WHERE DeletedAt IS NULL;

        CREATE TABLE IF NOT EXISTS ImportBatches (
            ImportBatchId TEXT PRIMARY KEY,
            SourceFile TEXT NOT NULL,
            Status TEXT NOT NULL,
            Notes TEXT NULL,
            CreatedAt TEXT NOT NULL,
            CompletedAt TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS ImportBatchRows (
            ImportBatchRowId TEXT PRIMARY KEY,
            ImportBatchId TEXT NOT NULL,
            RowNumber INTEGER NOT NULL,
            EntityType TEXT NOT NULL,
            RawJson TEXT NOT NULL,
            ValidationErrors TEXT NULL,
            ImportedEntityId TEXT NULL,
            CreatedAt TEXT NOT NULL,

            FOREIGN KEY (ImportBatchId) REFERENCES ImportBatches(ImportBatchId)
        );

        CREATE TABLE IF NOT EXISTS ApplicationSettings (
            Key TEXT PRIMARY KEY,
            Value TEXT NULL,
            UpdatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS BackupRecords (
            BackupRecordId TEXT PRIMARY KEY,
            FilePath TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            WasSuccessful INTEGER NOT NULL,
            Notes TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS ExpenseDocuments (
            ExpenseDocumentId TEXT PRIMARY KEY,
            ExpenseId TEXT NOT NULL,
            FileName TEXT NOT NULL,
            MimeType TEXT NOT NULL,
            DocumentBlob BLOB NOT NULL,
            ThumbnailBlob BLOB NULL,
            DocumentType TEXT NOT NULL,
            UploadedAt TEXT NOT NULL,
            DeletedAt TEXT NULL,

            FOREIGN KEY (ExpenseId) REFERENCES Expenses(ExpenseId)
        );
        """);
    }
}