using Microsoft.Data.Sqlite;

namespace Kallipr_Web_Api.Data;

public static class DatabaseInitializer
{
    public static void Initialize(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KalliprDb");

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var createTablesSql = @"
             CREATE TABLE IF NOT EXISTS tenants (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tenant_id TEXT NOT NULL UNIQUE,
                name TEXT NOT NULL,
                created_at TEXT NOT NULL,
                is_active INTEGER NOT NULL DEFAULT 1
            );

           CREATE TABLE IF NOT EXISTS devices (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                device_id TEXT NOT NULL UNIQUE,
                tenant_id TEXT NOT NULL,
                name TEXT,
                device_type TEXT,
                battery_low_threshold INTEGER NOT NULL DEFAULT 20,
                created_at TEXT NOT NULL,
                is_active INTEGER NOT NULL DEFAULT 1,
                FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE CASCADE
            );

           
            CREATE TABLE IF NOT EXISTS telemetry_readings (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tenant_id TEXT NOT NULL,
                device_id TEXT NOT NULL,
                type TEXT NOT NULL,
                value REAL NOT NULL,
                unit TEXT NOT NULL,
                battery INTEGER NOT NULL,
                signal INTEGER NOT NULL,
                recorded_at TEXT NOT NULL,
                external_id TEXT NOT NULL,
                created_at TEXT NOT NULL,
                battery_low INTEGER NOT NULL,
                FOREIGN KEY (tenant_id) REFERENCES tenants(tenant_id) ON DELETE CASCADE,
                FOREIGN KEY (device_id) REFERENCES devices(device_id) ON DELETE CASCADE,
                UNIQUE(tenant_id, external_id)
            );

            CREATE INDEX IF NOT EXISTS idx_telemetry_device_id ON telemetry_readings(device_id);
            CREATE INDEX IF NOT EXISTS idx_telemetry_type ON telemetry_readings(type);
            CREATE INDEX IF NOT EXISTS idx_telemetry_recorded_at ON telemetry_readings(recorded_at);
            CREATE INDEX IF NOT EXISTS idx_telemetry_tenant_external ON telemetry_readings(tenant_id, external_id);
            CREATE INDEX IF NOT EXISTS idx_telemetry_tenant_id ON telemetry_readings(tenant_id);

            CREATE INDEX IF NOT EXISTS idx_devices_tenant_id ON devices(tenant_id);

            INSERT OR IGNORE INTO tenants (tenant_id, name, created_at, is_active)
            VALUES ('acme', 'ACME Corporation', datetime('now'), 1);

            INSERT OR IGNORE INTO devices (device_id, tenant_id, name, device_type, battery_low_threshold, created_at, is_active)
            VALUES ('dev-123', 'acme', 'Water Level Sensor 123', 'water_level', 20, datetime('now'), 1);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTablesSql;
        command.ExecuteNonQuery();
    }
}