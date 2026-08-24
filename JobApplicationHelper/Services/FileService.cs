using System.Text;
using System.IO;
using JobApplicationHelper.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.Services;

public class FileService
{
    private readonly string templateBasePath;
    private readonly string applicationBasePath;
    private readonly LocationService locationService;
    private readonly IExperienceBankImportService experienceBankService;
    private readonly ILogger<FileService> logger;
    private const string NotesFileName = "Notes.txt";
    private readonly string candidateFullName;

    private string GetTemplatePath(string templateName)
    {
        return Path.Combine(templateBasePath, templateName);
    }

    public FileService(IOptions<FileServiceOptions> options, IOptions<CandidateOptions> candidateOptions, LocationService locationService, IExperienceBankImportService experienceBankService, ILogger<FileService> logger)
    {
        // Options are validated on startup via DI (ValidateOnStart). The constructor is only used by DI,
        // so null-check guards are unnecessary. Use null-forgiving operator to satisfy nullable analysis.
        var opts = options.Value!;
        var candidateOpts = candidateOptions.Value!;
        this.candidateFullName = candidateOpts.CandidateFullName;

        this.templateBasePath = opts.TemplateBasePath;
        this.applicationBasePath = opts.ApplicationBasePath;
        this.locationService = locationService;
        this.experienceBankService = experienceBankService;
        this.logger = logger;
    }

    public string CreateApplicationDocuments(ApplicationFile applicationFile)
    {
        if (!Directory.Exists(applicationBasePath))
        {
            throw new DirectoryNotFoundException($"Application base path does not exist: {applicationBasePath}");
        }

        if (!Directory.Exists(templateBasePath))
        {
            throw new DirectoryNotFoundException($"Template base path does not exist: {templateBasePath}");
        }

        // Verify CV template mapping exists for the given country code
        var location = locationService.GetLocations().FirstOrDefault(l => l.CountryCode == applicationFile.CountryCode) 
            ?? throw new FileNotFoundException($"No location found for country code '{applicationFile.CountryCode}'.");
        
        string cvTemplatePath = GetTemplatePath(location.CVTemplate);
        if (!File.Exists(cvTemplatePath))
        {
            throw new FileNotFoundException($"CV template file not found: {cvTemplatePath}");
        }

        // If a cover letter is requested, verify its template exists before creating any folder
        string? coverLetterTemplatePath = null;
        if (applicationFile.IncludeCoverLetter)
        {
            coverLetterTemplatePath = GetTemplatePath(location.CoverLetterTemplate);
            if (!File.Exists(coverLetterTemplatePath))
            {
                throw new FileNotFoundException($"Cover letter template file not found: {coverLetterTemplatePath}");
            }
        }

        // All pre-checks passed — create application folder and copy files
        string folderName = $"{DateTime.Now:yyyy-MM-dd} {applicationFile.CompanyName} - {applicationFile.PositionTitle} ({(string.IsNullOrEmpty(applicationFile.City) ? "" : applicationFile.City + ", ")}{applicationFile.CountryCode})";
        string folderPath = Path.Combine(applicationBasePath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Copy CV template
        string cvDestinationPath = Path.Combine(folderPath, $"{candidateFullName} - CV.odt");
        File.Copy(cvTemplatePath, cvDestinationPath, overwrite: true);

        // Copy cover letter template if needed
        if (applicationFile.IncludeCoverLetter)
        {
            string coverLetterDestinationPath = Path.Combine(folderPath, $"{candidateFullName} - Cover Letter.odt");
            File.Copy(coverLetterTemplatePath!, coverLetterDestinationPath, overwrite: true);
        }

        // Create Notes.txt
        string notesFilePath = Path.Combine(folderPath, NotesFileName);
        string notesFileContents = $"Company: {applicationFile.CompanyName}\n" +
                                  $"Position: {applicationFile.PositionTitle}\n" +
                                  $"Location: {(string.IsNullOrEmpty(applicationFile.City) ? "" : applicationFile.City + ", ")}{applicationFile.CountryCode}\n" +
                                  $"URL: {applicationFile.URL}\n" +
                                  $"Date Created: {DateTime.Now:yyyy-MM-dd}\n";
        File.WriteAllText(notesFilePath, notesFileContents);

        return folderPath;
    }

    public string GetCVText(string countryCode)
    {
        var location = locationService.GetLocations().FirstOrDefault(l => l.CountryCode == countryCode)
            ?? throw new FileNotFoundException($"No location found for country code '{countryCode}'.");
        string cvTextPath = GetTemplatePath(location.CVText);
        if (!File.Exists(cvTextPath))
        {
            throw new FileNotFoundException($"CV text file not found: {cvTextPath}");
        }
        return File.ReadAllText(cvTextPath, Encoding.UTF8);
    }

    public void SaveDraftToNotes(string draft, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        string notesFilePath = Path.Combine(folderPath, NotesFileName);

        if (!File.Exists(notesFilePath))
        {
            throw new FileNotFoundException($"Notes file not found: {notesFilePath}");
        }

        string newline = Environment.NewLine;
        string prefix = newline + newline + "Cover letter:" + newline + newline;

        // Append the newline + draft entry using UTF8
        File.AppendAllText(notesFilePath, prefix + draft + newline, Encoding.UTF8);

#pragma warning disable CA1873 // Avoid potentially expensive logging
        logger.LogInformation("Appended draft to notes file: {NotesFilePath}", notesFilePath);
#pragma warning restore CA1873
    }

    public async Task<ExperienceBank> LoadExperienceBank()
    {
        string experienceBankPath = Path.Combine(templateBasePath, "experience.yaml");
        return await experienceBankService.ImportAsync(experienceBankPath);
    }

    public static void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        else
        {
            throw new Exception($"Folder does not exist: {folderPath}");
        }
    }
}
