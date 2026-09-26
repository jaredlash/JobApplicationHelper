using JobApplicationHelper.Application;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Contracts.Locations;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure;
using JobApplicationHelper.Contracts.CoverLetters;

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

app.MapPost(
    "/api/cover-letters",
    async (
        GenerateCoverLetterRequest request,
        CoverLetterService coverLetterService,
        CancellationToken cancellationToken) =>
    {
        var draft = await coverLetterService.GenerateCoverLetterAsync(
            request.DraftParameters.ToDomain(),
            cancellationToken);

        var response = new GenerateCoverLetterResponse(draft);

        return Results.Ok(response);
    });

app.MapPost(
    "/api/cover-letters/verify",
    async (
        VerifyCoverLetterRequest request,
        CoverLetterService coverLetterService,
        CancellationToken cancellationToken) =>
    {
        var verificationResult = await coverLetterService.VerifyDraftAsync(
            request.DraftParameters.ToDomain(),
            request.Draft,
            cancellationToken);

        return Results.Ok(verificationResult.ToDto());
    });

app.MapGet(
    "/api/experiences",
    async (
        IExperienceBankService service,
        CancellationToken cancellationToken) =>
    {
        var experiences = await service.GetAllAsync(cancellationToken);

        var response = experiences
            .Select(e => e.ToDto())
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

