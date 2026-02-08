using FluentValidation.TestHelper;
using Kallipr_IOT_Monitor_Backend.Models;

namespace Kallipr_IOT_Monitor_Backend.Tests.Validators;

public class TelemetryReadingValidatorTests
{
    private readonly TelemetryReadingValidator _validator;

    public TelemetryReadingValidatorTests()
    {
        _validator = new TelemetryReadingValidator();
    }

    [Fact]
    public void Should_HaveError_When_TenantIdIsEmpty()
    {
        var reading = new TelemetryReading { TenantId = "" };
        var result = _validator.TestValidate(reading);
        result.ShouldHaveValidationErrorFor(r => r.TenantId);
    }

    [Fact]
    public void Should_HaveError_When_BatteryIsNegative()
    {
        var reading = new TelemetryReading { Battery = -10 };
        var result = _validator.TestValidate(reading);
        result.ShouldHaveValidationErrorFor(r => r.Battery);
    }

    [Fact]
    public void Should_HaveError_When_BatteryIsAbove100()
    {
        var reading = new TelemetryReading { Battery = 150 };
        var result = _validator.TestValidate(reading);
        result.ShouldHaveValidationErrorFor(r => r.Battery);
    }

    [Fact]
    public void Should_NotHaveError_When_BatteryIsValid()
    {
        var reading = new TelemetryReading { Battery = 50 };
        var result = _validator.TestValidate(reading);
        result.ShouldNotHaveValidationErrorFor(r => r.Battery);
    }

    [Fact]
    public void Should_HaveError_When_SignalIsPositive()
    {
        var reading = new TelemetryReading { Signal = 10 };
        var result = _validator.TestValidate(reading);
        result.ShouldHaveValidationErrorFor(r => r.Signal);
    }

    [Fact]
    public void Should_HaveError_When_RecordedAtIsInFuture()
    {
        var reading = new TelemetryReading { RecordedAt = DateTime.UtcNow.AddDays(1) };
        var result = _validator.TestValidate(reading);
        result.ShouldHaveValidationErrorFor(r => r.RecordedAt);
    }
}