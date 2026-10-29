using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class LoginViewModel : FormViewModel
{
    private readonly IAccountService _accounts;
    private readonly IAccountValidator _validator;
    private string _Login = "";
    public string Login { get => _Login; set => Field(ref _Login, value); }
    private string _Password = "";
    public string Password { get => _Password; set => Field(ref _Password, value); }
    public AsyncCommand SubmitCommand { get; }
    public event Action<AuthenticatedUser>? Authenticated;
    public LoginViewModel(IAccountService accounts, IAccountValidator validator)
    {
        _accounts = accounts; _validator = validator;
        SubmitCommand = new AsyncCommand(Submit, OnFailure);
    }
    protected override IReadOnlyDictionary<string, string> Validate() => _validator.Login(new(Login, Password));
    public void Prepare(string login = "", string notice = "") { Reset(() => { Login = login; Password = ""; }); Message = notice; }
    private async Task Submit()
    {
        if (!ValidateAll()) return;
        var input = new LoginInput(Login, Password);
        await RunOperation(() => _accounts.Login(input), result =>
        {
            Prepare();
            Authenticated?.Invoke(result.User!);
        });
    }
}
