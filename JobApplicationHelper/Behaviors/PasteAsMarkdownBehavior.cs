using System.Windows;
using System.Windows.Controls;
using JobApplicationHelper.Services;

namespace JobApplicationHelper.Behaviors;

public static class PasteAsMarkdownBehavior
{
    private static readonly JobPostingConverter Converter = new();

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(PasteAsMarkdownBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        if (e.NewValue is true)
        {
            DataObject.AddPastingHandler(textBox, OnPasting);
        }
        else
        {
            DataObject.RemovePastingHandler(textBox, OnPasting);
        }
    }

    private static void OnPasting(
        object sender,
        DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (!e.DataObject.GetDataPresent(DataFormats.Html))
            return;

        if (e.DataObject.GetData(DataFormats.Html) is not string html)
            return;

        if (string.IsNullOrWhiteSpace(html))
            return;

        string markdown;

        try
        {
            markdown = Converter.ConvertHtmlToMarkdown(html);
        }
        catch
        {
            // If conversion fails, don't interfere with the normal
            // WPF paste operation.
            return;
        }

        if (string.IsNullOrWhiteSpace(markdown))
            return;

        // Prevent WPF from inserting the browser's HTML/plain-text
        // representation.
        e.CancelCommand();

        textBox.SelectedText = markdown;
    }
}