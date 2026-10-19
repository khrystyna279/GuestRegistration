using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Models;
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
            ?? "Server=localhost;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5";
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

    public User AddUser(string login = "test_user")
    {
        var user = NewUser(login);
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    public static User NewUser(string login = "test_user") => new()
    {
        Login = login, FirstName = "Test", LastName = "Student",
        PasswordHash = "demo_password_hash",
        RecoveryWordHash = "demo_recovery_hash",
        Gender = "Other", Address = "Test Street 1", Email = login + "@example.com",
        Phone = "+380991234567", CreatedAtUtc = DateTime.UtcNow
    };

    public static BankCard NewCard(int userId) => new()
    {
        UserId = userId, CardHolder = "TEST STUDENT", Brand = "Visa", LastFourDigits = "1111",
        ExpirationMonth = 12, ExpirationYear = 2099
    };

    public static LoginAttempt NewAttempt(int? userId) => new()
    {
        UserId = userId, LoginEntered = "test_user", AttemptedAtUtc = DateTime.UtcNow, IsSuccessful = true
    };

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
