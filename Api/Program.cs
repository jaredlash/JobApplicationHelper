using JobApplicationHelper.Application;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Contracts.Locations;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost(
    "/api/job-requirements",
    async (
        ExtractJobRequirementsRequest request,
        JobRequirementService service,
        CancellationToken cancellationToken) =>
    {
        var result = await service.ExtractRequirementsAsync(
            request.JobPosting,
            cancellationToken);

        var response = new ExtractJobRequirementsResponse(
            result.Requirements
                .Select(r => new JobRequirementDto(r.Requirement, r.Category.ToString(), r.Priority.ToString()))
                .ToList()
        );

        return Results.Ok(response);
    });

app.MapGet(
    "/api/experiences",
    async (
        IExperienceBankService service,
        CancellationToken cancellationToken) =>
    {
        var experiences = await service.GetAllAsync(cancellationToken);

        var response = experiences
            .Select(e => new ExperienceDto(
                e.Id,
                e.Title,
                e.Type.ToString(),
                e.Organization,
                e.DateRange is null
                    ? null
                    : new DateRangeDto(
                        e.DateRange.Start is null
                            ? null
                            : new PartialDateDto(e.DateRange.Start.Year, e.DateRange.Start.Month, e.DateRange.Start.Day),
                        e.DateRange.End is null
                            ? null
                            : new PartialDateDto(e.DateRange.End.Year, e.DateRange.End.Month, e.DateRange.End.Day)),
                e.Summary,
                e.Skills,
                e.Evidence,
                e.Contexts,
                e.Links,
                e.Notes))
            .ToList();

        return Results.Ok(response);
    });

app.MapGet(
    "/api/locations",
    (LocationService service) =>
    {
        var locations = service.GetLocations();

        var response = locations
            .Select(location => new LocationDto(location.CountryCode, location.CountryName))
            .ToList();

        return Results.Ok(response);
    });

app.MapGet("/health", () => Results.Ok());

app.Run();

