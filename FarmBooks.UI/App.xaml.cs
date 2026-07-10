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
                        services.AddTransient<ExpenseDocumentRepository>();
                        services.AddTransient<ExpenseLineItemRepository>();
                        services.AddTransient<ExpenseMatchRepository>();
                        services.AddTransient<ExpenseRepository>();
                        services.AddTransient<ImportRepository>();
                        services.AddTransient<SettingsRepository>();

                        // Services
                        services.AddTransient<IExpenseService, ExpenseService>();
                        services.AddTransient<IAccountingCodeService, AccountingCodeService>();
                        services.AddTransient<IExpenseLineItemService, ExpenseLineItemService>();

                        services.AddTransient<AuditService>();
                        services.AddTransient<BackupService>();
                        services.AddTransient<BankService>();
                        services.AddTransient<DashboardService>();
                        services.AddTransient<ExpenseDocumentService>();
                        services.AddTransient<ExpenseMatchingService>();
                        services.AddTransient<ImportService>();
                        services.AddTransient<SettingsService>();

                        // Windows
                        services.AddSingleton<MainWindow>();

                        // Views
                        services.AddTransient<ExpensesView>();
                        services.AddTransient<AccountingCodesView>();

                        // ViewModels
                        services.AddTransient<ExpensesViewModel>();
                        services.AddTransient<ExpenseLineItemsViewModel>();
                        services.AddTransient<ExpenseListViewModel>();
                        services.AddTransient<ExpenseEditorViewModel>();
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
