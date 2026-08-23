using System.Text;
using System.Text.RegularExpressions;

namespace JobApplicationHelper.Services;

public static class HtmlClipboardHelper
{
    public static string ExtractFragment(string clipboardHtml)
    {
        if (string.IsNullOrWhiteSpace(clipboardHtml))
            return string.Empty;

        // Most browsers include explicit fragment markers.
        const string startMarker = "<!--StartFragment-->";
        const string endMarker = "<!--EndFragment-->";

        int startMarkerIndex = clipboardHtml.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);

        int endMarkerIndex = clipboardHtml.IndexOf(endMarker, StringComparison.OrdinalIgnoreCase);

        if (startMarkerIndex >= 0 && endMarkerIndex > startMarkerIndex)
        {
            int start = startMarkerIndex + startMarker.Length;

            return clipboardHtml.Substring(start, endMarkerIndex - start);
        }

        // Some clipboard producers don't include the markers but do include
        // StartFragment/EndFragment byte offsets.
        if (TryExtractUsingOffsets(clipboardHtml, out var fragment))
            return fragment;

        // Finally, just return the supplied HTML.
        return clipboardHtml;
    }

    private static bool TryExtractUsingOffsets(string clipboardHtml, out string fragment)
    {
        fragment = string.Empty;

        var startMatch = Regex.Match(
            clipboardHtml,
            @"(?im)^StartFragment:(\d+)\s*$");

        var endMatch = Regex.Match(
            clipboardHtml,
            @"(?im)^EndFragment:(\d+)\s*$");

        if (!startMatch.Success || !endMatch.Success)
            return false;

        if (!int.TryParse(startMatch.Groups[1].Value, out int start))
            return false;

        if (!int.TryParse(endMatch.Groups[1].Value, out int end))
            return false;

        if (start < 0 || end <= start)
            return false;

        // Clipboard offsets are byte offsets, normally UTF-8.
        byte[] bytes = Encoding.UTF8.GetBytes(clipboardHtml);

        if (start >= bytes.Length || end > bytes.Length)
            return false;

        fragment = Encoding.UTF8.GetString(bytes, start, end - start);

        return true;
    }
}