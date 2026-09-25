using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using System.Text;

namespace JobApplicationHelper.Infrastructure.Services;

public class CandidateContentProvider : ICandidateContentProvider
{
    private readonly CandidateContentOptions candidateContentOptions;
    private readonly ApplicationDocumentOptions documentOptions;

    public CandidateContentProvider(CandidateContentOptions candidateContentOptions, ApplicationDocumentOptions documentOptions)
    {
        this.candidateContentOptions = candidateContentOptions;
        this.documentOptions = documentOptions;
    }

    public async Task<CandidateContent> GetAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var cvTextFilename = candidateContentOptions.CvTextPathsByCountryCode
            .FirstOrDefault(
                x => string.Equals(
                    x.CountryCode,
                    countryCode,
                    StringComparison.OrdinalIgnoreCase))?.Path
            ?? throw new InvalidOperationException($"No CV text is configured for country code '{countryCode}'.");

        var cvTextPath = Path.Combine(documentOptions.TemplateBasePath, cvTextFilename);

        if (!File.Exists(cvTextPath))
        {
            throw new FileNotFoundException($"CV text file not found: {cvTextPath}", cvTextPath);
        }

        var candidateContent = new CandidateContent
        {
            CvText = await File.ReadAllTextAsync(cvTextPath, Encoding.UTF8)
        };
        return candidateContent;
    }
}
