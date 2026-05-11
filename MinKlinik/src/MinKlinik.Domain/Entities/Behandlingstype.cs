using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Entities;

/// <summary>
///     AGGREGATE ROOT: Behandlingstype
///     Identificeret som Aggregate Root fordi:
///     1. Egen livscyklus — behandlingstyper administreres uafhængigt (stamdata)
///     2. Transaktionsgrænse — ændres uafhængigt af konsultationer
///     3. Eget repository — IBehandlingstypeRepository
///     4. Refereres via FK fra Konsultation
/// </summary>
public class Behandlingstype : AggregateRoot
{
    public string Navn { get; private set; } = string.Empty;
    public EgenbetalingsBeløb EgenbetalingsBeløb { get; private set; } = new(0);

    public bool ErBetalingsYdelse => EgenbetalingsBeløb.Beløb > 0;

    // Parameterløs constructor til EF Core
    private Behandlingstype()
    {
    }

    public Behandlingstype(string navn, EgenbetalingsBeløb egenbetalingsBeløb)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new DomainException("Behandlingstype skal have et navn.");

        Id = Guid.NewGuid();
        Navn = navn;
        EgenbetalingsBeløb = egenbetalingsBeløb;
    }

    public void OpdaterEgenbetalingsBeløb(double beløb)
    {
        EgenbetalingsBeløb = new EgenbetalingsBeløb(beløb);
    }
}