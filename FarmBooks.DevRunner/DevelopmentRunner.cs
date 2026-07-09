using FarmBooks.Data.Database;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;
using FarmBooks.Core.Models;

namespace FarmBooks.DevRunner;

public sealed class DevelopmentRunner
{
    private readonly ExpenseService _expenseService;
    private readonly ExpenseLineItemService _lineItemService;
    private readonly AccountingCodeService _codeService;
    private readonly DashboardService _dashboardService;
    private readonly BankService _bankService;
    private readonly ExpenseMatchingService _expenseMatchingService;
    private readonly ImportService _importService;
    private readonly SettingsService _settingsService;
    private readonly BackupService _backupService;
    private readonly ExpenseDocumentService _expenseDocumentService;
    private readonly AuditService _auditService;

    public DevelopmentRunner(
        ExpenseService expenseService,
        ExpenseLineItemService lineItemService,
        AccountingCodeService codeService,
        DashboardService dashboardService,
        BankService bankService,
        ExpenseMatchingService expenseMatchingService,
        ImportService importService,
        SettingsService settingsService,
        BackupService backupService,
        ExpenseDocumentService expenseDocumentService,
        AuditService auditService)
    {
        _expenseService = expenseService;
        _lineItemService = lineItemService;
        _codeService = codeService;
        _dashboardService = dashboardService;
        _bankService = bankService;
        _expenseMatchingService = expenseMatchingService;
        _importService = importService;
        _settingsService = settingsService;
        _backupService = backupService;
        _expenseDocumentService = expenseDocumentService;
        _auditService = auditService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("FarmBooks Development");
            Console.WriteLine("=====================");
            Console.WriteLine();
            Console.WriteLine("1. Create Test Expense");
            Console.WriteLine("2. List Expenses");
            Console.WriteLine("3. Update First Expense");
            Console.WriteLine("4. Delete First Expense");
            Console.WriteLine("5. Restore First Expense");
            Console.WriteLine("6. Show Dashboard");
            Console.WriteLine("7. Create Test Bank Data");
            Console.WriteLine("8. List Bank Transactions");
            Console.WriteLine("9. Match First Expense To First Bank Transaction");
            Console.WriteLine("10. Unmatch First Bank Transaction");
            Console.WriteLine("11. Match Using ExpenseMatches Table");
            Console.WriteLine("12. Create Test Import Batch");
            Console.WriteLine("13. List Import Batches");
            Console.WriteLine("15. Save Test Settings");
            Console.WriteLine("16. List Settings");
            Console.WriteLine("17. Backup Database");
            Console.WriteLine("18. List Backups");
            Console.WriteLine("19. Attach Test Document To First Expense");
            Console.WriteLine("20. List Documents For First Expense");
            Console.WriteLine("21. List Expense DTOs");
            Console.WriteLine("22. List Audit Log");
            Console.WriteLine("23. List All Accounting Codes");
            Console.WriteLine("24. Update First Accounting Code");
            Console.WriteLine("25. Disable First Accounting Code");
            Console.WriteLine("26. Reactivate First Accounting Code");
            Console.WriteLine();
            Console.WriteLine("0. Exit");
            Console.WriteLine();

            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":
                    await CreateTestExpense();
                    break;

                case "2":
                    await ListExpenses();
                    break;

                case "3":
                    await UpdateFirstExpense();
                    break;

                case "4":
                    await DeleteFirstExpense();
                    break;
                case "5":
                    await RestoreFirstExpense();
                    break;
                case "6":
                    await ShowDashboard();
                    break;
                case "7":
                    await CreateTestBankData();
                    break;
                case "8":
                    await ListBankTransactions();
                    break;
                case "9":
                    await MatchFirstExpenseToFirstBankTransaction();
                    break;
                case "10":
                    await UnmatchFirstBankTransaction();
                    break;
                case "11":
                    await MatchUsingExpenseMatchesTable();
                    break;
                case "12":
                    await CreateTestImportBatch();
                    break;
                case "13":
                    await ListImportBatches();
                    break;
                case "15":
                    await SaveTestSettings();
                    break;
                case "16":
                    await ListSettings();
                    break;
                case "17":
                    await BackupDatabase();
                    break;
                case "18":
                    await ListBackups();
                    break;
                case "19":
                    await AttachTestDocumentToFirstExpense();
                    break;
                case "20":
                    await ListDocumentsForFirstExpense();
                    break;
                case "21":
                    await ListExpenseDtos();
                    break;
                case "22":
                    await ListAuditLog();
                    break;
                case "23":
                    await ListAllAccountingCodes();
                    break;
                case "24":
                    await UpdateFirstAccountingCode();
                    break;
                case "25":
                    await DisableFirstAccountingCode();
                    break;
                case "26":
                    await ReactivateFirstAccountingCode();
                    break;
                case "0":
                    return;
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER...");
            Console.ReadLine();
        }
    }

    private async Task CreateTestExpense()
    {
        var codeId = await _codeService.CreateCodeAsync(
            "80",
            "Sheep Feed",
            "Feed for sheep");

        var expenseId = await _expenseService.CreateExpenseAsync(
            expenseDate: DateTime.Today,
            paidDate: DateTime.Today,
            sourceType: ExpenseSourceType.Receipt,
            documentNumber: "ABC123",
            businessName: "Farm Supply",
            description: "Feed Purchase",
            total: 250.00m,
            notes: null);

        await _lineItemService.AddLineItemAsync(
            expenseId,
            codeId,
            "Feed",
            150.00m);

        await _lineItemService.AddLineItemAsync(
            expenseId,
            codeId,
            "More Feed",
            100.00m);

        var status = await _expenseService.GetStatusAsync(expenseId);

        Console.WriteLine($"Expense Created");
        Console.WriteLine($"Status: {status}");
    }

    private async Task ListExpenses()
    {
        var expenses = await _expenseService.ListExpensesAsync();

        foreach (var expense in expenses)
        {
            var status = await _expenseService.GetStatusAsync(expense.ExpenseId);

            Console.WriteLine(
                $"{expense.ExpenseDate:d} | " +
                $"{expense.BusinessName,-20} | " +
                $"{expense.Total,8:C} | " +
                $"{status}");
        }
    }

    private async Task UpdateFirstExpense()
    {
        var expenses = await _expenseService.ListExpensesAsync();

        var expense = expenses.FirstOrDefault();

        if (expense == null)
        {
            Console.WriteLine("No Expenses.");
            return;
        }

        await _expenseService.UpdateExpenseAsync(
            expense.ExpenseId,
            expense.ExpenseDate,
            expense.PaidDate,
            expense.SourceType,
            expense.DocumentNumber,
            expense.BusinessName + " UPDATED",
            expense.Description,
            expense.Total,
            expense.Notes);

        Console.WriteLine("Updated.");
    }

    private async Task DeleteFirstExpense()
    {
        var expense = (await _expenseService.ListExpensesAsync())
            .FirstOrDefault();

        if (expense == null)
        {
            Console.WriteLine("No Expenses.");
            return;
        }

        await _expenseService.DeleteExpenseAsync(expense.ExpenseId);

        Console.WriteLine("Deleted.");
    }

    private async Task RestoreFirstExpense()
    {
        Console.WriteLine("Restore not implemented yet.");
    }

    private async Task ShowDashboard()
    {
        var summary = await _dashboardService.GetSummaryAsync();

        Console.WriteLine("Dashboard");
        Console.WriteLine("---------");
        Console.WriteLine($"Total Expenses:                {summary.TotalExpenseCount}");
        Console.WriteLine($"Needs Line Items:              {summary.NeedsLineItemsCount}");
        Console.WriteLine($"Needs Codes:                   {summary.NeedsCodesCount}");
        Console.WriteLine($"Needs Review:                  {summary.NeedsReviewCount}");
        Console.WriteLine($"Complete:                      {summary.CompleteCount}");
        Console.WriteLine();
        Console.WriteLine($"Total Bank Transactions:       {summary.TotalBankTransactionCount}");
        Console.WriteLine($"Unmatched Expenses:            {summary.UnmatchedExpenseCount}");
        Console.WriteLine($"Unmatched Bank Transactions:   {summary.UnmatchedBankTransactionCount}");
    }

    private async Task CreateTestBankData()
    {
        var accountId = await _bankService.CreateAccountAsync(
            "Farm Cheque Account",
            "Farm Bank",
            1000.00m,
            DateTime.Today.AddMonths(-1));

        var statementId = await _bankService.CreateStatementAsync(
            accountId,
            DateTime.Today.AddDays(-30),
            DateTime.Today,
            1000.00m,
            750.00m,
            "TEST-001",
            "Test statement");

        await _bankService.CreateTransactionAsync(
            accountId,
            statementId,
            DateTime.Today,
            "Farm Supply Store",
            0m,
            250.00m,
            750.00m,
            "TEST PAYMENT");

        Console.WriteLine("Created test bank account, statement, and transaction.");
    }

    private async Task ListBankTransactions()
    {
        var transactions = await _bankService.ListTransactionsAsync();

        Console.WriteLine("Bank Transactions");
        Console.WriteLine("-----------------");

        foreach (var transaction in transactions)
        {
            var amount = transaction.MoneyIn > 0
                ? transaction.MoneyIn
                : -transaction.MoneyOut;

            var matched = transaction.ExpenseId == null
                ? "Unmatched"
                : "Matched";

            Console.WriteLine(
                $"{transaction.TransactionDate:d} | " +
                $"{transaction.Description,-25} | " +
                $"{amount,10:C} | " +
                $"{matched}");
        }
    }
    private async Task MatchFirstExpenseToFirstBankTransaction()
    {
        var expenses = await _expenseService.ListExpensesAsync();
        var transactions = await _bankService.ListTransactionsAsync();

        var expense = expenses.FirstOrDefault();
        var transaction = transactions.FirstOrDefault(x => x.ExpenseId == null);

        if (expense == null)
        {
            Console.WriteLine("No Expenses found.");
            return;
        }

        if (transaction == null)
        {
            Console.WriteLine("No unmatched bank transactions found.");
            return;
        }

        await _bankService.MatchExpenseAsync(
            transaction.BankTransactionId,
            expense.ExpenseId);

        Console.WriteLine("Matched first Expense to first unmatched bank transaction.");
    }

    private async Task UnmatchFirstBankTransaction()
    {
        var transactions = await _bankService.ListTransactionsAsync();

        var transaction = transactions.FirstOrDefault(x => x.ExpenseId != null);

        if (transaction == null)
        {
            Console.WriteLine("No matched bank transactions found.");
            return;
        }

        await _bankService.UnmatchExpenseAsync(transaction.BankTransactionId);

        Console.WriteLine("Unmatched first matched bank transaction.");
    }

    private async Task MatchUsingExpenseMatchesTable()
    {
        var expenses = await _expenseService.ListExpensesAsync();
        var transactions = await _bankService.ListTransactionsAsync();

        var expense = expenses.FirstOrDefault();
        var transaction = transactions.FirstOrDefault();

        if (expense == null)
        {
            Console.WriteLine("No Expenses found.");
            return;
        }

        if (transaction == null)
        {
            Console.WriteLine("No bank transactions found.");
            return;
        }

        var matchId = await _expenseMatchingService.MatchAsync(
            expense.ExpenseId,
            transaction.BankTransactionId,
            "Test match");

        Console.WriteLine($"Created Expense match: {matchId}");
    }

    private async Task CreateTestImportBatch()
    {
        var batchId = await _importService.CreateTestImportBatchAsync(
            "MomSpreadsheet.xlsb");

        Console.WriteLine($"Created import batch: {batchId}");
    }

    private async Task ListImportBatches()
    {
        var batches = await _importService.ListBatchesAsync();

        foreach (var batch in batches)
        {
            Console.WriteLine(
                $"{batch.CreatedAt:g} | {batch.SourceFile} | {batch.Status} | {batch.ImportBatchId}");

            var rows = await _importService.ListRowsAsync(batch.ImportBatchId);

            foreach (var row in rows)
            {
                var status = string.IsNullOrWhiteSpace(row.ValidationErrors)
                    ? "Valid"
                    : "Needs Review";

                Console.WriteLine(
                    $"    Row {row.RowNumber}: {row.EntityType} | {status}");

                if (!string.IsNullOrWhiteSpace(row.ValidationErrors))
                {
                    Console.WriteLine($"        {row.ValidationErrors}");
                }
            }
        }
    }

    private async Task SaveTestSettings()
    {
        await _settingsService.SaveAsync("FarmName", "Double D Farm");
        await _settingsService.SaveAsync("Currency", "ZAR");
        await _settingsService.SaveAsync("DefaultVatRate", "15");

        Console.WriteLine("Saved test settings.");
    }

    private async Task ListSettings()
    {
        var settings = await _settingsService.ListAsync();

        Console.WriteLine("Settings");
        Console.WriteLine("--------");

        foreach (var setting in settings)
        {
            Console.WriteLine($"{setting.Key}: {setting.Value}");
        }
    }

    private async Task BackupDatabase()
    {
        var path = await _backupService.BackupDatabaseAsync();

        Console.WriteLine("Backup created:");
        Console.WriteLine(path);

        Console.WriteLine();
        Console.WriteLine($"Valid: {_backupService.ValidateBackup(path)}");
    }

    private async Task ListBackups()
    {
        var backups = await _backupService.ListBackupsAsync();

        Console.WriteLine("Backups");
        Console.WriteLine("-------");

        foreach (var backup in backups)
        {
            var status = backup.WasSuccessful ? "Success" : "Failed";

            Console.WriteLine(
                $"{backup.CreatedAt:g} | {status} | {backup.FilePath}");

            if (!string.IsNullOrWhiteSpace(backup.Notes))
                Console.WriteLine($"    {backup.Notes}");
        }
    }

    private async Task AttachTestDocumentToFirstExpense()
    {
        var expenses = await _expenseService.ListExpensesAsync();
        var expense = expenses.FirstOrDefault();

        if (expense is null)
        {
            Console.WriteLine("No expenses found.");
            return;
        }

        var testFilePath = Path.Combine(
            Path.GetTempPath(),
            "FarmBooks-Test-Document.txt");

        await File.WriteAllTextAsync(
            testFilePath,
            $"Test document for expense {expense.ExpenseId} created at {DateTime.Now}");

        var documentId = await _expenseDocumentService.AttachDocumentAsync(
            expense.ExpenseId,
            testFilePath,
            "Test");

        Console.WriteLine($"Attached document: {documentId}");
    }

    private async Task ListDocumentsForFirstExpense()
    {
        var expenses = await _expenseService.ListExpensesAsync();
        var expense = expenses.FirstOrDefault();

        if (expense is null)
        {
            Console.WriteLine("No expenses found.");
            return;
        }

        var documents = await _expenseDocumentService.ListForExpenseAsync(expense.ExpenseId);

        Console.WriteLine($"Documents for expense: {expense.BusinessName}");
        Console.WriteLine("----------------------------------------");

        foreach (var document in documents)
        {
            Console.WriteLine(
                $"{document.UploadedAt:g} | " +
                $"{document.DocumentType} | " +
                $"{document.FileName} | " +
                $"{document.MimeType} | " +
                $"{document.DocumentBlob.Length} bytes");
        }
    }

    private async Task ListExpenseDtos()
    {
        var expenses = await _expenseService.GetExpenseListAsync();

        Console.WriteLine("Expense DTOs");
        Console.WriteLine("------------");

        foreach (var expense in expenses)
        {
            Console.WriteLine(
                $"{expense.ExpenseDate:d} | " +
                $"{expense.SourceType,-13} | " +
                $"{expense.BusinessName,-25} | " +
                $"{expense.Total,10:C} | " +
                $"{expense.Status,-15} | " +
                $"Matched: {expense.IsMatched,-5} | " +
                $"Lines: {expense.LineItemCount} | " +
                $"Docs: {expense.DocumentCount}");
        }
    }

    private async Task ListAuditLog()
    {
        var entries = await _auditService.ListRecentAsync();

        Console.WriteLine("Audit Log");
        Console.WriteLine("---------");

        foreach (var entry in entries)
        {
            Console.WriteLine(
                $"{entry.Timestamp:g} | " +
                $"{entry.EntityType} | " +
                $"{entry.Action} | " +
                $"{entry.EntityId}");
        }
    }

    private async Task ListAllAccountingCodes()
    {
        var codes = await _codeService.ListAllCodesAsync();

        Console.WriteLine("Accounting Codes");
        Console.WriteLine("----------------");

        foreach (var code in codes)
        {
            var status = code.IsActive ? "Active" : "Inactive";

            Console.WriteLine(
                $"{code.Code,-8} | {code.Name,-30} | {status}");
        }
    }

    private async Task UpdateFirstAccountingCode()
    {
        var codes = await _codeService.ListAllCodesAsync();
        var code = codes.FirstOrDefault();

        if (code is null)
        {
            Console.WriteLine("No accounting codes found.");
            return;
        }

        await _codeService.UpdateCodeAsync(
            code.CodeId,
            code.Code,
            code.Name + " UPDATED",
            code.Description,
            code.IsActive);

        Console.WriteLine($"Updated code {code.Code}.");
    }

    private async Task DisableFirstAccountingCode()
    {
        var codes = await _codeService.ListAllCodesAsync();
        var code = codes.FirstOrDefault(x => x.IsActive);

        if (code is null)
        {
            Console.WriteLine("No active accounting codes found.");
            return;
        }

        await _codeService.DisableCodeAsync(code.CodeId);

        Console.WriteLine($"Disabled code {code.Code}.");
    }

    private async Task ReactivateFirstAccountingCode()
    {
        var codes = await _codeService.ListAllCodesAsync();
        var code = codes.FirstOrDefault(x => !x.IsActive);

        if (code is null)
        {
            Console.WriteLine("No inactive accounting codes found.");
            return;
        }

        await _codeService.ReactivateCodeAsync(code.CodeId);

        Console.WriteLine($"Reactivated code {code.Code}.");
    }
}