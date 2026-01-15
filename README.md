# Don't Use a Generic Repository

A Blazor Server application demonstrating how to use **Entity Framework Core without a generic repository pattern**.

## Why Avoid Generic Repositories?

Generic repository patterns (`IRepository<T>`) are often seen as an anti-pattern when used with EF Core because:

1. **EF Core's DbContext is already a Unit of Work + Repository** – `DbSet<T>` provides all CRUD operations out of the box
2. **Leaky abstractions** – Generic repositories often leak EF Core concepts or require workarounds for complex queries
3. **Loss of EF Core features** – Features like `Include()`, `AsNoTracking()`, and raw SQL become harder to use
4. **Unnecessary abstraction layer** – Adds complexity without real benefits for testability (EF Core has `InMemory` provider)

## Architecture

This project uses **purpose-specific services** that directly consume `IDbContextFactory<AppDbContext>`:

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────┐
│  Weather.razor  │────▶│  WeatherService  │────▶│ AppDbContext│
│    (UI Layer)   │     │ (Business Logic) │     │  (EF Core)  │
└─────────────────┘     └──────────────────┘     └─────────────┘
```

### Key Components

| Component                        | Description                                         |
| -------------------------------- | --------------------------------------------------- |
| `Models/WeatherForecast.cs`      | Entity model with properties for weather data       |
| `Data/AppDbContext.cs`           | EF Core DbContext with `DbSet<WeatherForecast>`     |
| `Services/WeatherService.cs`     | Purpose-specific service with explicit CRUD methods |
| `Components/Pages/Weather.razor` | Blazor page with full CRUD UI                       |

## Why IDbContextFactory?

Blazor Server components have **long lifetimes** (they persist across user interactions), but `DbContext` instances should be **short-lived**. Using `IDbContextFactory<TContext>` solves this:

```csharp
public class WeatherService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public async Task<List<WeatherForecast>> GetAllAsync()
    {
        // Creates a new short-lived DbContext for this operation
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.WeatherForecasts.OrderBy(w => w.Date).ToListAsync();
    }
}
```

Each method creates and disposes its own `DbContext` instance, ensuring proper resource management.

## Tech Stack

- **.NET 10.0**
- **Blazor Server** (Interactive Server rendering)
- **Entity Framework Core 9.0**
- **SQLite** (file-based database: `weather.db`)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the Application

```bash
cd DontUseaGenericRepository
dotnet run
```

Navigate to `https://localhost:<port>/weather` to see the CRUD operations in action.

### Database

The SQLite database (`weather.db`) is automatically created on first run with seed data. To reset:

```bash
del weather.db   # Windows
rm weather.db    # Linux/macOS
dotnet run       # Recreates with fresh seed data
```

## Project Structure

```
DontUseaGenericRepository/
├── Data/
│   └── AppDbContext.cs           # EF Core DbContext
├── Models/
│   └── WeatherForecast.cs        # Entity model
├── Services/
│   └── WeatherService.cs         # Business logic (no generic repo!)
├── Components/
│   ├── Pages/
│   │   └── Weather.razor         # CRUD UI
│   └── Layout/
│       └── ...
├── Program.cs                    # Service registration & app config
├── appsettings.json              # Connection string
└── DontUseaGenericRepository.csproj
```

## The WeatherService Pattern

Instead of a generic `IRepository<T>.Add(T entity)`, we have explicit, intention-revealing methods:

```csharp
public class WeatherService
{
    Task<List<WeatherForecast>> GetAllAsync();
    Task<WeatherForecast?> GetByIdAsync(int id);
    Task<WeatherForecast> CreateAsync(WeatherForecast forecast);
    Task UpdateAsync(WeatherForecast forecast);
    Task DeleteAsync(int id);
}
```

**Benefits:**
- ✅ Clear API contract
- ✅ Easy to add business logic per operation
- ✅ Full access to EF Core features (Include, AsNoTracking, raw SQL)
- ✅ Easier to test with mocks or EF Core InMemory provider
- ✅ No abstraction leakage

## License

MIT
