using GuestRegistration.DAL.Data;
using GuestRegistration.DTO;
using GuestRegistration.DAL.Repositories;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.Tests;

public class TestDatabase : IDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _databaseName = "GuestRegistrationTests_" + Guid.NewGuid().ToString("N");
    public AppDbContext Context { get; }

    public TestDatabase()
    {
        string connectionString = Environment.GetEnvironmentVariable("GUEST_TEST_CONNECTION")
            ?? ReadConfiguredConnection();
        var admin = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master", Pooling = false };
        _adminConnectionString = admin.ConnectionString;
        using (var connection = new SqlConnection(_adminConnectionString))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            // The name contains only a fixed prefix and a generated GUID.
            command.CommandText = $"CREATE DATABASE [{_databaseName}]";
            command.ExecuteNonQuery();
        }
        var test = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = _databaseName, Pooling = false };
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(test.ConnectionString).Options;
        Context = new AppDbContext(options);
        try
        {
            // Use the same Database First SQL schema as the application.
            Context.Database.ExecuteSqlRaw(File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "sql", "01_CreateTables.sql")));
        }
        catch { Dispose(); throw; }
    }

    public UserDto AddUser(string login = "test_user")
    {
        var user = NewUser(login);
        return new UserRepository(Context).Create(user);
    }

    public static UserDto NewUser(string login = "test_user") => new()
    {
        Login = login, FirstName = "Test", LastName = "Student",
        PasswordHash = "demo_password_hash",
        RecoveryWordHash = "demo_recovery_hash",
        Gender = "Other", Address = "Test Street 1", Email = login + "@example.com",
        Phone = "+380991234567", CreatedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
    };

    public static BankCardDto NewCard(int userId) => new()
    {
        UserId = userId, CardHolder = "TEST STUDENT", Brand = "Visa", LastFourDigits = "1111",
        ExpirationMonth = 12, ExpirationYear = 2099
    };

    public static LoginAttemptDto NewAttempt(int? userId) => new()
    {
        UserId = userId, LoginEntered = "test_user",
        AttemptedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), IsSuccessful = true
    };

    private static string ReadConfiguredConnection()
    {
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.test.json")));
        return settings.RootElement.GetProperty("ConnectionStrings").GetProperty("TestDatabase").GetString()
            ?? throw new InvalidOperationException("Set the test database connection in appsettings.test.json.");
    }

    public void Dispose()
    {
        Context.Dispose();
        using var connection = new SqlConnection(_adminConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        // Only this test's own generated database is removed.
        command.CommandText = $"DROP DATABASE [{_databaseName}]";
        command.ExecuteNonQuery();
    }
}
