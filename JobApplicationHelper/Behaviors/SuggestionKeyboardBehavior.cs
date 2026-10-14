
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JobApplicationHelper.Behaviors;

public static class SuggestionKeyboardBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SuggestionKeyboardBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty HasSuggestionsProperty =
        DependencyProperty.RegisterAttached(
            "HasSuggestions",
            typeof(bool),
            typeof(SuggestionKeyboardBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty MoveUpCommandProperty =
        DependencyProperty.RegisterAttached(
            "MoveUpCommand",
            typeof(ICommand),
            typeof(SuggestionKeyboardBehavior));

    public static readonly DependencyProperty MoveDownCommandProperty =
        DependencyProperty.RegisterAttached(
            "MoveDownCommand",
            typeof(ICommand),
            typeof(SuggestionKeyboardBehavior));

    public static readonly DependencyProperty AcceptCommandProperty =
        DependencyProperty.RegisterAttached(
            "AcceptCommand",
            typeof(ICommand),
            typeof(SuggestionKeyboardBehavior));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static bool GetHasSuggestions(DependencyObject obj) => (bool)obj.GetValue(HasSuggestionsProperty);

    public static void SetHasSuggestions(DependencyObject obj, bool value) => obj.SetValue(HasSuggestionsProperty, value);

    public static ICommand? GetMoveUpCommand(DependencyObject obj) => (ICommand?)obj.GetValue(MoveUpCommandProperty);

    public static void SetMoveUpCommand(DependencyObject obj, ICommand? value) => obj.SetValue(MoveUpCommandProperty, value);

    public static ICommand? GetMoveDownCommand(DependencyObject obj) => (ICommand?)obj.GetValue(MoveDownCommandProperty);

    public static void SetMoveDownCommand(DependencyObject obj, ICommand? value) => obj.SetValue(MoveDownCommandProperty, value);

    public static ICommand? GetAcceptCommand(DependencyObject obj) => (ICommand?)obj.GetValue(AcceptCommandProperty);

    public static void SetAcceptCommand(DependencyObject obj, ICommand? value) => obj.SetValue(AcceptCommandProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            textBox.PreviewKeyDown += OnPreviewKeyDown;
        }
        else
        {
            textBox.PreviewKeyDown -= OnPreviewKeyDown;
        }
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox || !GetHasSuggestions(textBox))
        {
            return;
        }

        var command = e.Key switch
        {
            Key.Up => GetMoveUpCommand(textBox),
            Key.Down => GetMoveDownCommand(textBox),
            Key.Enter => GetAcceptCommand(textBox),
            _ => null
        };

        if (command is null || !command.CanExecute(null))
        {
            return;
        }

        command.Execute(null);
        e.Handled = true;
    }
}
