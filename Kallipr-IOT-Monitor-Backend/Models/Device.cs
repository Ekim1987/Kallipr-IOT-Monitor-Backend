using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


[Table("devices")]
public class Device
{
    [Key]
    [Column("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("device_id")]
    [MaxLength(100)]
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Required]
    [Column("tenant_id")]
    [MaxLength(100)]
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;
    
    [Column("name")]
    [MaxLength(200)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [Column("device_type")]
    [MaxLength(50)]
    [JsonPropertyName("deviceType")]
    public string? DeviceType { get; set; }
    
    [Column("battery_low_threshold")]
    [JsonPropertyName("batteryLowThreshold")]
    public int BatteryLowThreshold { get; set; }
    
    [Column("created_at")]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [Column("is_active")]
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}