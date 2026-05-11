using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Rabat;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Readers;
using MinKlinik.Infrastructure.Persistence;

namespace MinKlinik.Infrastructure.Readers;

/// <summary>
/// Sekventiel reference-implementation af Black Friday-simulering.
/// Bruges som baseline for benchmark mod den parallelle version
/// (jf. kap. 23 §23.3.1 + §23.4).
/// </summary>
public sealed class BlackFridaySimuleringSekventielQueryImpl : IBlackFridaySimuleringQuery
{
    private readonly AppDbContext _db;
    private readonly BlackFridayRabat _blackFridayRabat;

    public BlackFridaySimuleringSekventielQueryImpl(
        AppDbContext db,
        BlackFridayRabat blackFridayRabat)
    {
        _db = db;
        _blackFridayRabat = blackFridayRabat;
    }

    public async Task<BlackFridayRapportDto> Udfør(DateTime simuleretDato)
    {
        // Step 1: Batch-load alle planlagte konsultationer + relaterede aggregater.
        // AsNoTracking — vi muterer ingenting, det er en read-projection.
        var konsultationer = await _db.Konsultationer
            .AsNoTracking()
            .Where(k => k.Status == KonsultationStatus.Planlagt)
            .ToListAsync();

        var patientIds = konsultationer.Select(k => k.PatientId).Distinct().ToList();
        var patienter = await _db.Patienter
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var behandlingstypeIds = konsultationer.Select(k => k.BehandlingstypeId).Distinct().ToList();
        var behandlingstyper = await _db.Behandlingstyper
            .AsNoTracking()
            .Where(bt => behandlingstypeIds.Contains(bt.Id))
            .ToDictionaryAsync(bt => bt.Id);

        // Step 2: Sekventielt loop — strategi-kald + DTO-mapping.
        var rapport = new List<RabatprojektionDto>(konsultationer.Count);
        foreach (var k in konsultationer)
        {
            var patient = patienter[k.PatientId];
            var bt = behandlingstyper[k.BehandlingstypeId];

            // Genbrug af domain-strategi på query-siden — ren read-projektion.
            // Vi bruger simuleretDato som tidspunkt så Black Friday-reglen
            // udløses uafhængigt af konsultationens faktiske tidspunkt.
            var tidspunkt = new Tidsinterval(simuleretDato, simuleretDato.AddMinutes(15));
            var rabat = _blackFridayRabat.Beregn(tidspunkt, bt, patient);

            rapport.Add(new RabatprojektionDto(
                KonsultationId: k.Id,
                EgenbetalingsBeløb: bt.EgenbetalingsBeløb.Beløb,
                RabatBeløb: rabat.Beløb));
        }

        return new BlackFridayRapportDto(
            AntalBookinger: rapport.Count,
            AntalMedRabat: rapport.Count(r => r.RabatBeløb > 0),
            SamletRabat: rapport.Sum(r => r.RabatBeløb));
    }
}
