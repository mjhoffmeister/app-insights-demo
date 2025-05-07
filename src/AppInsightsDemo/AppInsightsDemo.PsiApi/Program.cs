using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Add HttpClient factory for making HTTP calls
builder.Services.AddHttpClient();

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


// GET /fibonacci/{n} - Calls Omega API to get the nth Fibonacci number
app.MapGet("/fibonacci/{n:int}", async ([FromRoute] int n, [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IConfiguration config, [FromServices] ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("PsiApi");
    var http = httpClientFactory.CreateClient();
    // Get Omega API base URL from configuration or use default
    var omegaBaseUri = config["OmegaBaseUri"];
    var url = $"{omegaBaseUri}/fibonacci/{n}";
    try
    {
        logger.LogInformation("Calling Omega API for n={n}", n);
        var response = await http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Omega API error for n={n}: {error}", n, error);
            return Results.Problem($"Omega API error: {error}", statusCode: (int)response.StatusCode);
        }
        var json = await response.Content.ReadAsStringAsync();
        logger.LogInformation("Omega API call succeeded for n={n}", n);
        return Results.Content(json, "application/json");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to call Omega API for n={n}", n);
        return Results.Problem($"Failed to call Omega API: {ex.Message}");
    }
})
.WithName("GetFibonacciFromOmega");

app.Run();