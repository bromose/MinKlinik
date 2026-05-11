using MinKlinik.Domain.Entities;

namespace MinKlinik.UseCases.Notifikation;

/// <summary>
/// Orkestrator der sender bekræftelse via *alle* registrerede notifikations-kanaler.
/// Klienter (use case-impl) injicerer denne i stedet for at injicere
/// IEnumerable&lt;INotifikation&gt; direkte — det indkapsler flow-logikken.
/// </summary>
public interface IKonsultationsNotifier
{
    Task NotifierOpretAsync(Konsultation konsultation, CancellationToken ct = default);
}
