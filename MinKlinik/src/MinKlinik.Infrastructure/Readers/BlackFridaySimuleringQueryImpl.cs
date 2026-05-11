using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Rabat;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Readers;
using MinKlinik.Infrastructure.Persistence;

namespace MinKlinik.Infrastructure.Readers;

/// <summary>
/// Parallel implementation af Black Friday-simulering.
/// Bruger Parallel.ForEachAsync + ConcurrentBag for at fordele
/// strategi-beregningen ud over CPU-kerner (jf. kap. 23 §23.3.3).
/// Strategy er stateless og dermed thread-safe (jf. kap. 23 §23.3.5).
/// </summary>
internal sealed class BlackFridaySimuleringQueryImpl : IBlackFridaySimuleringQuery
{
    private readonly AppDbContext _db;
    private readonly BlackFridayRabat _blackFridayRabat;

    public BlackFridaySimuleringQueryImpl(
        AppDbContext db,
        BlackFridayRabat blackFridayRabat)
    {
        _db = db;
        _blackFridayRabat = blackFridayRabat;
    }

    public async Task<BlackFridayRapportDto> Udfør(DateTime simuleretDato)
    {
        // Step 1: Batch-load (samme som sekventiel — undgå N+1, jf. §23.3.2).
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

        // Step 2: Parallel beregning. ConcurrentBag tillader flere tråde at
        // tilføje samtidigt uden lock (jf. §23.3.3).
        var rapport = new ConcurrentBag<RabatprojektionDto>();

        await Parallel.ForEachAsync(
            konsultationer,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (k, ct) =>
            {
                var patient = patienter[k.PatientId];
                var bt = behandlingstyper[k.BehandlingstypeId];

                var tidspunkt = new Tidsinterval(simuleretDato, simuleretDato.AddMinutes(15));
                var rabat = _blackFridayRabat.Beregn(tidspunkt, bt, patient);

                rapport.Add(new RabatprojektionDto(
                    KonsultationId: k.Id,
                    EgenbetalingsBeløb: bt.EgenbetalingsBeløb.Beløb,
                    RabatBeløb: rabat.Beløb));

                // Body skal være async for Parallel.ForEachAsync — vi har
                // dog intet faktisk async-arbejde her (alt I/O er færdigt).
                await Task.CompletedTask;
            });

        return new BlackFridayRapportDto(
            AntalBookinger: rapport.Count,
            AntalMedRabat: rapport.Count(r => r.RabatBeløb > 0),
            SamletRabat: rapport.Sum(r => r.RabatBeløb));
    }
}
