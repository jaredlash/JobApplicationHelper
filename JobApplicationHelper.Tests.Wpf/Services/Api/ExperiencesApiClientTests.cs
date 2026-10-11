using JobApplicationHelper.Contracts.Experiences;
using JobApplicationHelper.Domain.Models;
using JobApplicationHelper.Services.Api;
using JobApplicationHelper.Tests.Wpf.Fakes;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace JobApplicationHelper.Tests.Wpf.Services.Api;

public sealed class ExperiencesApiClientTests
{
    [Fact]
    public async Task GetAllAsync_sends_get_request_to_experiences_endpoint()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(CreateJsonResponse(
                HttpStatusCode.OK,
                Array.Empty<ExperienceDto>()));

        var client = CreateClient(handler);

        await client.GetAllAsync(TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://localhost/api/experiences", request.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetAllAsync_returns_mapped_experiences()
    {
        var handler = CreateHandler();

        var dto = new ExperienceDto(
            Guid.NewGuid(),
            "Software Developer",
            "professional",
            "Example Company",
            null,
            "Built software.",
            ["C#", "WPF"],
            ["Delivered a feature."],
            ["Professional"],
            ["https://example.com"],
            "Notes");

        handler.ResponseFactory = (_, _) =>
            Task.FromResult(CreateJsonResponse(
                HttpStatusCode.OK,
                new[] { dto }));

        var client = CreateClient(handler);

        var result = await client.GetAllAsync(TestContext.Current.CancellationToken);

        var experience = Assert.Single(result);

        Assert.Equal(dto.Id, experience.Id.Value);
        Assert.Equal(dto.Title, experience.Title);
        Assert.Equal(dto.Organization, experience.Organization);
        Assert.Equal(dto.Summary, experience.Summary);
        Assert.Equal(dto.Skills, experience.Skills);
        Assert.Equal(dto.Evidence, experience.Evidence);
        Assert.Equal(dto.Contexts, experience.Contexts);
        Assert.Equal(dto.Links, experience.Links);
        Assert.Equal(dto.Notes, experience.Notes);
    }

    [Fact]
    public async Task GetAllAsync_throws_when_response_is_unsuccessful()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAllAsync_throws_when_response_body_is_empty()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_sends_get_request_with_experience_id()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(CreateJsonResponse(
                HttpStatusCode.OK,
                CreateExperienceDto(id)));

        var client = CreateClient(handler);

        await client.GetAsync(id, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(
            $"https://localhost/api/experiences/{id}",
            request.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetAsync_returns_mapped_experience()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(CreateJsonResponse(
                HttpStatusCode.OK,
                CreateExperienceDto(id)));

        var client = CreateClient(handler);

        var result = await client.GetAsync(id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id.Value);
        Assert.Equal("Software Developer", result.Title);
    }

    [Fact]
    public async Task GetAsync_returns_null_when_experience_does_not_exist()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = CreateClient(handler);

        var result = await client.GetAsync(id, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_throws_when_response_is_unsuccessful()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetAsync(id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_throws_when_response_body_is_empty()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetAsync(id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddOrReplaceAsync_sends_put_request_with_experience_id()
    {
        var id = Guid.NewGuid();

        var requestModel = new Experience()
        {
            Id = new ExperienceId(id),
            Title = "Software Developer",
            Type = ExperienceType.Professional,
            Organization = "Example Company",
            DateRange = new DateRange(),
            Summary = "Built software.",
            Skills = ["C#", "WPF"],
            Evidence = ["Delivered a feature."],
            Contexts = ["Professional"],
            Links = ["https://example.com"],
            Notes = "Notes"
        };

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.NoContent));

        var client = CreateClient(handler);

        await client.AddOrReplaceAsync(requestModel, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal(
            $"https://localhost/api/experiences/{id}",
            request.RequestUri?.ToString());
    }

    [Fact]
    public async Task AddOrReplaceAsync_sends_request_body()
    {
        var id = Guid.NewGuid();

        var requestModel = new Experience()
        {
            Id = new ExperienceId(id),
            Title = "Software Developer",
            Type = ExperienceType.Professional,
            Organization = "Example Company",
            DateRange = new DateRange(),
            Summary = "Built software.",
            Skills = ["C#", "WPF"],
            Evidence = ["Delivered a feature."],
            Contexts = ["Professional"],
            Links = ["https://example.com"],
            Notes = "Notes"
        };

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.NoContent));

        var client = CreateClient(handler);

        await client.AddOrReplaceAsync(requestModel, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        var body = await request.Content!.ReadFromJsonAsync<ExperienceDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(requestModel.Title, body.Title);
        Assert.Equal(requestModel.Type.ToString().ToLower(), body.Type.ToLower());
        Assert.Equal(requestModel.Organization, body.Organization);
        Assert.Equal(requestModel.Summary, body.Summary);
        Assert.Equal(requestModel.Skills, body.Skills);
        Assert.Equal(requestModel.Evidence, body.Evidence);
        Assert.Equal(requestModel.Contexts, body.Contexts);
        Assert.Equal(requestModel.Links, body.Links);
        Assert.Equal(requestModel.Notes, body.Notes);
    }

    [Fact]
    public async Task AddOrReplaceAsync_succeeds_when_response_is_no_content()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        var client = CreateClient(handler);

        await client.AddOrReplaceAsync(CreateExperience(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpsertAsync_throws_when_response_is_unsuccessful()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.AddOrReplaceAsync(CreateExperience(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_sends_delete_request_with_experience_id()
    {
        var id = Guid.NewGuid();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.NoContent));

        var client = CreateClient(handler);

        await client.DeleteAsync(id, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal(
            $"https://localhost/api/experiences/{id}",
            request.RequestUri?.ToString());
    }

    [Fact]
    public async Task DeleteAsync_succeeds_when_response_is_no_content()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.NoContent));

        var client = CreateClient(handler);

        await client.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_throws_when_response_is_unsuccessful()
    {
        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ImportAsync_sends_post_request_to_import_endpoint()
    {
        await using var stream = CreateYamlStream();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                CreateJsonResponse(
                    HttpStatusCode.OK,
                    new ImportExperiencesResponse(1, 2, 3)));

        var client = CreateClient(handler);

        await client.ImportAsync(stream, "experiences.yaml", TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            "https://localhost/api/experiences/import",
            request.RequestUri?.ToString());
    }

    [Fact]
    public async Task ImportAsync_sends_yaml_as_file_named_file()
    {
        const string yaml = """
        version: 1
        experiences: []
        """;

        await using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(yaml));

        var handler = CreateHandler();

        handler.ResponseFactory = async (request, _) =>
        {
            Assert.NotNull(request.Content);

            var multipart = await request.Content.ReadAsMultipartAsync();

            var fileContent = Assert.Single(multipart.Contents);

            Assert.Equal(
                "file",
                fileContent.Headers.ContentDisposition?.Name);

            Assert.Equal(
                "experiences.yaml",
                fileContent.Headers.ContentDisposition?.FileName);

            var uploadedYaml = await fileContent.ReadAsStringAsync();

            Assert.Equal(yaml, uploadedYaml);

            return CreateJsonResponse(
                HttpStatusCode.OK,
                new ImportExperiencesResponse(1, 2, 3));
        };

        var client = CreateClient(handler);

        await client.ImportAsync(stream, "experiences.yaml", TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ImportAsync_returns_import_result()
    {
        await using var stream = CreateYamlStream();

        var handler = CreateHandler();

        var expected = new ImportExperiencesResponse(
            AddedCount: 4,
            SkippedCount: 5,
            PendingCount: 6);

        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                CreateJsonResponse(HttpStatusCode.OK, expected));

        var client = CreateClient(handler);

        var result = await client.ImportAsync(
            stream,
            "experiences.yaml", TestContext.Current.CancellationToken);

        Assert.Equal(expected.AddedCount, result.AddedCount);
        Assert.Equal(expected.SkippedCount, result.SkippedCount);
        Assert.Equal(expected.PendingCount, result.PendingCount);
    }

    [Fact]
    public async Task ImportAsync_throws_when_response_is_unsuccessful()
    {
        await using var stream = CreateYamlStream();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.BadRequest));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ImportAsync(
                stream,
                "experiences.yaml", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ImportAsync_throws_when_response_body_is_empty()
    {
        await using var stream = CreateYamlStream();

        var handler = CreateHandler();
        handler.ResponseFactory = (_, _) =>
            Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK));

        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.ImportAsync(
                stream,
                "experiences.yaml", TestContext.Current.CancellationToken));
    }

    private static FakeHttpMessageHandler CreateHandler()
    {
        return new FakeHttpMessageHandler();
    }

    private static ExperiencesApiClient CreateClient(
        FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        };

        return new ExperiencesApiClient(httpClient);
    }

    private static ExperienceDto CreateExperienceDto(Guid id)
    {
        return new ExperienceDto(
            id,
            "Software Developer",
            "professional",
            "Example Company",
            null,
            "Built software.",
            ["C#", "WPF"],
            ["Delivered a feature."],
            ["Professional"],
            ["https://example.com"],
            "Notes");
    }

    private static Experience CreateExperience()
    {
        return new Experience
        {
            Id = new ExperienceId(Guid.NewGuid()),
            Title = "Software Developer",
            Type = ExperienceType.Professional,
            Organization = "Example Company",
            DateRange = new DateRange(),
            Summary = "Built software.",
            Skills = ["C#", "WPF"],
            Evidence = ["Delivered a feature."],
            Contexts = ["Professional"],
            Links = ["https://example.com"],
            Notes = "Notes"
        };
    }

    private static MemoryStream CreateYamlStream()
    {
        const string yaml = """
            version: 1
            experiences: []
            """;

        return new MemoryStream(Encoding.UTF8.GetBytes(yaml));
    }

    private static HttpResponseMessage CreateJsonResponse<T>(
        HttpStatusCode statusCode,
        T value)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(value)
        };
    }
}