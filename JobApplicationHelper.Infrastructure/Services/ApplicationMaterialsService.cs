using JobApplicationHelper.Application.Configuration;
using JobApplicationHelper.Application.Repositories;
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
    private readonly IJobApplicationRepository jobApplicationRepository;

    public ApplicationMaterialsService(
        IOptions<ApplicationDocumentOptions> documentOptions,
        IOptions<CandidateContentOptions> candidateOptions,
        IJobApplicationRepository jobApplicationRepository)
    {
        this.documentOptions = documentOptions.Value;
        this.candidateOptions = candidateOptions.Value;
        this.jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationId> CreateApplicationMaterialsAsync(ApplicationFile application, CancellationToken cancellationToken = default)
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
        var applicationId = new JobApplicationId(Guid.NewGuid());
        var createdAt = DateTime.UtcNow;

        string folderName =
            $"{createdAt.ToLocalTime():yyyy-MM-dd} " +
            $"{application.CompanyName} - {application.PositionTitle} " +
            $"({(string.IsNullOrEmpty(application.City) ? "" : application.City + ", ")}" +
            $"{application.CountryCode})";
        string folderPath = Path.Combine(documentOptions.ApplicationsBasePath, folderName);

        var jobApplication = new JobApplication
        {
            Id = applicationId,
            CountryCode = application.CountryCode,
            IncludeCoverLetter = application.IncludeCoverLetter,
            CompanyName = application.CompanyName,
            PositionTitle = application.PositionTitle,
            URL = application.URL,
            City = application.City,
            JobPosting = application.JobPosting,
            CreatedAt = createdAt,
            ApplicationFolder = folderPath
        };

        await jobApplicationRepository.AddAsync(jobApplication, cancellationToken);


        try
        {
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
            string notesFileContents = $"Company: {application.CompanyName}{Environment.NewLine}" +
                                      $"Position: {application.PositionTitle}{Environment.NewLine}" +
                                      $"Location: {(string.IsNullOrEmpty(application.City) ? "" : application.City + ", ")}{application.CountryCode}{Environment.NewLine}" +
                                      $"URL: {application.URL}{Environment.NewLine}" +
                                      $"Date Created: {jobApplication.CreatedAt.ToLocalTime():yyyy-MM-dd}{Environment.NewLine}{Environment.NewLine}" +
                                      $"Job Posting:{Environment.NewLine}" +
                                      application.JobPosting;

            File.WriteAllText(notesFilePath, notesFileContents);
        }
        catch
        {
            await jobApplicationRepository.DeleteAsync(applicationId, CancellationToken.None);

            throw;
        }

        return jobApplication.Id;
    }

    public async Task<string> GetApplicationFolderAsync(
        JobApplicationId applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await jobApplicationRepository.GetAsync(applicationId, cancellationToken);

        return application?.ApplicationFolder
            ?? throw new KeyNotFoundException($"Job application '{applicationId.Value}' was not found.");
    }

    public async Task SaveCoverLetterDraftAsync(JobApplicationId applicationId, string draft, CancellationToken cancellationToken = default)
    {
        var folderPath = await GetApplicationFolderAsync(applicationId, cancellationToken);

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
