using JobApplicationHelper.Models;
using JobApplicationHelper.Serialization;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JobApplicationHelper.Services;

public sealed class YamlExperienceBankImportService : IExperienceBankImportService
{
    private readonly IDeserializer _deserializer;

    public YamlExperienceBankImportService()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new ExperienceTypeYamlConverter())
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<ExperienceBank> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The experience bank file could not be found.",
                filePath);
        }

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var reader = new StreamReader(stream);

        var yaml = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new InvalidDataException("The experience bank file is empty.");
        }

        try
        {
            var experienceBank = _deserializer.Deserialize<ExperienceBank>(yaml)
                ?? throw new InvalidDataException("The experience bank could not be deserialized.");

            Validate(experienceBank, filePath);

            return experienceBank;
        }
        catch (YamlException ex)
        {
            throw new InvalidDataException($"The experience bank contains invalid YAML: {ex.Message}", ex);
        }
    }

    private static void Validate(ExperienceBank bank, string filePath)
    {
        if (bank.Version <= 0)
        {
            throw new InvalidDataException($"The experience bank '{filePath}' has an invalid version.");
        }

        if (bank.Experiences is null)
        {
            throw new InvalidDataException($"The experience bank '{filePath}' does not contain an experiences collection.");
        }

        var duplicateIds = bank.Experiences
            .Where(e => !string.IsNullOrWhiteSpace(e.Id))
            .GroupBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            throw new InvalidDataException("The experience bank contains duplicate experience IDs: " +
                string.Join(", ", duplicateIds));
        }

        for (var index = 0; index < bank.Experiences.Count; index++)
        {
            var experience = bank.Experiences[index];

            if (string.IsNullOrWhiteSpace(experience.Id))
            {
                throw new InvalidDataException($"Experience at index {index} does not have an ID.");
            }

            if (string.IsNullOrWhiteSpace(experience.Title))
            {
                throw new InvalidDataException($"Experience '{experience.Id}' does not have a title.");
            }

            if (experience.Skills is null)
            {
                throw new InvalidDataException($"Experience '{experience.Id}' has no skills collection.");
            }

            if (experience.Evidence is null)
            {
                throw new InvalidDataException($"Experience '{experience.Id}' has no evidence collection.");
            }
        }
    }
}
