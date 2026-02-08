using FluentAssertions;
using Kallipr_IOT_Monitor_Backend.Models;
using Kallipr_IOT_Monitor_Backend.Repositories;
using Kallipr_IOT_Monitor_Backend.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kallipr_IOT_Monitor_Backend.Tests.Services;

public class TelemetryServiceTests
{
    private readonly Mock<ITelemetryRepository> _mockRepository;
    private readonly Mock<ILogger<TelemetryService>> _mockLogger;
    private readonly TelemetryService _service;

    public TelemetryServiceTests()
    {
        _mockRepository = new Mock<ITelemetryRepository>();
        _mockLogger = new Mock<ILogger<TelemetryService>>();
        _service = new TelemetryService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task IngestAsync_ShouldSetBatteryLowToTrue_WhenBatteryBelowThreshold()
    {
        var reading = new TelemetryReading
        {
            TenantId = "acme",
            DeviceId = "dev-123",
            Type = "water_level",
            Value = 1.23,
            Unit = "m",
            Battery = 15,
            Signal = -85,
            RecordedAt = DateTime.UtcNow,
            ExternalId = "r-789"
        };

        _mockRepository.Setup(r => r.ExistsAsync(reading.TenantId, reading.ExternalId))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.GetDeviceBatteryThresholdAsync(reading.DeviceId))
            .ReturnsAsync(20);
        _mockRepository.Setup(r => r.InsertAsync(It.IsAny<TelemetryReading>()))
            .ReturnsAsync(1);


        var result = await _service.IngestAsync(reading);

        result.BatteryLow.Should().BeTrue();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task IngestAsync_ShouldSetBatteryLowToFalse_WhenBatteryAboveThreshold()
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
            ExternalId = "r-790"
        };

        _mockRepository.Setup(r => r.ExistsAsync(reading.TenantId, reading.ExternalId))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.GetDeviceBatteryThresholdAsync(reading.DeviceId))
            .ReturnsAsync(20);
        _mockRepository.Setup(r => r.InsertAsync(It.IsAny<TelemetryReading>()))
            .ReturnsAsync(2);


        var result = await _service.IngestAsync(reading);


        result.BatteryLow.Should().BeFalse();
        result.Id.Should().Be(2);
    }

    [Fact]
    public async Task IngestAsync_ShouldThrowException_WhenDuplicateExternalId()
    {
        var reading = new TelemetryReading
        {
            TenantId = "acme",
            DeviceId = "dev-123",
            ExternalId = "r-789",
            Type = "water_level",
            Value = 1.23,
            Unit = "m",
            Battery = 62,
            Signal = -85,
            RecordedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.ExistsAsync(reading.TenantId, reading.ExternalId))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IngestAsync(reading));
    }

    [Fact]
    public async Task IngestAsync_ShouldSetCreatedAt_WhenCalled()
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
            ExternalId = "r-791"
        };

        var beforeCall = DateTime.UtcNow;

        _mockRepository.Setup(r => r.ExistsAsync(reading.TenantId, reading.ExternalId))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.GetDeviceBatteryThresholdAsync(reading.DeviceId))
            .ReturnsAsync(20);
        _mockRepository.Setup(r => r.InsertAsync(It.IsAny<TelemetryReading>()))
            .ReturnsAsync(3);

        var result = await _service.IngestAsync(reading);

        result.CreatedAt.Should().BeOnOrAfter(beforeCall);
        result.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}