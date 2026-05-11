using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Readers;

// Reader-interfaces: klient-vendt, returnerer DTO'er (aldrig domain entities).
// Implementeres direkte i Infrastructure (jf. kap. 11 §11.3.5 — queries går direkte
// fra UseCases-kontraktens Reader til Infrastructure, uden om en internal use case-impl).
//
// Sammenlign med IRepository-interfaces (i UseCases/IRepositories.cs):
// - Reader   = klient ↔ DTO   (det public surface)
// - Repository = use case-impl ↔ Aggregate Root  (det internt værktøj)

public interface IKonsultationReader
{
    Task<KonsultationDto?> HentAsync(Guid id);
    Task<IReadOnlyList<KonsultationDto>> HentAlleAsync();
}

public interface IBehandlingstypeReader
{
    Task<IReadOnlyList<BehandlingstypeDto>> HentAlleAsync();
}

public interface IPatientReader
{
    Task<IReadOnlyList<PatientDto>> HentAlleAsync();
}

public interface IBehandlerReader
{
    Task<IReadOnlyList<BehandlerDto>> HentAlleAsync();
}

// Black Friday-simulering (kap. 23 — read-projection).
// "Query" beholdes for enkelt-action computational handlers (CQRS-stil),
// hvor en "Reader" er en repository-lignende klasse med flere read-metoder.
// Returnerer en hypotetisk rapport for hvad rabatterne ville være hvis
// "simuleretDato" var den dag bookingerne blev oprettet. Aggregaterne
// muteres aldrig (jf. kap. 18 §18.3.5 + kap. 23 §23.3.1).
public interface IBlackFridaySimuleringQuery
{
    Task<BlackFridayRapportDto> Udfør(DateTime simuleretDato);
}
