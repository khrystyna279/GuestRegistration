using System.Windows;
using GuestRegistration.Wpf.ViewModels;

namespace GuestRegistration.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel) { InitializeComponent(); DataContext = viewModel; }
}
