using Microsoft.Extensions.Logging;
using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Notifikation;

namespace MinKlinik.Infrastructure.Notifikation;

/// <summary>
/// E-mail-baseret bekræftelse via SMTP.
///
/// Stub-implementation: logger til ILogger i stedet for at kalde en rigtig
/// SMTP-server. Pædagogisk eksempel på en INotifikation-impl (kap. 1 §1.3.10).
///
/// I produktion ville konstruktøren tage en IEmailKlient (SmtpClient eller
/// en SaaS-wrapper som SendGrid/Mailgun) som dependency.
/// </summary>
internal sealed class EmailNotifikation : INotifikation
{
    private readonly ILogger<EmailNotifikation> _logger;

    public EmailNotifikation(ILogger<EmailNotifikation> logger)
    {
        _logger = logger;
    }

    public Task SendBekræftelseAsync(Konsultation konsultation, CancellationToken ct = default)
    {
        // Patient har pt. ingen Email-property — logger PatientId som proxy.
        // Når Patient.Email tilføjes (jf. P3-18), erstattes dette med rigtig SMTP-kald.
        _logger.LogInformation(
            "EMAIL: Bekræftelse sendt for konsultation {KonsultationId} (patient {PatientId}) — tidspunkt {Tidspunkt:yyyy-MM-dd HH:mm}",
            konsultation.Id,
            konsultation.PatientId,
            konsultation.Tidspunkt.Fra);

        return Task.CompletedTask;
    }
}
