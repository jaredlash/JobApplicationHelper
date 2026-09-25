using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace JobApplicationHelper.Services.Api;

public sealed class ApiHealthService : IApiHealthService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

    private readonly HttpClient httpClient;
    private readonly ILogger<ApiHealthService> logger;

    public ApiHealthService(HttpClient httpClient, ILogger<ApiHealthService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

        while (true)
        {
            try
            {
                using var response = await httpClient.GetAsync(
                    "health",
                    timeoutCts.Token);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("API is ready.");
                    return;
                }

                logger.LogDebug("API health check returned HTTP {StatusCode}.", (int)response.StatusCode);
            }
            catch (HttpRequestException)
            {
                logger.LogDebug("API is not yet available.");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Overall timeout has expired.
                throw new TimeoutException("The API did not become ready within 30 seconds.");
            }

            await Task.Delay(PollInterval, timeoutCts.Token);
        }
    }
}