using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

string lifeUniverseEverythingMeterName = "fib.life_universe_everything";

// Configure the OpenTelemetry meter provider to add a meter named "OTel.AzureMonitor.Demo".
builder.Services.ConfigureOpenTelemetryMeterProvider((_, builder) =>
    builder.AddMeter(lifeUniverseEverythingMeterName));

builder.Services.AddOpenTelemetry().UseAzureMonitor();

var app = builder.Build();

app.UseHttpsRedirection();

// Create a meter and counter for "life_universe_everything"
Meter lifeUniverseEverythingMeter = new(lifeUniverseEverythingMeterName);
Counter<long> lifeUniverseEverythingCounter = lifeUniverseEverythingMeter.CreateCounter<long>(
    "fib.life_universe_everything.counter",
    "count",
    "Counts the number of times the answer to the life, universe, and everything is attempted to " +
    "be used as a Fibonacci sequence number.");

// GET /fibonacci/{n} - Returns the nth Fibonacci number (n > 0)
app.MapGet("/fibonacci/{n:int}", (int n, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("FibonacciApi");
    if (n < 0)
    {
        logger.LogWarning("Invalid Fibonacci request: n={n}", n);
        return Results.BadRequest(new { error = "n must be a non-negative integer." });
    }

    // Throw an exception the answer to the life, universe, and everything
    if (n == 42)
    {
        lifeUniverseEverythingCounter.Add(1);

        throw new Exception(
            "The answer to the life, universe, and everything is 42.");
    }

    // Calculate nth Fibonacci number iteratively
    ulong Fib(int num)
    {
        if (num == 0) return 0;
        if (num == 1) return 1;
        ulong a = 0, b = 1;
        for (int i = 2; i <= num; i++)
        {
            ulong temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
    var result = Fib(n);
    logger.LogInformation("Fibonacci calculated for n={n}: {result}", n, result);
    return Results.Ok(new { n, fibonacci = result });
})
.WithName("GetFibonacci");

app.Run();


