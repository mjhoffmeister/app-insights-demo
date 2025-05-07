
var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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


