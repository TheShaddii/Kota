using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Kota.Data;
using Kota.Data.Repositories;
using Kota.Services.Authentication;
using Kota.Services.Items;
using Kota.Services.Locations;
using Kota.App.ViewModels;
using Kota.App.Services;
using System.IO;

namespace Kota.App
{
    public partial class App : Application
    {
        private IHost? _host;

        public IHost Host => _host!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Data layer
                    services.AddSingleton<IConnectionFactory, AccessConnectionFactory>();
                    services.AddSingleton<IDatabaseInitializer, AccessDatabaseInitializer>();

                    // Repositories
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<ISiteRepository, SiteRepository>();
                    services.AddScoped<IBuildingRepository, BuildingRepository>();
                    services.AddScoped<IItemRepository, ItemRepository>();
                    services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
                    services.AddScoped<IAuditLogRepository, AuditLogRepository>();

                    // Services
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IItemService, ItemService>();
                    services.AddScoped<ILocationService, LocationService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<ICurrentUserService, CurrentUserService>();

                    // ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<ItemDialogViewModel>();
                    services.AddTransient<SettingsViewModel>();

                    // Windows
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            // Fast database initialization - only when needed
            await QuickDatabaseSetup();

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        private async Task QuickDatabaseSetup()
        {
            try
            {
                var settingsService = _host!.Services.GetRequiredService<ISettingsService>();
                var connectionFactory = _host.Services.GetRequiredService<IConnectionFactory>();

                // Set up database path
                var dbPath = await settingsService.GetDatabasePathAsync();
                if (string.IsNullOrEmpty(dbPath))
                {
                    dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KotaData", "inventory.accdb");
                    await settingsService.SetDatabasePathAsync(dbPath);
                }

                // Always set the connection path
                connectionFactory.SetDatabasePath(dbPath);

                // Only do full initialization if database doesn't exist OR is empty
                if (!File.Exists(dbPath) || new FileInfo(dbPath).Length < 10000) // Less than 10KB means probably empty
                {
                    var databaseInitializer = _host.Services.GetRequiredService<IDatabaseInitializer>();
                    await databaseInitializer.InitializeDatabaseAsync(dbPath);
                }
                // If database exists and is substantial size, skip initialization entirely
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database setup error: {ex.Message}", "Startup Error");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}