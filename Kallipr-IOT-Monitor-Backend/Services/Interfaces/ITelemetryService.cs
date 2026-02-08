using Kallipr_IOT_Monitor_Backend.Models;

namespace Kallipr_IOT_Monitor_Backend.Services.Interfaces;

public interface ITelemetryService
{
    Task<TelemetryReading> IngestAsync(TelemetryReading reading);
    Task<TelemetryReading?> GetByIdAsync(int id);

    Task<(IEnumerable<TelemetryReading> Data, int Total)> QueryAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);
}