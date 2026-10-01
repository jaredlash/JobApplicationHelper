using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.ApiMappings.ToDto;
using JobApplicationHelper.Application;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Contracts.CoverLetters;
using JobApplicationHelper.Contracts.JobApplications;
using JobApplicationHelper.Contracts.JobRequirements;
using JobApplicationHelper.Contracts.Locations;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Infrastructure;
using JobApplicationHelper.Api.Endpoints;
using Scalar.AspNetCore;

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
    app.MapScalarApiReference(); // adds the /scalar/v1 endpoint for the Scalar API reference
}

app.UseHttpsRedirection();

app.MapJobRequirementsEndpoints();
app.MapBackgroundJobEndpoints();
app.MapCoverLetterEndpoints();


app.MapPost(
    "/api/job-requirements",
    async (
        SynchronousExtractJobRequirementsRequest request,
        IJobRequirementService service,
        CancellationToken cancellationToken) =>
    {
        var result = await service.ExtractRequirementsAsync(
            request.JobPosting,
            cancellationToken);

        var response = new SynchronousExtractJobRequirementsResponse(
            result.Requirements
                .Select(r => new JobRequirementDto(r.Requirement, r.Category.ToString(), r.Priority.ToString()))
                .ToList()
        );

        return Results.Ok(response);
    });

//app.MapPost(
//    "/api/cover-letters",
//    async (
//        GenerateCoverLetterRequest request,
//        ICoverLetterService coverLetterService,
//        CancellationToken cancellationToken) =>
//    {
//        //var draft = await coverLetterService.GenerateCoverLetterAsync(
//        //    request.DraftParameters.ToDomain(),
//        //    cancellationToken);

//        var backgroundJobId = Guid.NewGuid();
//        var response = new GenerateCoverLetterResponse(backgroundJobId);

//        return Results.Ok(response);
//    });

app.MapPost(
    "/api/cover-letters/verify",
    async (
        VerifyCoverLetterRequest request,
        ICoverLetterService coverLetterService,
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


app.MapGet(
    "/api/job-applications/{id}/folder",
    async (string id, IApplicationMaterialsService applicationMaterialsService, CancellationToken cancellationToken) =>
    {
        var folderPath = await applicationMaterialsService.GetApplicationFolderAsync(new JobApplicationId(Guid.Parse(id)), cancellationToken);
        return Results.Ok(new ApplicationFolderResponse(folderPath));
    });

app.MapPut(
    "/api/job-applications/{id}/cover-letter",
    async (
        string id,
        SaveCoverLetterRequest request,
        IApplicationMaterialsService applicationMaterialsService,
        CancellationToken cancellationToken) =>
    {
        await applicationMaterialsService.SaveCoverLetterDraftAsync(new JobApplicationId(Guid.Parse(id)), request.CoverLetter, cancellationToken);

        return Results.NoContent();
    });

app.MapPost(
    "/api/job-applications",
    async (
        CreateJobApplicationRequest request,
        IApplicationMaterialsService applicationMaterialsService,
        CancellationToken cancellationToken) =>
    {
        var applicationId = await applicationMaterialsService.CreateApplicationMaterialsAsync(request.ToDomain(), cancellationToken);
        return Results.Ok(new CreateJobApplicationResponse(applicationId.Value.ToString()));
    });

//app.MapPost(
//    "/api/background-jobs",
//    async (
//        CreateBackgroundJobRequest request,
//        IBackgroundJobService backgroundJobService,
//        CancellationToken cancellationToken) =>
//    {
//        var id = await backgroundJobService.CreateAsync(
//            Enum.Parse<BackgroundJobType>(request.Type, ignoreCase: true),
//            Enum.Parse<BackgroundJobPriority>(request.Priority, ignoreCase: true),
//            new JobApplicationId(Guid.NewGuid()), //new JobApplicationId(Guid.Parse(request.JobApplicationId)),
//            null, //request.Payload,
//            cancellationToken);

//        return Results.Ok(new CreateBackgroundJobResponse(id.Value));
//    });

app.MapGet("/health", () => Results.Ok());

app.Run();

// Make the Program class public so that integration tests can access it
public partial class Program
{
}