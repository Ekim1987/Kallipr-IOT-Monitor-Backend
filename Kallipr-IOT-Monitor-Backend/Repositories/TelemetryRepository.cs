using System.Data;
using Dapper;
using Kallipr_IOT_Monitor_Backend.Models;
using Microsoft.Data.Sqlite;

namespace Kallipr_IOT_Monitor_Backend.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly IDbConnection _db;

    public TelemetryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<int> InsertAsync(TelemetryReading reading)
    {
        try
        {
            var sql = @"INSERT INTO telemetry_readings 
            (tenant_id, device_id, type, value, unit, battery, signal, recorded_at, external_id, created_at, battery_low)
            VALUES (@TenantId, @DeviceId, @Type, @Value, @Unit, @Battery, @Signal, @RecordedAt, @ExternalId, @CreatedAt, @BatteryLow);
            SELECT last_insert_rowid();";

            return await _db.QuerySingleAsync<int>(sql, reading);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException("Duplicate externalId for this tenant", ex);
        }
        catch (SqliteException ex)
        {
            throw new InvalidOperationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to insert telemetry reading: {ex.Message}", ex);
        }
    }

    public async Task<TelemetryReading?> GetByIdAsync(int id)
    {
        try
        {
            var sql = "SELECT * FROM telemetry_readings WHERE id = @Id";
            return await _db.QuerySingleOrDefaultAsync<TelemetryReading>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve telemetry reading with id {id}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string tenantId, string externalId)
    {
        try
        {
            var sql =
                "SELECT COUNT(1) FROM telemetry_readings WHERE tenant_id = @TenantId AND external_id = @ExternalId";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { TenantId = tenantId, ExternalId = externalId });
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check if telemetry reading exists for tenant {tenantId}",
                ex);
        }
    }

    public async Task<IEnumerable<TelemetryReading>> QueryAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var sql = "SELECT * FROM telemetry_readings WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(deviceId))
            {
                sql += " AND device_id = @DeviceId";
                parameters.Add("DeviceId", deviceId);
            }

            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND type = @Type";
                parameters.Add("Type", type);
            }

            if (startDate.HasValue)
            {
                sql += " AND recorded_at >= @StartDate";
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                sql += " AND recorded_at <= @EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            sql += " ORDER BY recorded_at DESC LIMIT @PageSize OFFSET @Offset";
            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", (page - 1) * pageSize);

            return await _db.QueryAsync<TelemetryReading>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to query telemetry readings", ex);
        }
    }

    public async Task<int> GetCountAsync(
        string? deviceId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM telemetry_readings WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(deviceId))
            {
                sql += " AND device_id = @DeviceId";
                parameters.Add("DeviceId", deviceId);
            }

            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND type = @Type";
                parameters.Add("Type", type);
            }

            if (startDate.HasValue)
            {
                sql += " AND recorded_at >= @StartDate";
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                sql += " AND recorded_at <= @EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            return await _db.ExecuteScalarAsync<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get count of telemetry readings", ex);
        }
    }
    
    public async Task<int> GetDeviceBatteryThresholdAsync(string deviceId)
    {
        try
        {
            var sql = "SELECT battery_low_threshold FROM devices WHERE device_id = @DeviceId";
            var threshold = await _db.QuerySingleOrDefaultAsync<int?>(sql, new { DeviceId = deviceId });
            return threshold ?? 20;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get battery threshold for device {deviceId}", ex);
        }
    }
}