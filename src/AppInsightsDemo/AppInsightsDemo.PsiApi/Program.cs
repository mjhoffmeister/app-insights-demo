using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().UseAzureMonitor();

// Add HttpClient factory for making HTTP calls
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseHttpsRedirection();


// GET /fibonacci/{n} - Calls Omega API to get the nth Fibonacci number
app.MapGet("/fibonacci/{n:int}", async ([FromRoute] int n, [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IConfiguration config, [FromServices] ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("PsiApi");
    var http = httpClientFactory.CreateClient();

    // Get Omega API base URL from configuration or use default
    var omegaBaseUri = config["OmegaBaseUri"];
    var url = $"{omegaBaseUri}/fibonacci/{n}";

    // Try to get the Fibonacci number from Omega API
    try
    {
        logger.LogInformation("Calling Omega API for n={n}", n);
        var response = await http.GetAsync(url);

        // On a failure, log the error and return the response from Omega API
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Omega API error for n={n}: {error}", n, error);
            return Results.Problem($"Omega API error: {error}", statusCode: (int)response.StatusCode);
        }

        // On success, read the response and return it
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