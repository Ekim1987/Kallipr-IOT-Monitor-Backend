# Kallipr Telemetry API

A .NET 8 Web API for ingesting and querying IoT device telemetry data with SQLite persistence.

## Features

-  **Telemetry Ingestion** - Accept and validate sensor readings from IoT devices
-  **Query API** - Filter telemetry by device, type, and time range with pagination
-  **Battery Monitoring** - Per-device battery thresholds with low-battery flagging
-  **Duplicate Prevention** - Idempotent ingestion using external IDs
-  **API Documentation** - Interactive Scalar and Swagger UI
-  **Data Validation** - FluentValidation for request validation
-  **Structured Logging** - Contextual logging with tenant, device, and type information
-  **Health Endpoint** - Basic readiness/liveness checks

## Tech Stack

- **Framework**: .NET 8 / ASP.NET Core (Minimal APIs)
- **Database**: SQLite with Dapper (micro-ORM)
- **Validation**: FluentValidation
- **API Docs**: Swagger
- **Architecture**: Repository Pattern + Service Layer

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [DB Browser for SQLite](https://sqlitebrowser.org/) (optional, for viewing data)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Kallipr-IOT-Monitor-Backend
```

### 2. Restore Dependencies

```bash
cd Kallipr-Web-Api
dotnet restore
```

### 3. Configure Connection String

Update `appsettings.json` if needed (default is fine for local development):

```json
{
  "ConnectionStrings": {
    "KalliprDb": "Data Source=kallipr.db"
  },
  "BatteryLowThreshold": 20
}
```

### 4. Run the Application

```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5264`
- HTTPS: `https://localhost:7162`

### 5. Access API Documentation

- **Swagger UI**: `http://localhost:5264/swagger`

### 6. Test the Health Endpoint

```bash
curl http://localhost:5264/health
```

Response:
```json
{
  "status": "healthy",
  "service": "TelemetryAPI",
  "timestamp": "2025-02-07T15:30:00.000Z"
}
```

## API Endpoints

### POST /api/telemetry
Ingest a single telemetry reading.

**Request Body:**
```json
{
  "tenantId": "acme",
  "deviceId": "dev-123",
  "type": "water_level",
  "value": 1.23,
  "unit": "m",
  "battery": 62,
  "signal": -85,
  "recordedAt": "2025-02-07T10:15:00Z",
  "externalId": "r-789"
}
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "tenantId": "acme",
  "deviceId": "dev-123",
  "type": "water_level",
  "value": 1.23,
  "unit": "m",
  "battery": 62,
  "signal": -85,
  "recordedAt": "2025-02-07T10:15:00Z",
  "externalId": "r-789",
  "createdAt": "2025-02-07T10:16:00Z",
  "batteryLow": false
}
```

### GET /api/telemetry
Query telemetry readings with optional filters and pagination.

**Query Parameters:**
- `deviceId` (optional) - Filter by device ID
- `type` (optional) - Filter by telemetry type
- `startDate` (optional) - Filter from date (ISO 8601)
- `endDate` (optional) - Filter to date (ISO 8601)
- `page` (default: 1) - Page number
- `pageSize` (default: 10) - Items per page

**Example:**
```bash
GET /api/telemetry?deviceId=dev-123&type=water_level&page=1&pageSize=20
```

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": 1,
      "tenantId": "acme",
      "deviceId": "dev-123",
      "type": "water_level",
      "value": 1.23,
      "batteryLow": false,
      ...
    }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 45
}
```

### GET /api/telemetry/{id}
Get a single telemetry reading by ID.

**Response:** `200 OK` or `404 Not Found`

## Database Schema

### Tables

**tenants**
- `id` - Primary key
- `tenant_id` - Unique tenant identifier
- `name` - Tenant name
- `created_at` - Creation timestamp
- `is_active` - Active status

**devices**
- `id` - Primary key
- `device_id` - Unique device identifier
- `tenant_id` - Foreign key to tenants
- `name` - Device name
- `device_type` - Type of device
- `battery_low_threshold` - Battery percentage threshold (default: 20)
- `created_at` - Creation timestamp
- `is_active` - Active status

**telemetry_readings**
- `id` - Primary key
- `tenant_id` - Foreign key to tenants
- `device_id` - Foreign key to devices
- `type` - Reading type (water_level, temperature, etc.)
- `value` - Measurement value
- `unit` - Unit of measurement
- `battery` - Battery percentage (0-100)
- `signal` - Signal strength (dBm)
- `recorded_at` - When reading was recorded
- `external_id` - Device's unique ID for this reading
- `created_at` - When saved to database
- `battery_low` - Computed flag based on device threshold
- **Constraint**: UNIQUE(tenant_id, external_id)

## Project Structure

```
Kallipr-Web-Api/
├── Controllers/
│   └── TelemetryEndpoints.cs      # Minimal API endpoints
├── Data/
│   └── DatabaseInitializer.cs     # Database schema setup
├── Models/
│   ├── Device.cs                  # Device model
│   ├── Tenant.cs                  # Tenant model
│   ├── TelemetryReading.cs        # Telemetry model
│   └── Payloads/
│       └── TelemetryQuery.cs      # Query parameters model
├── Repositories/
│   ├── Interfaces/
│   │   └── ITelemetryRepository.cs
│   └── TelemetryRepository.cs     # Data access with Dapper
├── Services/
│   ├── Interfaces/
│   │   └── ITelemetryService.cs
│   └── TelemetryService.cs        # Business logic
├── Validators/
│   └── TelemetryReadingValidator.cs  # FluentValidation rules
├── Program.cs                     # Application entry point
├── appsettings.json              # Configuration
└── Kallipr-Web-Api.csproj        # Project file
```

## Testing with cURL

**Ingest telemetry:**
```bash
curl -X POST http://localhost:5264/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "acme",
    "deviceId": "dev-123",
    "type": "water_level",
    "value": 1.23,
    "unit": "m",
    "battery": 62,
    "signal": -85,
    "recordedAt": "2025-02-07T10:15:00Z",
    "externalId": "r-789"
  }'
```

**Query telemetry:**
```bash
curl "http://localhost:5264/api/telemetry?deviceId=dev-123&page=1&pageSize=10"
```

**Test duplicate prevention:**
```bash
# Send the same externalId twice - second request should return 409 Conflict
curl -X POST http://localhost:5264/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "acme",
    "deviceId": "dev-123",
    "type": "water_level",
    "value": 1.23,
    "unit": "m",
    "battery": 62,
    "signal": -85,
    "recordedAt": "2025-02-07T10:15:00Z",
    "externalId": "r-789"
  }'
```

## Validation Rules

- `tenantId`, `deviceId`, `type`, `unit`, `externalId` - Required, max 100 chars
- `battery` - Must be 0-100
- `signal` - Must be ≤ 0 (dBm)
- `recordedAt` - Must not be in the future
- Duplicate `externalId` per tenant - Rejected with 409 Conflict

## Error Responses

**400 Bad Request** - Validation failed
```json
{
  "errors": [
    "Battery must be between 0 and 100",
    "TenantId is required"
  ]
}
```

**409 Conflict** - Duplicate externalId
```json
{
  "error": "Duplicate externalId for this tenant"
}
```

**500 Internal Server Error** - Unexpected error
```json
{
  "detail": "An unexpected error occurred",
  "statusCode": 500,
  "title": "Internal Server Error"
}
```

## Viewing the Database

The SQLite database is created at `bin/Debug/net8.0/kallipr.db`.

**Option 1: DB Browser for SQLite**
1. Download from https://sqlitebrowser.org/
2. Open the `.db` file
3. Browse tables and run queries

**Option 2: Command Line**
```bash
sqlite3 bin/Debug/net8.0/kallipr.db
.tables
SELECT * FROM telemetry_readings;
.quit
```

## Sample Data

The database is seeded with:
- **Tenant**: `acme` (ACME Corporation)
- **Device**: `dev-123` (Water Level Sensor 123) with 20% battery threshold

## Development

**Clean and rebuild:**
```bash
dotnet clean
dotnet build
```

**Run with specific port:**
```bash
dotnet run --urls "http://localhost:5264"
```

**Watch mode (auto-reload on file changes):**
```bash
dotnet watch run
```

## Future Enhancements

- Push notifications via Firebase when battery is low
- Authentication & authorization (JWT, API keys)


## License

MIT License

## Author
Michael James Boonekamp
Created as part of the Kallipr Technical Vetting Assignment
