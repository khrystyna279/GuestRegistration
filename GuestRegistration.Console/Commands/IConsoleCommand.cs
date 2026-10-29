namespace GuestRegistration.ConsoleApp.Commands;

internal interface IConsoleCommand
{
    string Key { get; }
    string Description { get; }
    void Execute();
}
