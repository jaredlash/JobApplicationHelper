using JobApplicationHelper.ApiMappings.ToDomain;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Contracts.JobApplications;
using JobApplicationHelper.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobApplicationHelper.Api.Endpoints;

public static class JobApplicationsEndpoints
{
    public static IEndpointRouteBuilder MapJobApplicationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/job-applications");

        group.MapPost("", CreateJobApplication);
        //group.MapGet("", GetAllJobApplicationsAsync);
        group.MapGet("/{id:guid}/folder", GetJobApplicationFolderAsync);
        group.MapPut("/{id:guid}/cover-letter", SaveCoverLetterAsync);
        

        return endpoints;
    }

    //private static async Task<IResult> GetAllJobApplicationsAsync(
    //    IJobApplicationService jobApplicationService,
    //    CancellationToken cancellationToken)
    //{
    //    var jobApplications = await jobApplicationService.GetAllAsync(cancellationToken);

    //    var response = jobApplications
    //        .Select(jobApplication => jobApplication.ToDto())
    //        .ToList();

    //    return Results.Ok(response);
    //}

    private static async Task<Results<Ok<CreateJobApplicationResponse>, BadRequest<string>>> CreateJobApplication(
        CreateJobApplicationRequest request,
        IApplicationMaterialsService applicationMaterialsService,
        CancellationToken cancellationToken)
    {
        var applicationId = await applicationMaterialsService.CreateApplicationMaterialsAsync(request.ToDomain(), cancellationToken);
        return TypedResults.Ok(new CreateJobApplicationResponse(applicationId.Value.ToString()));
    }

    private static async Task<Results<Ok<ApplicationFolderResponse>, NotFound>> GetJobApplicationFolderAsync(
        Guid id,
        IApplicationMaterialsService applicationMaterialsService,
        CancellationToken cancellationToken)
    {
        var folderPath = await applicationMaterialsService.GetApplicationFolderAsync(new JobApplicationId(id), cancellationToken);
        return TypedResults.Ok(new ApplicationFolderResponse(folderPath));
    }

    private static async Task<NoContent> SaveCoverLetterAsync(
        Guid id,
        SaveCoverLetterRequest request,
        IApplicationMaterialsService applicationMaterialsService,
        CancellationToken cancellationToken)
    {
        await applicationMaterialsService.SaveCoverLetterDraftAsync(new JobApplicationId(id), request.CoverLetter, cancellationToken);

        return TypedResults.NoContent();
    }
}