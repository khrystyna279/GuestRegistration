using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class RegistrationViewModel : FormViewModel
{
    private readonly IAccountService _accounts;
    private readonly IAccountValidator _validator;
    private string _Login = "";
    public string Login { get => _Login; set => Field(ref _Login, value); }
    private string _FirstName = "";
    public string FirstName { get => _FirstName; set => Field(ref _FirstName, value); }
    private string _LastName = "";
    public string LastName { get => _LastName; set => Field(ref _LastName, value); }
    private string _Password = "";
    public string Password { get => _Password; set => Field(ref _Password, value); }
    private string _ConfirmPassword = "";
    public string ConfirmPassword { get => _ConfirmPassword; set => Field(ref _ConfirmPassword, value); }
    private string _RecoveryWord = "";
    public string RecoveryWord { get => _RecoveryWord; set => Field(ref _RecoveryWord, value); }
    private string _Gender = "";
    public string Gender { get => _Gender; set => Field(ref _Gender, value); }
    private string _Address = "";
    public string Address { get => _Address; set => Field(ref _Address, value); }
    private string _Email = "";
    public string Email { get => _Email; set => Field(ref _Email, value); }
    private string _Phone = "";
    public string Phone { get => _Phone; set => Field(ref _Phone, value); }
    private string _CardNumber = "";
    public string CardNumber { get => _CardNumber; set => Field(ref _CardNumber, value); }
    private string _CardHolder = "";
    public string CardHolder { get => _CardHolder; set => Field(ref _CardHolder, value); }
    private string _ExpirationMonth = "";
    public string ExpirationMonth { get => _ExpirationMonth; set => Field(ref _ExpirationMonth, value); }
    private string _ExpirationYear = "";
    public string ExpirationYear { get => _ExpirationYear; set => Field(ref _ExpirationYear, value); }
    public AsyncCommand SubmitCommand { get; }
    public event Action<string>? Registered;
    public RegistrationViewModel(IAccountService accounts, IAccountValidator validator)
    {
        _accounts = accounts; _validator = validator;
        SubmitCommand = new AsyncCommand(Submit, OnFailure);
    }
    private RegistrationInput Input() => new() { Login = Login, FirstName = FirstName, LastName = LastName, Password = Password, ConfirmPassword = ConfirmPassword, RecoveryWord = RecoveryWord, Gender = Gender, Address = Address, Email = Email, Phone = Phone, CardNumber = CardNumber, CardHolder = CardHolder, ExpirationMonth = ExpirationMonth, ExpirationYear = ExpirationYear };
    protected override IReadOnlyDictionary<string, string> Validate() => _validator.Registration(Input());
    public void Clear() => Reset(() => { Login = FirstName = LastName = Password = ConfirmPassword = RecoveryWord = Gender = Address = Email = Phone = CardNumber = CardHolder = ExpirationMonth = ExpirationYear = ""; });
    private async Task Submit()
    {
        if (!ValidateAll()) return;
        var input = Input();
        await RunOperation(() => _accounts.Register(input), _ => { Clear(); Registered?.Invoke(input.Login.Trim().ToLowerInvariant()); });
    }
}
