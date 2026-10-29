using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profiles;
    private UserProfile? _profile;
    private string _message = "";
    private bool _busy;
    public UserProfile? Profile { get => _profile; private set => Set(ref _profile, value); }
    public string Message { get => _message; private set => Set(ref _message, value); }
    public bool IsBusy { get => _busy; private set => Set(ref _busy, value); }
    public AsyncCommand RefreshCommand { get; }
    public ProfileViewModel(IProfileService profiles)
    {
        _profiles = profiles;
        RefreshCommand = new AsyncCommand(Load, exception => Message = exception is UnauthorizedAccessException
            ? exception.Message : "Не вдалося завантажити профіль. Перевірте підключення до SQL Server.");
    }
    public void Clear() { Profile = null; Message = ""; }
    private async Task Load()
    {
        Clear(); IsBusy = true;
        try { Profile = await Task.Run(_profiles.GetCurrentProfile); }
        finally { IsBusy = false; }
    }
}
