using System.Net;

namespace JobApplicationHelper.Tests.Wpf.Fakes;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> ResponseFactory
    {
        get;
        set;
    } = (_, _) => Task.FromResult(
        new HttpResponseMessage(HttpStatusCode.NotFound));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        return await ResponseFactory(request, cancellationToken);
    }
}