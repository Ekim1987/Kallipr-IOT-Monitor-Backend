using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FluentValidation;

namespace Kallipr_IOT_Monitor_Backend.Models;


[Table("telemetry_readings")]
public class TelemetryReading
{
    [Key]
    [Column("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("tenant_id")]
    [MaxLength(100)]
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;
    
    [Required]
    [Column("device_id")]
    [MaxLength(100)]
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Required]
    [Column("type")]
    [MaxLength(50)]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [Column("value")]
    [JsonPropertyName("value")]
    public double Value { get; set; }
    
    [Required]
    [Column("unit")]
    [MaxLength(20)]
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Range(0, 100)]
    [Column("battery")]
    [JsonPropertyName("battery")]
    public int Battery { get; set; }
    
    [Column("signal")]
    [JsonPropertyName("signal")]
    public int Signal { get; set; }
    
    [Column("recorded_at")]
    [JsonPropertyName("recordedAt")]
    public DateTime RecordedAt { get; set; }
    
    [Required]
    [Column("external_id")]
    [MaxLength(100)]
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [Column("created_at")]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [Column("battery_low")]
    [JsonPropertyName("batteryLow")]
    public bool BatteryLow { get; set; }
}

public class TelemetryReadingValidator : AbstractValidator<TelemetryReading>
{
    public TelemetryReadingValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required")
            .MaximumLength(100).WithMessage("TenantId must not exceed 100 characters");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId is required")
            .MaximumLength(100).WithMessage("DeviceId must not exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .MaximumLength(50).WithMessage("Type must not exceed 50 characters");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters");

        RuleFor(x => x.Battery)
            .InclusiveBetween(0, 100).WithMessage("Battery must be between 0 and 100");

        RuleFor(x => x.Signal)
            .LessThanOrEqualTo(0).WithMessage("Signal must be 0 or negative (dBm)");

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage("RecordedAt is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("RecordedAt cannot be in the future");

        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("ExternalId is required")
            .MaximumLength(100).WithMessage("ExternalId must not exceed 100 characters");
    }
}