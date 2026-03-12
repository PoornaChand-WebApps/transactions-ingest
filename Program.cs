using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using CodingExerciseTransactions.Utils;
using CodingExerciseTransactions.Services;
using CodingExerciseTransactions.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Read configuration
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "transactions.db")}";
}
var feedPath = builder.Configuration["FeedSettings:MockFeedPath"] ?? "mock-data.json";

// Resolve relative paths to application base directory
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    var ds = connectionString.Substring("Data Source=".Length);
    if (!Path.IsPathRooted(ds))
    {
        var full = Path.Combine(AppContext.BaseDirectory, ds);
        connectionString = $"Data Source={full}";
    }
}

if (!string.IsNullOrEmpty(feedPath) && !Path.IsPathRooted(feedPath))
{
    feedPath = Path.Combine(AppContext.BaseDirectory, feedPath);
}

// Log resolved feed path for troubleshooting
Console.WriteLine($"Resolved feed path: {feedPath}");

// Register services
Console.WriteLine($"Using connection string: {connectionString}");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSingleton<Messages>();
builder.Services.AddScoped<TransactionService>();

var host = builder.Build();

// Run single ingestion job
using var scope = host.Services.CreateScope();

var messages = scope.ServiceProvider.GetRequiredService<Messages>();
var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();

messages.LogInformation("Transaction ingestion started.");

transactionService.ProcessSnapshot(feedPath);

messages.LogInformation("Transaction ingestion finished.");