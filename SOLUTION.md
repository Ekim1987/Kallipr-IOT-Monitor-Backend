# Architecture & Design Decisions

## System Overview

IoT telemetry ingestion API built with .NET 8, SQLite, and Dapper. Three-layer architecture: endpoints handle HTTP, services contain business logic, repositories manage data access.

## Why These Choices?

### Layered Architecture
Went with the classic repository + service layer pattern because it keeps things organized. HTTP stuff stays in endpoints, business rules in services, SQL in repositories. Makes testing way easier since you can mock each layer independently.

### Minimal APIs
Used .NET 8's minimal APIs instead of controllers. Less boilerplate, faster to write, and it's what Microsoft recommends now. Organized them into static classes so it doesn't turn into spaghetti.

### Dapper Over EF Core
Picked Dapper because:
- It's fast and lightweight
- I can write the exact SQL I want
- No migrations to deal with
- Perfect for read-heavy telemetry data

EF Core is great, but for this use case it felt like bringing a tank to a knife fight.

### Database Design

```
tenants (1) ──< (N) devices (1) ──< (N) telemetry_readings
```

Three tables with foreign keys. Supports multi-tenant, tracks per-device config (like battery thresholds), and keeps data clean with cascade deletes.

Each device has its own battery threshold in the database. Some devices are solar-powered and can run lower, others need early warning. The `battery_low` flag gets calculated on insert - makes it trivial to add push notifications later.

Added a `UNIQUE(tenant_id, external_id)` constraint so devices can retry failed uploads without creating duplicates. Learned that one the hard way on a previous project.

### Duplicate Prevention

Database enforces uniqueness, service layer checks before insert. When the check fails, you get a clean 409 Conflict instead of a cryptic database error. Costs an extra query but worth it for the UX.

### Validation
Using FluentValidation because it's cleaner than data annotations and easier to test. Battery must be 0-100, timestamps can't be in the future, that kind of thing.

### Error Handling & Logging

Structured logging with context fields:
```csharp
_logger.LogInformation(
    "Ingesting telemetry: TenantId={TenantId}, DeviceId={DeviceId}",
    reading.TenantId, reading.DeviceId
);
```

Different exceptions map to different HTTP codes. Database errors get logged with full context so you can actually debug production issues.

### API Design

```
POST /api/telemetry - ingest reading
GET /api/telemetry?deviceId=X&type=Y&startDate=Z&page=1&pageSize=20
GET /api/telemetry/{id}
```

Pagination is required - you don't want to accidentally return a million readings. Response includes total count for client-side pagination.

Skipped sorting since the assignment said to omit it. Easy to add later if needed.

## What Got Cut (4-hour time limit)


**Nice to have:**
- Auth (JWT or API keys)
- Rate limiting
- Background queue for high-volume ingestion
- Aggregation endpoints (min/max/avg by time period)
- Push notifications when battery is low


## Technical Details

**Indexes** based on common queries:
- device_id, type, recorded_at (frequent filters)
- tenant_id + external_id (duplicate checks)

**Foreign key cascades** so deleting a tenant cleans up everything automatically.

**HTTP status codes:**
- 200 - query success
- 201 - created
- 400 - validation failed
- 404 - not found
- 409 - duplicate
- 500 - server error

## Trade-offs

Repository pattern adds code but makes it testable. Dapper is faster than EF but you write your own SQL. Per-device thresholds cost a query per insert but match real-world requirements. Structured logging is more verbose but essential for production debugging.

All reasonable trade-offs for a production system. Avoided over-engineering while keeping it maintainable.
