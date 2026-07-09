using FarmBooks.DevRunner;
using FarmBooks.Data.Database;
using FarmBooks.Data.Repositories;
using FarmBooks.Data.Services;

var databasePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "FarmBooks",
    "farmbooks.sqlite");

Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

var db = new DbConnectionFactory(databasePath);

var schema = new SchemaInitializer(db);
await schema.InitializeAsync();

var accountingCodeRepository = new AccountingCodeRepository(db);
var expenseRepository = new ExpenseRepository(db);
var expenseLineItemRepository = new ExpenseLineItemRepository(db);
var dashboardRepository = new DashboardRepository(db);
var bankRepository = new BankRepository(db);
var expenseMatchRepository = new ExpenseMatchRepository(db);
var importRepository = new ImportRepository(db);
var settingsRepository = new SettingsRepository(db);
var backupRepository = new BackupRepository(db);
var expenseDocumentRepository = new ExpenseDocumentRepository(db);

var accountingCodeService = new AccountingCodeService(accountingCodeRepository);
var expenseService = new ExpenseService(expenseRepository, expenseLineItemRepository, expenseMatchRepository, expenseDocumentRepository, accountingCodeRepository);
var expenseLineItemService = new ExpenseLineItemService(expenseLineItemRepository);
var dashboardService = new DashboardService(dashboardRepository);
var bankService = new BankService(bankRepository);
var expenseMatchingService = new ExpenseMatchingService(expenseMatchRepository);
var importService = new ImportService(importRepository);
var settingsService = new SettingsService(settingsRepository);
var backupService = new BackupService(databasePath, backupRepository, settingsService);
var expenseDocumentService = new ExpenseDocumentService(
    expenseDocumentRepository,
    expenseRepository);

var runner = new DevelopmentRunner(
    expenseService,
    expenseLineItemService,
    accountingCodeService,
    dashboardService,
    bankService,
    expenseMatchingService,
    importService,
    settingsService,
    backupService,
    expenseDocumentService);

await runner.RunAsync();