using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Notifikation;

namespace MinKlinik.UseCases.Notifikation;

/// <summary>
/// Orkestrator-impl: kalder alle registrerede INotifikation-kanaler parallelt.
///
/// Eksempel på polymorfi via DI (kap. 1 §1.3.10 + kap. 2 §2.3.3 OCP):
/// containeren leverer alle INotifikation-implementationer via
/// IEnumerable&lt;INotifikation&gt;. Når en ny kanal tilføjes (fx PushNotifikation
/// eller BrevpostNotifikation), registreres den i Composition Root og
/// inkluderes automatisk — KonsultationsNotifier ændres ikke.
/// </summary>
internal sealed class KonsultationsNotifier : IKonsultationsNotifier
{
    private readonly IEnumerable<INotifikation> _kanaler;

    public KonsultationsNotifier(IEnumerable<INotifikation> kanaler)
    {
        _kanaler = kanaler;
    }

    public Task NotifierOpretAsync(Konsultation konsultation, CancellationToken ct = default)
        => Task.WhenAll(_kanaler.Select(n => n.SendBekræftelseAsync(konsultation, ct)));
}
