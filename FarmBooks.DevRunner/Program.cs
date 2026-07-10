using FarmBooks.Data.Database;
using FarmBooks.Data.Repositories;
using FarmBooks.DevRunner;
using FarmBooks.Services;
using Microsoft.Extensions.DependencyInjection;

var databasePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "FarmBooks",
    "farmbooks.sqlite"
);

Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

var services = new ServiceCollection();

services.AddSingleton(new DbConnectionFactory(databasePath));
services.AddSingleton(new FarmBooks.Core.Models.DatabaseOptions { DatabasePath = databasePath });

services.AddTransient<SchemaInitializer>();

// Repositories
services.AddTransient<AccountingCodeRepository>();
services.AddTransient<AuditRepository>();
services.AddTransient<BackupRepository>();
services.AddTransient<BankRepository>();
services.AddTransient<DashboardRepository>();
services.AddTransient<ExpenseDocumentRepository>();
services.AddTransient<ExpenseLineItemRepository>();
services.AddTransient<ExpenseMatchRepository>();
services.AddTransient<ExpenseRepository>();
services.AddTransient<ImportRepository>();
services.AddTransient<SettingsRepository>();

// Services
services.AddTransient<AccountingCodeService>();
services.AddTransient<AuditService>();
services.AddTransient<BackupService>();
services.AddTransient<BankService>();
services.AddTransient<DashboardService>();
services.AddTransient<ExpenseDocumentService>();
services.AddTransient<ExpenseLineItemService>();
services.AddTransient<ExpenseMatchingService>();
services.AddTransient<ExpenseService>();
services.AddTransient<ImportService>();
services.AddTransient<SettingsService>();

services.AddTransient<DevelopmentRunner>();

var serviceProvider = services.BuildServiceProvider();

var schema = serviceProvider.GetRequiredService<SchemaInitializer>();
await schema.InitializeAsync();

var runner = serviceProvider.GetRequiredService<DevelopmentRunner>();
await runner.RunAsync();
