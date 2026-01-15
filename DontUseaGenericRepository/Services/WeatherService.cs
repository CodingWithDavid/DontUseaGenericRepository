using DontUseaGenericRepository.Data;
using DontUseaGenericRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace DontUseaGenericRepository.Services;

/// <summary>
/// Weather service demonstrating EF Core usage WITHOUT a generic repository.
/// Uses IDbContextFactory for proper DbContext lifetime management in Blazor Server.
/// Each method creates a short-lived DbContext instance via the factory.
/// </summary>
public class WeatherService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WeatherService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Gets all weather forecasts ordered by date.
    /// </summary>
    public async Task<List<WeatherForecast>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WeatherForecasts
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a weather forecast by its ID.
    /// </summary>
    public async Task<WeatherForecast?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WeatherForecasts.FindAsync(id);
    }

    /// <summary>
    /// Creates a new weather forecast.
    /// </summary>
    public async Task<WeatherForecast> CreateAsync(WeatherForecast forecast)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.WeatherForecasts.Add(forecast);
        await context.SaveChangesAsync();
        return forecast;
    }

    /// <summary>
    /// Updates an existing weather forecast.
    /// </summary>
    public async Task UpdateAsync(WeatherForecast forecast)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.WeatherForecasts.Update(forecast);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a weather forecast by its ID.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var forecast = await context.WeatherForecasts.FindAsync(id);
        if (forecast is not null)
        {
            context.WeatherForecasts.Remove(forecast);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets available weather summary options.
    /// </summary>
    public static string[] GetSummaryOptions() =>
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
}
