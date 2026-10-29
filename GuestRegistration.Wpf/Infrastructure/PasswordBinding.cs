using System.Windows;
using System.Windows.Controls;

namespace GuestRegistration.Wpf.Infrastructure;

public static class PasswordBinding
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(PasswordBinding), new PropertyMetadata(false, EnabledChanged));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
        "Value", typeof(string), typeof(PasswordBinding), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChanged));
    public static bool GetEnabled(DependencyObject target) => (bool)target.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject target, bool value) => target.SetValue(EnabledProperty, value);
    public static string GetValue(DependencyObject target) => (string)target.GetValue(ValueProperty);
    public static void SetValue(DependencyObject target, string value) => target.SetValue(ValueProperty, value);
    private static void EnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not PasswordBox box) return;
        box.PasswordChanged -= PasswordChanged;
        if ((bool)args.NewValue) box.PasswordChanged += PasswordChanged;
    }
    private static void PasswordChanged(object sender, RoutedEventArgs args)
    {
        var box = (PasswordBox)sender;
        if (GetValue(box) != box.Password) box.SetCurrentValue(ValueProperty, box.Password);
    }
    private static void ValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is PasswordBox box && box.Password != (string?)args.NewValue) box.Password = (string?)args.NewValue ?? "";
    }
}
