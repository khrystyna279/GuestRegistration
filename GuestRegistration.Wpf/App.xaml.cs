using System.IO;
using System.Text.Json;
using System.Windows;
using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Services;
using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.Wpf.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GuestRegistration.Wpf;

public partial class App : Application
{
    private ServiceProvider? _services;
    protected override void OnStartup(StartupEventArgs args)
    {
        base.OnStartup(args);
        try
        {
            var services = new ServiceCollection();
            string? connection = Environment.GetEnvironmentVariable("GUEST_DB_CONNECTION");
            if (string.IsNullOrWhiteSpace(connection))
            {
                using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
                connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("GuestDatabase").GetString();
            }
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Не задано рядок підключення.");
            services.AddSingleton(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options);
            services.AddSingleton<IDataSessionFactory, DataSessionFactory>();
            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddSingleton<ISecretHasher, Pbkdf2SecretHasher>();
            services.AddSingleton<IAccountValidator, AccountValidator>();
            services.AddSingleton<UserSession>();
            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<RegistrationViewModel>();
            services.AddSingleton<RecoveryViewModel>();
            services.AddSingleton<ProfileViewModel>();
            services.AddSingleton<RestrictedViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
            _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
            MainWindow = _services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show("Не вдалося запустити програму. Перевірте appsettings.json.\n" + exception.Message, "Помилка запуску");
            Shutdown(1);
        }
    }
    protected override void OnExit(ExitEventArgs args) { _services?.Dispose(); base.OnExit(args); }
}
