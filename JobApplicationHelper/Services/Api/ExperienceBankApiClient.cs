using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace JobApplicationHelper.Services.Api;

public sealed class ExperienceBankApiClient
{
    private readonly HttpClient _httpClient;

    public ExperienceBankApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var httpResponse = await _httpClient.GetAsync("api/experiences", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var experiences = await httpResponse.Content.ReadFromJsonAsync<List<ExperienceDto>>(cancellationToken)
            ?? throw new InvalidOperationException("The experience API returned an empty response.");

        return experiences.Select(e => new Experience
        {
            Id = e.Id,
            Title = e.Title,
            Type = Enum.Parse<ExperienceType>(
                e.Type,
                ignoreCase: true),
            Organization = e.Organization,
            DateRange = e.DateRange is null
                ? null
                : new DateRange
                {
                    Start = e.DateRange.Start is null
                        ? null
                        : new PartialDate
                        {
                            Year = e.DateRange.Start.Year,
                            Month = e.DateRange.Start.Month,
                            Day = e.DateRange.Start.Day
                        },
                    End = e.DateRange.End is null
                        ? null
                        : new PartialDate
                        {
                            Year = e.DateRange.End.Year,
                            Month = e.DateRange.End.Month,
                            Day = e.DateRange.End.Day
                        }
                },
            Summary = e.Summary,
            Skills = e.Skills.ToList(),
            Evidence = e.Evidence.ToList(),
            Contexts = e.Contexts.ToList(),
            Links = e.Links.ToList(),
            Notes = e.Notes
        })
        .ToList();
    }
}