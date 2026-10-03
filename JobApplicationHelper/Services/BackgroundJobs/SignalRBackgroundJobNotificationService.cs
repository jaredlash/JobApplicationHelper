using JobApplicationHelper.Contracts.BackgroundJobs;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Exceptions;
using JobApplicationHelper.Services.Api;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;

namespace JobApplicationHelper.Services.BackgroundJobs;

public sealed class SignalRBackgroundJobNotificationService :
    IBackgroundJobNotificationService,
    IAsyncDisposable
{
    private readonly HubConnection hubConnection;

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<BackgroundJobStatusChanged>> waiters = new();
    private readonly BackgroundJobsApiClient backgroundJobsApiClient;

    public SignalRBackgroundJobNotificationService(BackgroundJobsApiClient backgroundJobsApiClient, string hubUrl)
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<BackgroundJobStatusChanged>("BackgroundJobStatusChanged", OnBackgroundJobStatusChanged);
        hubConnection.Closed += OnConnectionClosed;

        this.backgroundJobsApiClient = backgroundJobsApiClient;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (hubConnection.State == HubConnectionState.Connected)
            return;

        try
        {
            await hubConnection.StartAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BackgroundJobNotificationException("Unable to connect to the background job notification service.", ex);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await hubConnection.StopAsync(cancellationToken);
    }

    public async Task<BackgroundJobStatusChanged> WaitForStatusChangeAsync(BackgroundJobId backgroundJobId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var waiter = new TaskCompletionSource<BackgroundJobStatusChanged>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!waiters.TryAdd(backgroundJobId.Value, waiter))
        {
            throw new InvalidOperationException($"Already waiting for background job {backgroundJobId.Value}.");
        }

        try
        {
            // Check again after registering the waiter. This closes the race
            // between the initial status check and registering the waiter.
            var response = await backgroundJobsApiClient.GetAsync(backgroundJobId, cancellationToken);

            var status = ParseStatus(response.Status);

            if (IsTerminal(status))
            {
                waiter.TrySetResult(new BackgroundJobStatusChanged(response.Id, response.Status, response.Error));
            }

            return await waiter.Task.WaitAsync(timeout, cancellationToken);
        }
        finally
        {
            waiters.TryRemove(backgroundJobId.Value, out _);
        }
    }

    private void OnBackgroundJobStatusChanged(BackgroundJobStatusChanged notification)
    {
        if (waiters.TryGetValue(notification.BackgroundJobId, out var waiter))
        {
            waiter.TrySetResult(notification);
        }
    }

    private Task OnConnectionClosed(Exception? exception)
    {
        foreach (var waiter in waiters.Values)
        {
            waiter.TrySetException(new BackgroundJobNotificationException("The SignalR connection was closed.", exception));
        }

        return Task.CompletedTask;
    }

    private static BackgroundJobStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<BackgroundJobStatus>(status, ignoreCase: true, out var statusResult))
        {
            throw new InvalidOperationException($"The background jobs API returned an unknown status '{status}'.");
        }

        return statusResult;
    }

    private static bool IsTerminal(BackgroundJobStatus status) =>
        status is
            BackgroundJobStatus.Completed or
            BackgroundJobStatus.Failed or
            BackgroundJobStatus.Cancelled;

    public async ValueTask DisposeAsync()
    {
        await hubConnection.DisposeAsync();
    }
}