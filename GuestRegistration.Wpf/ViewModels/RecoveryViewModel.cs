using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class RecoveryViewModel : FormViewModel
{
    private readonly IAccountService _accounts;
    private readonly IAccountValidator _validator;
    private string _Login = "";
    public string Login { get => _Login; set => Field(ref _Login, value); }
    private string _RecoveryWord = "";
    public string RecoveryWord { get => _RecoveryWord; set => Field(ref _RecoveryWord, value); }
    private string _Password = "";
    public string Password { get => _Password; set => Field(ref _Password, value); }
    private string _ConfirmPassword = "";
    public string ConfirmPassword { get => _ConfirmPassword; set => Field(ref _ConfirmPassword, value); }
    public AsyncCommand SubmitCommand { get; }
    public event Action<string>? Recovered;
    public RecoveryViewModel(IAccountService accounts, IAccountValidator validator)
    {
        _accounts = accounts; _validator = validator;
        SubmitCommand = new AsyncCommand(Submit, OnFailure);
    }
    private RecoveryInput Input() => new(Login, RecoveryWord, Password, ConfirmPassword);
    protected override IReadOnlyDictionary<string, string> Validate() => _validator.Recovery(Input());
    public void Clear() => Reset(() => { Login = RecoveryWord = Password = ConfirmPassword = ""; });
    private async Task Submit()
    {
        if (!ValidateAll()) return;
        var input = Input();
        await RunOperation(() => _accounts.RecoverPassword(input), _ => { Clear(); Recovered?.Invoke(input.Login.Trim().ToLowerInvariant()); });
    }
}
