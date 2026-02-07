using Microsoft.AspNetCore.Mvc;

namespace Kallipr_Web_Api.Models.Payloads;

public class TelemetryQuery
{
    [FromQuery(Name = "deviceId")]
    public string? DeviceId { get; set; }
    
    [FromQuery(Name = "type")]
    public string? Type { get; set; }
    
    [FromQuery(Name = "startDate")]
    public DateTime? StartDate { get; set; }
    
    [FromQuery(Name = "endDate")]
    public DateTime? EndDate { get; set; }
    
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;
    
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 10;
}