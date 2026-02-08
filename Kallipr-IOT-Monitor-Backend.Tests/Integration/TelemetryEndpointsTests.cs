using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Kallipr_IOT_Monitor_Backend.Models;


namespace Kallipr_IOT_Monitor_Backend.Tests.Integration;

public class TelemetryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TelemetryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTelemetry_ShouldReturn201Created_WhenValidReading()
    {
        var reading = new TelemetryReading
        {
            TenantId = "acme",
            DeviceId = "dev-123",
            Type = "water_level",
            Value = 1.23,
            Unit = "m",
            Battery = 62,
            Signal = -85,
            RecordedAt = DateTime.UtcNow,
            ExternalId = $"test-{Guid.NewGuid()}"
        };

        var response = await _client.PostAsJsonAsync("/api/telemetry", reading);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, $"Response was: {body}");

        var result = await response.Content.ReadFromJsonAsync<TelemetryReading>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.BatteryLow.Should().BeFalse();
        result.TenantId.Should().Be("acme");
    }

    [Fact]
    public async Task PostTelemetry_ShouldReturn409Conflict_WhenDuplicateExternalId()
    {
        var externalId = $"duplicate-{Guid.NewGuid()}";
        var reading1 = new TelemetryReading
        {
            TenantId = "acme",
            DeviceId = "dev-123",
            Type = "water_level",
            Value = 1.23,
            Unit = "m",
            Battery = 62,
            Signal = -85,
            RecordedAt = DateTime.UtcNow,
            ExternalId = externalId
        };

        var response1 = await _client.PostAsJsonAsync("/api/telemetry", reading1);
        var body1 = await response1.Content.ReadAsStringAsync();
        response1.StatusCode.Should().Be(HttpStatusCode.Created, $"First request failed: {body1}");

        var reading2 = new TelemetryReading
        {
            TenantId = "acme",
            DeviceId = "dev-123",
            Type = "water_level",
            Value = 2.34,
            Unit = "m",
            Battery = 62,
            Signal = -85,
            RecordedAt = DateTime.UtcNow,
            ExternalId = externalId
        };
        var response2 = await _client.PostAsJsonAsync("/api/telemetry", reading2);
        await response2.Content.ReadAsStringAsync();

        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetTelemetry_ShouldReturn200OK_WithPaginatedResults()
    {
        var response = await _client.GetAsync("/api/telemetry?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Response was: {body}");

        body.Should().Contain("data");
        body.Should().Contain("page");
        body.Should().Contain("total");
    }

    [Fact]
    public async Task GetHealth_ShouldReturn200OK()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("healthy");
    }
}