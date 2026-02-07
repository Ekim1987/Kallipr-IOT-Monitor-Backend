using System.Data;
using FluentValidation;
using Kallipr_Web_Api.Controllers;
using Kallipr_Web_Api.Data;
using Kallipr_Web_Api.Models;
using Kallipr_Web_Api.Repositories;
using Kallipr_Web_Api.Services;
using Kallipr_Web_Api.Services.Interfaces;
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

DatabaseInitializer.Initialize(app.Configuration);
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