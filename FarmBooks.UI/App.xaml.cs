using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using FarmBooks.Data.Database;
using FarmBooks.Data.Repositories;
using FarmBooks.Services;
using FarmBooks.UI.Infrastructure;
using FarmBooks.UI.ViewModels;
using FarmBooks.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmBooks.UI;

public partial class App : Application
{
    private IHost? _host;
    private ILogger<App>? _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo("en-ZA");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag))
            );

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FarmBooks",
                "Logs"
            );

            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddDebug();
                    logging.AddProvider(new FileLoggerProvider(logDirectory));
                })
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
                        services.AddTransient<
                            ITransactionLineItemService,
                            TransactionLineItemService
                        >();

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

            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            _logger.LogInformation("FarmBooks application started.");

            _logger.LogInformation("Initializing the FarmBooks database schema.");

            var schema = _host.Services.GetRequiredService<SchemaInitializer>();

            await schema.InitializeAsync();

            _logger.LogInformation("Database schema initialization completed.");

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "FarmBooks failed during application startup.");

            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FarmBooks",
                "Logs"
            );

            MessageBox.Show(
                $"""
                FarmBooks could not start.

                {ex.Message}

                Full details were written to:
                {logPath}
                """,
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

    private void App_DispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        _logger?.LogCritical(e.Exception, "Unhandled WPF dispatcher exception.");

        MessageBox.Show(
            $"""
            FarmBooks encountered an unexpected error.

            {e.Exception.Message}

            The full error was written to the FarmBooks log.
            """,
            "FarmBooks error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );

        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _logger?.LogCritical(
                exception,
                "Unhandled application-domain exception. Terminating: {IsTerminating}",
                e.IsTerminating
            );
        }
        else
        {
            _logger?.LogCritical(
                "Unknown unhandled application-domain exception. Terminating: {IsTerminating}",
                e.IsTerminating
            );
        }
    }

    private void TaskScheduler_UnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e
    )
    {
        _logger?.LogError(e.Exception, "Unobserved background task exception.");

        e.SetObserved();
    }
}
