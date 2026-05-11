namespace MinKlinik.UseCases.Dtos;

// === Request DTO'er til Use Cases (commands) ===

public record OpretKonsultationRequest(
    DateTime Fra,
    DateTime Til,
    Guid BehandlingstypeId,
    Guid PatientId,
    Guid BehandlerId);

public record AfslutKonsultationRequest(
    Guid KonsultationId,
    string Notat);

public record AflysKonsultationRequest(
    Guid KonsultationId);

// === Response DTO'er ===

public record KonsultationDto(
    Guid Id,
    DateTime Fra,
    DateTime Til,
    Guid BehandlingstypeId,
    string BehandlingstypeNavn,
    Guid PatientId,
    string PatientNavn,
    Guid BehandlerId,
    string BehandlerNavn,
    string Status,
    string? Notat);

public record BehandlingstypeDto(
    Guid Id,
    string Navn);

public record PatientDto(
    Guid Id,
    string Navn);

public record BehandlerDto(
    Guid Id,
    string Navn,
    string Speciale);

// === Request DTO'er til Queries ===

public record HentKonsultationRequest(Guid KonsultationId);

public record SøgKonsultationerRequest(
    DateTime? FraDato,
    DateTime? TilDato,
    Guid? BehandlerId,
    Guid? PatientId);

// === Black Friday-simulering (kap. 23 — read-projection) ===
//
// Det her er en projektion, IKKE en command. Strategy-mønstret fra kap. 18
// genbruges på query-siden for at vise et hypotetisk "hvad nu hvis"-scenarie
// uden at mutere konsultations-aggregaterne (jf. kap. 18 §18.3.5 +
// kap. 23 §23.3.1).

public record BlackFridayRapportDto(
    int AntalBookinger,
    int AntalMedRabat,
    double SamletRabat);

public record RabatprojektionDto(
    Guid KonsultationId,
    double EgenbetalingsBeløb,
    double RabatBeløb);
