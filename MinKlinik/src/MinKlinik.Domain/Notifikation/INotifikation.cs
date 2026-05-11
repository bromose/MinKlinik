using MinKlinik.Domain.Entities;

namespace MinKlinik.Domain.Notifikation;

/// <summary>
/// Kontrakt for notifikationer der sendes til patienter når en konsultation
/// oprettes, ændres eller aflyses.
///
/// Interfacet bor i Domain fordi *evnen til at notificere* er en del af
/// forretnings-modellen (jf. kap. 1 §1.3.9-10). Konkrete implementationer
/// (Email, SMS, Push, Brevpost) bor i Infrastructure og er udskifteligt
/// uden at Domain skal ændres — det er Open/Closed Principle (kap. 2 §2.3.3).
/// </summary>
public interface INotifikation
{
    /// <summary>
    /// Sender bekræftelse til patienten på en oprettet konsultation.
    /// </summary>
    Task SendBekræftelseAsync(Konsultation konsultation, CancellationToken ct = default);
}
