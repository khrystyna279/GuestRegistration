namespace GuestRegistration.ConsoleApp;

internal static class ConsoleInput
{
    public static string Read(string prompt)
    {
        Console.Write(prompt + ": ");
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    public static string ReadSecret(string prompt)
    {
        if (Console.IsInputRedirected) return Read(prompt);
        Console.Write(prompt + ": ");
        string value = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return value;
            }
            if (key.Key == ConsoleKey.Backspace && value.Length > 0)
            {
                value = value[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                value += key.KeyChar;
                Console.Write('*');
            }
        }
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
