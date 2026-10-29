using System.ComponentModel;
using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IAccountService _accounts;
    private readonly LoginViewModel _login;
    private readonly RegistrationViewModel _registration;
    private readonly RecoveryViewModel _recovery;
    private readonly ProfileViewModel _profile;
    private readonly RestrictedViewModel _restricted;
    private ObservableObject _page;
    public ObservableObject Page { get => _page; private set => Set(ref _page, value); }
    public bool IsBusy => _login.IsBusy || _registration.IsBusy || _recovery.IsBusy || _profile.IsBusy;
    public bool IsSignedIn => _accounts.CurrentUser != null;
    public string SessionCaption => _accounts.CurrentUser?.DisplayName ?? "Гостьовий доступ";
    public RelayCommand ShowLoginCommand { get; }
    public RelayCommand ShowRegistrationCommand { get; }
    public RelayCommand ShowRecoveryCommand { get; }
    public RelayCommand LogoutCommand { get; }

    public MainViewModel(IAccountService accounts, LoginViewModel login, RegistrationViewModel registration,
        RecoveryViewModel recovery, ProfileViewModel profile, RestrictedViewModel restricted)
    {
        _accounts = accounts; _login = login; _registration = registration; _recovery = recovery;
        _profile = profile; _restricted = restricted; _page = login;
        ShowLoginCommand = new RelayCommand(() => Navigate(_login), CanBrowse);
        ShowRegistrationCommand = new RelayCommand(() => Navigate(_registration), CanBrowse);
        ShowRecoveryCommand = new RelayCommand(() => Navigate(_recovery), CanBrowse);
        LogoutCommand = new RelayCommand(Logout, () => IsSignedIn && !IsBusy);
        _login.Authenticated += SignedIn;
        _registration.Registered += name => { _login.Prepare(name, "Реєстрація завершена. Увійдіть зі своїм паролем."); Page = _login; };
        _recovery.Recovered += name => { _login.Prepare(name, "Пароль змінено. Тепер можна увійти з новим паролем."); Page = _login; };
        foreach (var vm in new ObservableObject[] { _login, _registration, _recovery, _profile }) vm.PropertyChanged += ChildChanged;
    }

    private bool CanBrowse() => !IsSignedIn && !IsBusy;
    private void Navigate(ObservableObject page)
    {
        _login.Prepare(); _registration.Clear(); _recovery.Clear(); Page = page;
    }
    private void SignedIn(AuthenticatedUser user)
    {
        if (user.HasGuestAccess) { Page = _profile; _profile.RefreshCommand.Execute(null); }
        else { _restricted.DisplayName = user.DisplayName; Page = _restricted; }
        RefreshSession();
    }
    private void Logout()
    {
        _accounts.Logout(); _profile.Clear(); _restricted.DisplayName = "";
        Navigate(_login); RefreshSession();
    }
    private void ChildChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "IsBusy") { Raise(nameof(IsBusy)); RefreshSession(); }
    }
    private void RefreshSession()
    {
        Raise(nameof(IsSignedIn)); Raise(nameof(SessionCaption));
        ShowLoginCommand.Refresh(); ShowRegistrationCommand.Refresh(); ShowRecoveryCommand.Refresh(); LogoutCommand.Refresh();
    }
}
