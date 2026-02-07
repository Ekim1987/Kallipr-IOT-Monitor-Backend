using Kallipr_Web_Api.Models;
using Kallipr_Web_Api.Repositories;
using Kallipr_Web_Api.Services.Interfaces;
using Microsoft.Data.Sqlite;

namespace Kallipr_Web_Api.Services;

public class TelemetryService : ITelemetryService
{
    private readonly ITelemetryRepository _repository;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(
        ITelemetryRepository repository,
        ILogger<TelemetryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TelemetryReading> IngestAsync(TelemetryReading reading)
    {
        try
        {
            if (await _repository.ExistsAsync(reading.TenantId, reading.ExternalId))
            {
                _logger.LogWarning("Duplicate telemetry reading: TenantId={TenantId}, ExternalId={ExternalId}",
                    reading.TenantId, reading.ExternalId);
                throw new InvalidOperationException("Duplicate externalId for this tenant");
            }

            var batteryThreshold = await _repository.GetDeviceBatteryThresholdAsync(reading.DeviceId);
            
            reading.BatteryLow = reading.Battery < batteryThreshold;
            reading.CreatedAt = DateTime.UtcNow;

            // TODO: Send push notification via Firebase when battery is low to mobile app
            // if (reading.BatteryLow)
            // {
            //     await _notificationService.SendLowBatteryAlertAsync(reading.DeviceId, reading.Battery);
            // }

            _logger.LogInformation(
                "Ingesting telemetry: TenantId={TenantId}, DeviceId={DeviceId}, Type={Type}, BatteryLow={BatteryLow}",
                reading.TenantId, reading.DeviceId, reading.Type, reading.BatteryLow);

            var id = await _repository.InsertAsync(reading);
            reading.Id = id;

            return reading;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Database error while ingesting telemetry: TenantId={TenantId}, DeviceId={DeviceId}",
                reading.TenantId, reading.DeviceId);
            throw new InvalidOperationException("Failed to save telemetry reading due to database error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while ingesting telemetry: TenantId={TenantId}, DeviceId={DeviceId}",
                reading.TenantId, reading.DeviceId);
            throw new InvalidOperationException("An unexpected error occurred while processing the telemetry reading",
                ex);
        }
    }

    public async Task<TelemetryReading?> GetByIdAsync(int id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving telemetry reading: Id={Id}", id);
            throw new InvalidOperationException("Failed to retrieve telemetry reading", ex);
        }
    }

    public async Task<(IEnumerable<TelemetryReading> Data, int Total)> QueryAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var readings = await _repository.QueryAsync(deviceId, type, startDate, endDate, page, pageSize);
            var total = await _repository.GetCountAsync(deviceId, type, startDate, endDate);

            return (readings, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying telemetry readings: DeviceId={DeviceId}, Type={Type}", deviceId, type);
            throw new InvalidOperationException("Failed to query telemetry readings", ex);
        }
    }
}