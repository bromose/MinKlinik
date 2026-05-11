using Microsoft.Extensions.Logging;
using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Notifikation;

namespace MinKlinik.Infrastructure.Notifikation;

/// <summary>
/// SMS-baseret bekræftelse via en SMS-gateway.
///
/// Stub-implementation: logger til ILogger i stedet for at kalde en rigtig
/// gateway. Pædagogisk eksempel på OCP fra kap. 2 §2.3.3 — SmsNotifikation
/// blev tilføjet *uden* at modificere EmailNotifikation eller selve
/// INotifikation-kontrakten.
/// </summary>
internal sealed class SmsNotifikation : INotifikation
{
    private readonly ILogger<SmsNotifikation> _logger;

    public SmsNotifikation(ILogger<SmsNotifikation> logger)
    {
        _logger = logger;
    }

    public Task SendBekræftelseAsync(Konsultation konsultation, CancellationToken ct = default)
    {
        // Patient har pt. ingen Telefon-property — logger PatientId som proxy.
        // Når Patient.Telefon tilføjes (jf. P3-18), erstattes dette med rigtig gateway-kald.
        _logger.LogInformation(
            "SMS: Bekræftelse sendt for konsultation {KonsultationId} (patient {PatientId}) — tidspunkt {Tidspunkt:HH:mm dd-MM}",
            konsultation.Id,
            konsultation.PatientId,
            konsultation.Tidspunkt.Fra);

        return Task.CompletedTask;
    }
}
