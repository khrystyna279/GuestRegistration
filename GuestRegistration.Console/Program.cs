using System.Text;
using GuestRegistration.ConsoleApp;
using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

string connectionString = ConnectionSettings.ReadConnectionString();

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
using var context = new AppDbContext(options);
try
{
    DatabaseInitializer.ValidateDatabase(context);
}
catch (Exception exception) when (exception is SqlException or InvalidOperationException)
{
    Console.WriteLine("Не вдалося відкрити базу GuestRegistrationCourse.");
    Console.WriteLine("1. Підключіться до SQL Server у SSMS.");
    Console.WriteLine("2. Виконайте скрипти database/00, 01 та 02 за інструкцією.");
    Console.WriteLine("3. Перевірте Server та спосіб входу в appsettings.json.");
    Console.WriteLine("Деталі: " + exception.Message);
    return;
}
var connectionDetails = new SqlConnectionStringBuilder(connectionString);
Console.WriteLine($"SQL Server: {connectionDetails.DataSource}; база: {connectionDetails.InitialCatalog}");

var dalMenu = new DalMenu(new UserRepository(context), new BankCardRepository(context),
    new LoginAttemptRepository(context));
bool running = true;
while (running)
{
    Console.WriteLine("\n=== Демонстрація DAL / CRUD ===");
    try
    {
        running = dalMenu.Run();
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine("Помилка введення: " + exception.Message);
    }
    catch (DbUpdateException)
    {
        Console.WriteLine("База відхилила зміну: перевірте унікальність логіна/e-mail, UserId та обмеження полів.");
    }
    catch (EndOfStreamException)
    {
        running = false;
    }
    finally
    {
        // A failed save must not affect the next menu operation.
        context.ChangeTracker.Clear();
    }
}
