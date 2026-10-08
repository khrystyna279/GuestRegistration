using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Data;

public static class DatabaseInitializer
{
    public static void ValidateDatabase(AppDbContext context)
    {
        // Database First: SQL scripts create the database, not the application.
        if (!context.Database.CanConnect())
            throw new InvalidOperationException("Не вдалося підключитися до SQL Server. Перевірте сервер і ConnectionStrings у appsettings.json.");
        _ = context.Users.Count();
        _ = context.BankCards.Count();
        _ = context.LoginAttempts.Count();
    }
}
