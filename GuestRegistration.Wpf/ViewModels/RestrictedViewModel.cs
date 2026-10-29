using GuestRegistration.Wpf.Infrastructure;

namespace GuestRegistration.Wpf.ViewModels;

public sealed class RestrictedViewModel : ObservableObject
{
    private string _displayName = "";
    public string DisplayName { get => _displayName; set => Set(ref _displayName, value); }
}
