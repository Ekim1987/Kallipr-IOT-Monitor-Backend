using Kallipr_Web_Api.Models;

namespace Kallipr_Web_Api.Repositories;

public interface ITelemetryRepository
{
    Task<int> InsertAsync(TelemetryReading reading);

    Task<TelemetryReading?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(string tenantId, string externalId);

    Task<IEnumerable<TelemetryReading>> QueryAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);

    Task<int> GetCountAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<int> GetDeviceBatteryThresholdAsync(string deviceId);

}