using System.Text.Json;

namespace GuestRegistration.ConsoleApp;

internal static class ConnectionSettings
{
    public static string ReadConnectionString()
    {
        string? connectionString = Environment.GetEnvironmentVariable("GUEST_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connectionString)) return connectionString;
        string assemblyDirectory = Path.GetDirectoryName(typeof(ConnectionSettings).Assembly.Location)!;
        string settingsPath = Path.Combine(assemblyDirectory, "appsettings.json");
        using var settings = JsonDocument.Parse(File.ReadAllText(settingsPath));
        return settings.RootElement.GetProperty("ConnectionStrings").GetProperty("GuestDatabase").GetString()
            ?? throw new InvalidOperationException("Не задано рядок підключення.");
    }
}
