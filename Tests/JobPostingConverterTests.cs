using JobApplicationHelper.Services;

namespace Tests;

public class JobPostingConverterTests
{
    [Fact]
    public void ConvertHtmlToMarkdown_RemovesImages()
    {
        var converter = new JobPostingConverter();

        const string html = """
            <h1>Software Engineer</h1>
            <p>Join our team.</p>
            <img src="data:image/png;base64,THIS_IS_NOT_A_REAL_IMAGE" />
            <p>We offer great benefits.</p>
        """;

        var markdown = converter.ConvertHtmlToMarkdown(html);

        Assert.Contains("# Software Engineer", markdown);
        Assert.Contains("Join our team.", markdown);
        Assert.Contains("We offer great benefits.", markdown);
        Assert.DoesNotContain("data:image", markdown);
        Assert.DoesNotContain("THIS_IS_NOT_A_REAL_IMAGE", markdown);
    }
}
