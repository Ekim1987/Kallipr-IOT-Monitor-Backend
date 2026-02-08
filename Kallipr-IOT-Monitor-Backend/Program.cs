using System.Data;
using FluentValidation;
using Kallipr_IOT_Monitor_Backend.Controllers;
using Kallipr_IOT_Monitor_Backend.Data;
using Kallipr_IOT_Monitor_Backend.Models;
using Kallipr_IOT_Monitor_Backend.Repositories;
using Kallipr_IOT_Monitor_Backend.Services;
using Kallipr_IOT_Monitor_Backend.Services.Interfaces;
using Microsoft.Data.Sqlite;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqliteConnection(builder.Configuration.GetConnectionString("KalliprDb")));

builder.Services.AddScoped<IValidator<TelemetryReading>, TelemetryReadingValidator>();
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

var app = builder.Build();
if (!app.Environment.IsEnvironment("Test"))
{
    DatabaseInitializer.Initialize(app.Configuration);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "TelemetryAPI",
    timestamp = DateTime.UtcNow
}));

app.MapTelemetryEndpoints();

app.Run();

public partial class Program { }