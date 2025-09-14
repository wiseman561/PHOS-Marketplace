public class ReviewPolicyApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReviewPolicyApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthz_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ok");
    }

    [Fact]
    public async Task GetApiInfo_ReturnsServiceInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("review-policy");
        content.Should().Contain("0.1.0");
    }

    [Fact]
    public async Task PostSubmissions_CreatesSubmission()
    {
        // Arrange
        var submission = new
        {
            appId = "app.demo",
            version = "1.0.0",
            notes = "Initial"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/submissions", submission);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/submissions/");

        var createdSubmission = await response.Content.ReadFromJsonAsync<Submission>();
        createdSubmission.Should().NotBeNull();
        createdSubmission!.Id.Should().NotBeEmpty();
        createdSubmission.AppId.Should().Be("app.demo");
        createdSubmission.Version.Should().Be("1.0.0");
        createdSubmission.Notes.Should().Be("Initial");
        createdSubmission.Status.Should().Be("received");
        createdSubmission.CreatedAt.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSubmissions_ReturnsArray()
    {
        // Arrange - Create a submission first
        var submission = new
        {
            appId = "app.demo",
            version = "1.0.0",
            notes = "Test submission"
        };
        await _client.PostAsJsonAsync("/submissions", submission);

        // Act
        var response = await _client.GetAsync("/submissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var submissions = await response.Content.ReadFromJsonAsync<Submission[]>();
        submissions.Should().NotBeNull();
        submissions!.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetSubmissionById_ReturnsSubmission()
    {
        // Arrange - Create a submission first
        var submission = new
        {
            appId = "app.demo",
            version = "1.0.0",
            notes = "Test submission"
        };
        var createResponse = await _client.PostAsJsonAsync("/submissions", submission);
        var createdSubmission = await createResponse.Content.ReadFromJsonAsync<Submission>();
        var submissionId = createdSubmission!.Id;

        // Act
        var response = await _client.GetAsync($"/submissions/{submissionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedSubmission = await response.Content.ReadFromJsonAsync<Submission>();
        retrievedSubmission.Should().NotBeNull();
        retrievedSubmission!.Id.Should().Be(submissionId);
        retrievedSubmission.AppId.Should().Be("app.demo");
    }

    [Fact]
    public async Task GetSubmissionById_NotFound_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/submissions/not-there");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not_found");
    }
}

