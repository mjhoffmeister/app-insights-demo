using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().UseAzureMonitor();

var app = builder.Build();

app.UseHttpsRedirection();


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


