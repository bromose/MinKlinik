using MinKlinik.Domain.Rabat;
using MinKlinik.UseCases.Konsultationer;
using MinKlinik.UseCases.Notifikation;

// Bevidst placeret i Microsoft-namespace så composition root får extension metoden
// ind uden at skulle tilføje et ekstra using. Samme konvention som AddControllers,
// AddDbContext osv.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registrerer alle use case-implementeringer (application layer).
/// </summary>
public static class UseCasesServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IOpretKonsultationUseCase, OpretKonsultationUseCase>();
        services.AddScoped<IAfslutKonsultationUseCase, AfslutKonsultationUseCase>();
        services.AddScoped<IAflysKonsultationUseCase, AflysKonsultationUseCase>();

        // Notifikations-orkestrator (kap. 1 §1.3.10 + kap. 2 §2.3.3 — polymorfi via DI)
        services.AddScoped<IKonsultationsNotifier, KonsultationsNotifier>();

        // Domainservices
        services.AddScoped<IRabatStrategi, StandardRabat>();
        services.AddScoped<IRabatStrategi, StudenterRabat>();
        services.AddScoped<IRabatStrategi, SeniorRabat>();
        services.AddScoped<IRabatStrategi, BlackFridayRabat>();
        services.AddScoped<IEgenBetalingsBeregner, EgenBetalingsBeregner>();

        return services;
    }
}
