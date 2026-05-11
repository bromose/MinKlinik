using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinKlinik.Domain.Notifikation;
using MinKlinik.Domain.Rabat;
using MinKlinik.Infrastructure.Notifikation;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.Infrastructure.Readers;
using MinKlinik.Infrastructure.Repositories;
using MinKlinik.UseCases;
using MinKlinik.UseCases.Readers;

// Bevidst placeret i Microsoft-namespace så composition root får extension metoden
// ind uden at skulle tilføje et ekstra using.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Registrerer alle infrastruktur-implementeringer: DbContext, repositories og readers.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    // Overload 1: Læs connection string fra IConfiguration.
    // Hvis ingen connection string er konfigureret, falder vi tilbage til in-memory —
    // det er praktisk for udvikling og integrationstests.
    
    // HUSK: Database
    // https://github.com/dotnet/SqlClient/issues/2239
    // https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli
    // Add-Migration InitialMigration -Context AppDbContext -Project MinKlinik.Infrastructure
    // Update-Database -Context AppDbContext -Project MinKlinik.Infrastructure
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sqlServerString = configuration.GetConnectionString("SqlServer");
        if (!string.IsNullOrWhiteSpace(sqlServerString))
            return services.AddInfrastructure(options => options.UseSqlServer(sqlServerString));

        var sqliteString = configuration.GetConnectionString("Sqlite");
        if (!string.IsNullOrWhiteSpace(sqliteString))
            return services.AddInfrastructure(options => options.UseSqlite(sqliteString));

        // Hvis der ikke er opsat en database anvendes SQLite in-memory
        // SQLite in-memory kræver en åben forbindelse for at databasen lever på tværs af scopes.
        var sqliteConnection = new SqliteConnection("DataSource=:memory:");
        sqliteConnection.Open();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>());
        });

        RegisterRepositoriesAndReaders(services);
        return services;
    }

    // Overload 2: Kalderen bestemmer selv hvordan DbContext'en opsættes.
    // Bruges fx fra Console, tests eller andre scenarier uden IConfiguration.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.LogTo(Console.WriteLine);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            configureDb(options);
        });

        RegisterRepositoriesAndReaders(services);
        return services;
    }

    private static void RegisterRepositoriesAndReaders(IServiceCollection services)
    {
        // Repositories (use case-internal: aggregat-CRUD)
        services.AddScoped<IKonsultationRepository, KonsultationRepository>();
        services.AddScoped<IBehandlingstypeRepository, BehandlingstypeRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IBehandlerRepository, BehandlerRepository>();

        // Readers (klient-vendt: DTO-læsning, jf. kap. 12 §12.3.5 —
        // implementeres direkte i Infrastructure uden om use case-laget)
        services.AddScoped<IKonsultationReader, KonsultationReader>();
        services.AddScoped<IBehandlingstypeReader, BehandlingstypeReader>();
        services.AddScoped<IPatientReader, PatientReader>();
        services.AddScoped<IBehandlerReader, BehandlerReader>();

        // Notifikations-kanaler (kap. 1 §1.3.10 + kap. 2 §2.3.3 — flere impl
        // af samme interface; KonsultationsNotifier i UseCases injicerer
        // IEnumerable<INotifikation> og kalder dem alle).
        services.AddSingleton<INotifikation, EmailNotifikation>();
        services.AddSingleton<INotifikation, SmsNotifikation>();

        // Black Friday-simulering (kap. 23 — read-projection).
        // Vi registrerer den parallelle som default — den sekventielle er
        // kun til benchmark-sammenligning fra Console-appen.
        services.AddScoped<BlackFridaySimuleringSekventielQueryImpl>();
        services.AddScoped<IBlackFridaySimuleringQuery, BlackFridaySimuleringQueryImpl>();

        // BlackFridayRabat skal også kunne injiceres som konkret type,
        // ikke bare som IRabatStrategi (jf. kap. 23 §23.3.1 — query handlers
        // bruger den specifikke strategi, ikke hele EgenBetalingsBeregner).
        services.AddScoped<BlackFridayRabat>();
    }
}