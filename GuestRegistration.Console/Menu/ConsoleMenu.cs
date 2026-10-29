using GuestRegistration.ConsoleApp.Commands;

namespace GuestRegistration.ConsoleApp.Menu;

internal sealed class ConsoleMenu
{
    private readonly string _title;
    private readonly string _exitLabel;
    private readonly IReadOnlyList<IConsoleCommand> _commands;

    public ConsoleMenu(string title, IEnumerable<IConsoleCommand> commands, string exitLabel = "назад")
    {
        _title = title;
        _commands = commands.ToList();
        _exitLabel = exitLabel;
    }

    public bool RunOnce()
    {
        Console.WriteLine("\n" + _title);
        foreach (var command in _commands) Console.WriteLine($"{command.Key} — {command.Description}");
        Console.WriteLine("0 — " + _exitLabel);
        string key = ConsoleInput.Read("Вибір");
        if (key == "0") return false;
        var selected = _commands.FirstOrDefault(command => command.Key == key)
            ?? throw new ArgumentException("Невідомий пункт меню.");
        selected.Execute();
        return true;
    }
}
