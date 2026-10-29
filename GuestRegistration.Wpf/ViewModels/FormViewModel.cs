using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public abstract class FormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly HashSet<string> _touched = new();
    private Dictionary<string, string> _errors = new();
    private bool _showAll, _suppress, _busy;
    private string _message = "";
    public bool IsBusy { get => _busy; private set => Set(ref _busy, value); }
    public string Message { get => _message; protected set => Set(ref _message, value); }
    public bool HasErrors => _errors.Count != 0;
    public string this[string name] => _errors.GetValueOrDefault(name, "");
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public IEnumerable GetErrors(string? name) => name != null && _errors.TryGetValue(name, out string? error) ? new[] { error } : Array.Empty<string>();
    protected abstract IReadOnlyDictionary<string, string> Validate();
    protected void Field(ref string field, string value, [CallerMemberName] string name = "")
    {
        if (!Set(ref field, value ?? "", name) || _suppress) return;
        _touched.Add(name); Message = ""; RefreshErrors();
    }
    protected bool ValidateAll() { _showAll = true; RefreshErrors(); return !HasErrors; }
    private void RefreshErrors() => ReplaceErrors(Validate().Where(pair => _showAll || _touched.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value));
    private void ReplaceErrors(Dictionary<string, string> errors)
    {
        var keys = _errors.Keys.Union(errors.Keys).ToArray();
        _errors = errors;
        foreach (string key in keys) ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
        Raise(nameof(HasErrors)); Raise("Item[]");
    }
    protected void Reset(Action clear)
    {
        _suppress = true;
        try { clear(); }
        finally { _suppress = false; _showAll = false; _touched.Clear(); ReplaceErrors(new()); Message = ""; }
    }
    protected async Task RunOperation(Func<AccountResult> operation, Action<AccountResult> success)
    {
        IsBusy = true; Message = "";
        try
        {
            var result = await Task.Run(operation);
            if (result.IsSuccess) success(result);
            else
            {
                ReplaceErrors(result.Errors.Where(pair => pair.Key != "").ToDictionary(pair => pair.Key, pair => pair.Value));
                Message = result.Errors.GetValueOrDefault("", "Перевірте позначені поля.");
            }
        }
        finally { IsBusy = false; }
    }
    protected void OnFailure(Exception exception) => Message = exception is UnauthorizedAccessException
        ? exception.Message : "Не вдалося виконати операцію. Перевірте SQL Server і підключення в appsettings.json.";
}
