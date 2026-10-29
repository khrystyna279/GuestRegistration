using System.Windows.Input;

namespace GuestRegistration.Wpf.Infrastructure;

public sealed class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Action<Exception> _onError;
    private bool _running;
    public AsyncCommand(Func<Task> execute, Action<Exception> onError) { _execute = execute; _onError = onError; }
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running;
    public async void Execute(object? parameter) => await ExecuteAsync();
    public async Task ExecuteAsync()
    {
        if (_running) return;
        _running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _execute(); }
        catch (Exception exception) { _onError(exception); }
        finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
    }
}
