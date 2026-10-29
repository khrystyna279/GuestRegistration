using System.Text;
using GuestRegistration.ConsoleApp;
using GuestRegistration.ConsoleApp.Commands;
using GuestRegistration.ConsoleApp.Menu;
using GuestRegistration.DTO;
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
    Console.WriteLine("2. Для нової бази виконайте database/00–02, для наявної — 06_AddAuditColumns.sql.");
    Console.WriteLine("3. Перевірте Server та спосіб входу в appsettings.json.");
    Console.WriteLine("Деталі: " + exception.Message);
    return;
}
var connectionDetails = new SqlConnectionStringBuilder(connectionString);
Console.WriteLine($"SQL Server: {connectionDetails.DataSource}; база: {connectionDetails.InitialCatalog}");

var menu = new ConsoleMenu("Демонстрація DAL / CRUD", new IConsoleCommand[]
{
    new EntityMenu<UserDto>("1", "Users", new UserRepository(context), DtoInput.ReadUser, DtoOutput.PrintUser,
        "Картки також видаляться; історія входів залишиться без UserId."),
    new EntityMenu<BankCardDto>("2", "BankCards", new BankCardRepository(context), DtoInput.ReadCard, DtoOutput.PrintCard),
    new EntityMenu<LoginAttemptDto>("3", "LoginAttempts", new LoginAttemptRepository(context), DtoInput.ReadAttempt, DtoOutput.PrintAttempt)
}, "вихід");
bool running = true;
while (running)
{
    try
    {
        running = menu.RunOnce();
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
