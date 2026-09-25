namespace JobApplicationHelper.Services.Api;

public interface IApiHealthService
{
    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
}

