using System.Text.RegularExpressions;
using ReverseMarkdown;

namespace JobApplicationHelper.Services;

public sealed class JobPostingConverter
{
    private readonly Converter _converter;

    public JobPostingConverter()
    {
        var config = new Config
        {
            Flavor = Config.MarkdownFlavor.Default,

            Formatting =
            {
                RemoveComments = true
            },
            Links =
            {
                SmartHref = true
            }
        };

        config.Preprocess
            .RemoveScripts()
            .RemoveStyles()
            .Remove("img")
            .Unwrap("span, font");

        _converter = new Converter(config);
    }

    public string ConvertHtmlToMarkdown(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        html = HtmlClipboardHelper.ExtractFragment(html);

        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var markdown = _converter.Convert(html);

        return NormalizeMarkdown(markdown);
    }

    private static string NormalizeMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        markdown = markdown
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        markdown = markdown.Replace('\u00A0', ' ');

        markdown = string.Join(
            "\n",
            markdown
                .Split('\n')
                .Select(line => line.TrimEnd()));

        markdown = Regex.Replace(markdown, @"\n{3,}", "\n\n");

        return markdown.Trim();
    }
}
