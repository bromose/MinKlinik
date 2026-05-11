using Microsoft.EntityFrameworkCore;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Readers;
using MinKlinik.Infrastructure.Persistence;

namespace MinKlinik.Infrastructure.Readers;

internal sealed class KonsultationReader : IKonsultationReader
{
    private readonly AppDbContext _db;

    public KonsultationReader(AppDbContext db) => _db = db;

    public async Task<KonsultationDto?> HentAsync(Guid id)
    {
        // Ingen Include — vi har kun Guid-referencer, ingen navigation properties.
        // Navn-felter hentes via separate lookups eller joins.
        return await _db.Konsultationer
            .AsNoTracking()
            .Where(k => k.Id == id)
            .Select(k => new KonsultationDto(
                k.Id,
                k.Tidspunkt.Fra,
                k.Tidspunkt.Til,
                k.BehandlingstypeId,
                _db.Behandlingstyper.Where(bt => bt.Id == k.BehandlingstypeId).Select(bt => bt.Navn).FirstOrDefault() ?? "",
                k.PatientId,
                _db.Patienter.Where(p => p.Id == k.PatientId).Select(p => p.Navn).FirstOrDefault() ?? "",
                k.BehandlerId,
                _db.Behandlere.Where(b => b.Id == k.BehandlerId).Select(b => b.Navn).FirstOrDefault() ?? "",
                k.Status.ToString(),
                k.Notat))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<KonsultationDto>> HentAlleAsync()
    {
        return await HentAlleQueryable().ToListAsync();
    }

    public async Task<IReadOnlyList<KonsultationDto>> HentForMånedAsync(int år, int måned)
    {
        return await HentAlleQueryable().Where(a => a.Fra.Year == år && a.Fra.Month == måned).ToListAsync();
    }

    private IQueryable<KonsultationDto> HentAlleQueryable()
    {
        return _db.Konsultationer
            .AsNoTracking()
            .Select(k => new KonsultationDto(
                k.Id,
                k.Tidspunkt.Fra,
                k.Tidspunkt.Til,
                k.BehandlingstypeId,
                _db.Behandlingstyper.Where(bt => bt.Id == k.BehandlingstypeId).Select(bt => bt.Navn).FirstOrDefault() ??
                "",
                k.PatientId,
                _db.Patienter.Where(p => p.Id == k.PatientId).Select(p => p.Navn).FirstOrDefault() ?? "",
                k.BehandlerId,
                _db.Behandlere.Where(b => b.Id == k.BehandlerId).Select(b => b.Navn).FirstOrDefault() ?? "",
                k.Status.ToString(),
                k.Notat)).AsQueryable();
    }
}

internal sealed class BehandlingstypeReader : IBehandlingstypeReader
{
    private readonly AppDbContext _db;
    public BehandlingstypeReader(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BehandlingstypeDto>> HentAlleAsync()
        => await _db.Behandlingstyper.AsNoTracking()
            .Select(b => new BehandlingstypeDto(b.Id, b.Navn))
            .ToListAsync();
}

internal sealed class PatientReader : IPatientReader
{
    private readonly AppDbContext _db;
    public PatientReader(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PatientDto>> HentAlleAsync()
        => await _db.Patienter.AsNoTracking()
            .Select(p => new PatientDto(p.Id, p.Navn))
            .ToListAsync();
}

internal sealed class BehandlerReader : IBehandlerReader
{
    private readonly AppDbContext _db;
    public BehandlerReader(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BehandlerDto>> HentAlleAsync()
        => await _db.Behandlere.AsNoTracking()
            .Select(b => new BehandlerDto(b.Id, b.Navn, b.Speciale))
            .ToListAsync();
}
