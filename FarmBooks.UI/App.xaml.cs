using System.IO;
using System.Windows;
using FarmBooks.Data.Database;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;
using FarmBooks.UI.ViewModels;
using FarmBooks.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FarmBooks.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    (context, services) =>
                    {
                        var databasePath = Path.Combine(
                            Environment.GetFolderPath(
                                Environment.SpecialFolder.LocalApplicationData
                            ),
                            "FarmBooks",
                            "farmbooks.sqlite"
                        );

                        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

                        services.AddSingleton(new DbConnectionFactory(databasePath));

                        services.AddSingleton(
                            new FarmBooks.Core.Models.DatabaseOptions
                            {
                                DatabasePath = databasePath,
                            }
                        );

                        services.AddTransient<SchemaInitializer>();

                        // Repositories
                        services.AddTransient<AccountingCodeRepository>();
                        services.AddTransient<AuditRepository>();
                        services.AddTransient<BackupRepository>();
                        services.AddTransient<BankRepository>();
                        services.AddTransient<DashboardRepository>();
                        services.AddTransient<TransactionDocumentRepository>();
                        services.AddTransient<TransactionLineItemRepository>();
                        services.AddTransient<TransactionMatchRepository>();
                        services.AddTransient<TransactionRepository>();
                        services.AddTransient<ImportRepository>();
                        services.AddTransient<SettingsRepository>();

                        // Services
                        services.AddTransient<ITransactionService, TransactionService>();
                        services.AddTransient<IAccountingCodeService, AccountingCodeService>();
                        services.AddTransient<ITransactionLineItemService, TransactionLineItemService>();

                        services.AddTransient<AuditService>();
                        services.AddTransient<BackupService>();
                        services.AddTransient<BankService>();
                        services.AddTransient<DashboardService>();
                        services.AddTransient<TransactionDocumentService>();
                        services.AddTransient<TransactionMatchingService>();
                        services.AddTransient<ImportService>();
                        services.AddTransient<SettingsService>();

                        // Windows
                        services.AddSingleton<MainWindow>();

                        // Views
                        services.AddTransient<TransactionsView>();
                        services.AddTransient<AccountingCodesView>();

                        // ViewModels
                        services.AddTransient<TransactionsViewModel>();
                        services.AddTransient<TransactionLineItemsViewModel>();
                        services.AddTransient<TransactionListViewModel>();
                        services.AddTransient<TransactionEditorViewModel>();
                        services.AddTransient<AccountingCodesViewModel>();
                    }
                )
                .Build();

            await _host.StartAsync();

            var schema = _host.Services.GetRequiredService<SchemaInitializer>();
            await schema.InitializeAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "FarmBooks startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
