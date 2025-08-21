using AdPlacementService.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var adPlatforms = new List<AdPlatform>();

#region API Endpoints

app.MapPost("/api/load", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Results.BadRequest("Content is empty");
        }

        adPlatforms.Clear();
        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length != 2) continue;

            var name = parts[0].Trim();
            if (string.IsNullOrEmpty(name)) continue;

            var locations = parts[1]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(loc => loc.Trim())
                .Where(loc => !string.IsNullOrEmpty(loc))
                .ToHashSet();

            adPlatforms.Add(new AdPlatform
            {
                Name = name,
                Locations = locations
            });
        }

        return Results.Ok($"Data loaded successfully. Loaded {adPlatforms.Count} platforms.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error loading data: {ex.Message}");
    }
});

app.MapGet("/api/search/{*location}", (string location) =>
{
    if (adPlatforms.Count == 0)
        return Results.Ok("Before searching, you should upload the data to the server.");

    try
    {
        var decodedLocation = "/" + Uri.UnescapeDataString(location);

        var result = adPlatforms
            .Where(platform => platform.Locations.Any(loc =>
                decodedLocation.Equals(loc, StringComparison.Ordinal) ||
                decodedLocation.StartsWith(loc + "/", StringComparison.Ordinal)))
            .Select(platform => platform.Name)
            .ToHashSet()
            .ToList();

        return result.Count > 0
            ? Results.Ok(result)
            : Results.Ok($"The specified location '{decodedLocation}' was not found");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error searching platforms: {ex.Message}");
    }
});

#endregion

app.Run();

#if TESTING
public partial class Program { }

#endif