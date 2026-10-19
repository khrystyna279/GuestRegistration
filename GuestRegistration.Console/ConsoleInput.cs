namespace GuestRegistration.ConsoleApp;

internal static class ConsoleInput
{
    public static string Read(string prompt)
    {
        Console.Write(prompt + ": ");
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    public static DateTime ReadUtcDateTime(string prompt)
    {
        string text = Read(prompt + " (UTC, yyyy-MM-dd HH:mm:ss)");
        if (!DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out DateTime value))
            throw new ArgumentException("Дата й час мають бути у форматі yyyy-MM-dd HH:mm:ss.");
        return value;
    }

    public static int ReadInt(string prompt)
    {
        if (!int.TryParse(Read(prompt), out int value))
            throw new ArgumentException("Потрібно ввести ціле число.");
        return value;
    }

    public static int? ReadOptionalId(string prompt)
    {
        string text = Read(prompt + " (Enter — без користувача)");
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!int.TryParse(text, out int id) || id < 1)
            throw new ArgumentException("ID має бути додатним цілим числом.");
        return id;
    }

    public static bool ReadBool(string prompt)
    {
        string text = Read(prompt + " (1 — так, 0 — ні)");
        return text switch
        {
            "1" => true,
            "0" => false,
            _ => throw new ArgumentException("Введіть 1 або 0.")
        };
    }
}
