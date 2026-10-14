using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Exceptions;
using JobApplicationHelper.Services.Api;
using System.IO;

namespace JobApplicationHelper.Services.Experiences;

public class ExperiencesImportService
{
    private readonly ExperiencesApiClient experiencesApiClient;

    public ExperiencesImportService(ExperiencesApiClient experiencesApiClient)
    {
        this.experiencesApiClient = experiencesApiClient;
    }

    public async Task<ImportExperiencesResponse> ImportYamlFileAsync(string fullFilename, CancellationToken cancellationToken)
    {
        try
        {
            var filename = Path.GetFileName(fullFilename);

            await using var stream = File.OpenRead(fullFilename);

            var response = await experiencesApiClient.ImportAsync(stream, filename, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            throw new ImportExperiencesException($"Could not import file: {ex.Message}");
        }
    }
}
