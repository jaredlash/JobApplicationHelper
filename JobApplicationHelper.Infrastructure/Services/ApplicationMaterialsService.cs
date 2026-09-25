using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Domain.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace JobApplicationHelper.Infrastructure.Services;

public class ApplicationMaterialsService : IApplicationMaterialsService
{
    private const string NotesFileName = "Notes.txt";
    private readonly ApplicationDocumentOptions documentOptions;
    private readonly CandidateContentOptions candidateOptions;

    public ApplicationMaterialsService(IOptions<ApplicationDocumentOptions> documentOptions, IOptions<CandidateContentOptions> candidateOptions)
    {
        this.documentOptions = documentOptions.Value;
        this.candidateOptions = candidateOptions.Value;
    }

    public JobApplicationId CreateApplicationMaterials(ApplicationFile application)
    {
        if (!Directory.Exists(documentOptions.ApplicationsBasePath))
        {
            throw new DirectoryNotFoundException($"Application base path does not exist: {documentOptions.ApplicationsBasePath}");
        }

        if (!Directory.Exists(documentOptions.TemplateBasePath))
        {
            throw new DirectoryNotFoundException($"Template base path does not exist: {documentOptions.TemplateBasePath}");
        }

        // Verify document configuration mapping exists for the given country code
        var documentConfig = documentOptions.Documents.FirstOrDefault(l => l.CountryCode == application.CountryCode)
            ?? throw new FileNotFoundException($"No document configuration found for country code '{application.CountryCode}'.");

        var cvTemplatePath = Path.Combine(documentOptions.TemplateBasePath, documentConfig.CvTemplate);
        if (!File.Exists(cvTemplatePath))
        {
            throw new FileNotFoundException($"CV template file not found: {cvTemplatePath}");
        }

        // If a cover letter is requested, verify its template exists before creating any folder
        string? coverLetterTemplatePath = null;
        if (application.IncludeCoverLetter)
        {
            coverLetterTemplatePath = Path.Combine(documentOptions.TemplateBasePath, documentConfig.CoverLetterTemplate);
            if (!File.Exists(coverLetterTemplatePath))
            {
                throw new FileNotFoundException($"Cover letter template file not found: {coverLetterTemplatePath}");
            }
        }

        // All pre-checks passed — create application folder and copy files
        string folderName = $"{DateTime.Now:yyyy-MM-dd} {application.CompanyName} - {application.PositionTitle} ({(string.IsNullOrEmpty(application.City) ? "" : application.City + ", ")}{application.CountryCode})";
        string folderPath = Path.Combine(documentOptions.ApplicationsBasePath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Copy CV template
        string cvDestinationPath = Path.Combine(folderPath, $"{candidateOptions.CandidateFullName} - CV.odt");
        File.Copy(cvTemplatePath, cvDestinationPath, overwrite: true);

        // Copy cover letter template if needed
        if (application.IncludeCoverLetter)
        {
            string coverLetterDestinationPath = Path.Combine(folderPath, $"{candidateOptions.CandidateFullName} - Cover Letter.odt");
            File.Copy(coverLetterTemplatePath!, coverLetterDestinationPath, overwrite: true);
        }

        // Create Notes.txt
        string notesFilePath = Path.Combine(folderPath, NotesFileName);
        string notesFileContents = $"Company: {application.CompanyName}\n" +
                                  $"Position: {application.PositionTitle}\n" +
                                  $"Location: {(string.IsNullOrEmpty(application.City) ? "" : application.City + ", ")}{application.CountryCode}\n" +
                                  $"URL: {application.URL}\n" +
                                  $"Date Created: {DateTime.Now:yyyy-MM-dd}\n\n" +
                                  $"Job Posting:\n" +
                                  application.JobPosting;

        File.WriteAllText(notesFilePath, notesFileContents);

        return new JobApplicationId(folderPath);
    }

    public string GetApplicationFolder(JobApplicationId applicationId) => applicationId.Value;

    public void SaveCoverLetterDraft(JobApplicationId applicationId, string draft)
    {
        var folderPath = GetApplicationFolder(applicationId);

        string notesFilePath = Path.Combine(folderPath, NotesFileName);

        if (!File.Exists(notesFilePath))
        {
            throw new FileNotFoundException($"Notes file not found: {notesFilePath}");
        }

        string newline = Environment.NewLine;
        string prefix = newline + newline + "Cover letter:" + newline + newline;

        // Append the newline + draft entry using UTF8
        File.AppendAllText(notesFilePath, prefix + draft + newline, Encoding.UTF8);
    }
}
