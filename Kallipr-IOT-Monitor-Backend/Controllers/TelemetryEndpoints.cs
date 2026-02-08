using FluentValidation;
using Kallipr_IOT_Monitor_Backend.Models;
using Kallipr_IOT_Monitor_Backend.Models.Payloads;
using Kallipr_IOT_Monitor_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kallipr_IOT_Monitor_Backend.Controllers;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this WebApplication app)
    {
        app.MapPost("/api/telemetry", async (
            [FromBody] TelemetryReading reading, [FromServices] ITelemetryService service,
            IValidator<TelemetryReading> validator) =>
        {
            try
            {
                var validationResult = await validator.ValidateAsync(reading);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new
                    {
                        errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                }

                var result = await service.IngestAsync(reading);
                return Results.Created($"/api/telemetry/{result.Id}", result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Duplicate"))
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return Results.Problem(
                    detail: "An unexpected error occurred",
                    statusCode: 500,
                    title: "Internal Server Error"
                );
            }
        });

        app.MapGet("/api/telemetry", async (
            ITelemetryService service,
            [AsParameters] TelemetryQuery query) =>
        {
            try
            {
                var (data, total) = await service.QueryAsync(
                    query.DeviceId,
                    query.Type,
                    query.StartDate,
                    query.EndDate,
                    query.Page,
                    query.PageSize);

                return Results.Ok(new
                {
                    data,
                    page = query.Page,
                    pageSize = query.PageSize,
                    total
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return Results.Problem(
                    detail: "An unexpected error occurred",
                    statusCode: 500,
                    title: "Internal Server Error"
                );
            }
        });

        app.MapGet("/api/telemetry/{id}", async (int id, [FromServices] ITelemetryService service) =>
        {
            try
            {
                var reading = await service.GetByIdAsync(id);
                return reading is not null ? Results.Ok(reading) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return Results.Problem(
                    detail: "An unexpected error occurred",
                    statusCode: 500,
                    title: "Internal Server Error"
                );
            }
        });
    }
}