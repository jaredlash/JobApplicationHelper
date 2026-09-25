namespace JobApplicationHelper.Application.Configuration;

public sealed record ApplicationDocumentConfiguration(
    string CountryCode,
    string CvTemplate,
    string CoverLetterTemplate);