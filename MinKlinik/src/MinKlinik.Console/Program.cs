using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Konsultationer;
using MinKlinik.UseCases.Readers;
using MinKlinik.Infrastructure;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.Infrastructure.Readers;

const string ConnectionString =
    @"Server=localhost;Database=MinKlinikDb;Trusted_Connection=True;TrustServerCertificate=True;";

var services = new ServiceCollection();

// Applikationslag — præcis samme extension metoder som Api bruger.
// Console vælger selv DbContext-opsætning (SQL Server LocalDB her).
services
    .AddUseCases()
    .AddInfrastructure(options => options.UseSqlServer(ConnectionString));

var serviceProvider = services.BuildServiceProvider();

// Seed testdata
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    new SeedData().Initialize(db);
}

await RunMenuAsync(serviceProvider);

static async Task RunMenuAsync(IServiceProvider rootSp)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== MinKlinik ===");
        Console.WriteLine("1. Vis stamdata");
        Console.WriteLine("2. Vis konsultationer");
        Console.WriteLine("3. Opret konsultation");
        Console.WriteLine("4. Afslut konsultation");
        Console.WriteLine("5. Aflys konsultation");
        Console.WriteLine("6. Seed mange konsultationer (til benchmark)");
        Console.WriteLine("7. Benchmark Black Friday-rabat (sekventiel vs. parallel)");
        Console.WriteLine("0. Afslut");
        Console.Write("Valg: ");

        var input = Console.ReadLine()?.Trim();
        if (input == "0")
            break;

        using var scope = rootSp.CreateScope();
        var sp = scope.ServiceProvider;

        try
        {
            switch (input)
            {
                case "1":
                    await VisStamdataAsync(sp);
                    break;
                case "2":
                    await VisKonsultationerAsync(sp);
                    break;
                case "3":
                    await OpretKonsultationAsync(sp);
                    break;
                case "4":
                    await AfslutKonsultationAsync(sp);
                    break;
                case "5":
                    await AflysKonsultationAsync(sp);
                    break;
                case "6":
                    await SeedManyKonsultationerAsync(sp);
                    break;
                default:
                    Console.WriteLine("Ugyldigt valg.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl: {ex.Message}");
        }
    }
}

static async Task VisStamdataAsync(IServiceProvider sp)
{
    var behandlingstyper = sp.GetRequiredService<IBehandlingstypeReader>();
    var patienter = sp.GetRequiredService<IPatientReader>();
    var behandlere = sp.GetRequiredService<IBehandlerReader>();

    Console.WriteLine("\n--- Behandlingstyper ---");
    foreach (var b in await behandlingstyper.HentAlleAsync())
        Console.WriteLine($"  {b.Id}: {b.Navn}");

    Console.WriteLine("\n--- Patienter ---");
    foreach (var p in await patienter.HentAlleAsync())
        Console.WriteLine($"  {p.Id}: {p.Navn}");

    Console.WriteLine("\n--- Behandlere ---");
    foreach (var b in await behandlere.HentAlleAsync())
        Console.WriteLine($"  {b.Id}: {b.Navn} ({b.Speciale})");
}

static async Task VisKonsultationerAsync(IServiceProvider sp)
{
    var queries = sp.GetRequiredService<IKonsultationReader>();
    var liste = await queries.HentAlleAsync();

    Console.WriteLine("\n--- Konsultationer ---");
    if (liste.Count == 0)
    {
        Console.WriteLine("  (ingen)");
        return;
    }

    foreach (var k in liste)
    {
        Console.WriteLine($"  {k.Id}");
        Console.WriteLine($"    {k.Fra:yyyy-MM-dd HH:mm} - {k.Til:HH:mm} | {k.PatientNavn} | {k.BehandlerNavn} | {k.BehandlingstypeNavn} | {k.Status}");
    }
}

static async Task OpretKonsultationAsync(IServiceProvider sp)
{
    var behandlingstyper = sp.GetRequiredService<IBehandlingstypeReader>();
    var patienter = sp.GetRequiredService<IPatientReader>();
    var behandlere = sp.GetRequiredService<IBehandlerReader>();
    var useCase = sp.GetRequiredService<IOpretKonsultationUseCase>();

    var typer = (await behandlingstyper.HentAlleAsync()).ToList();
    var patienterListe = (await patienter.HentAlleAsync()).ToList();
    var behandlereListe = (await behandlere.HentAlleAsync()).ToList();

    if (typer.Count == 0 || patienterListe.Count == 0 || behandlereListe.Count == 0)
    {
        Console.WriteLine("Manglende stamdata. Sikr at der findes mindst én behandlingstype, patient og behandler.");
        return;
    }

    Console.Write("Fra (yyyy-MM-dd HH:mm): ");
    if (!DateTime.TryParse(Console.ReadLine(), out var fra))
    {
        Console.WriteLine("Ugyldig dato.");
        return;
    }

    Console.Write("Til (yyyy-MM-dd HH:mm): ");
    if (!DateTime.TryParse(Console.ReadLine(), out var til))
    {
        Console.WriteLine("Ugyldig dato.");
        return;
    }

    var typeId = VælgGuid(typer.Select(t => (t.Id, t.Navn)), "Behandlingstype");
    var patientId = VælgGuid(patienterListe.Select(p => (p.Id, p.Navn)), "Patient");
    var behandlerId = VælgGuid(behandlereListe.Select(b => (b.Id, $"{b.Navn} ({b.Speciale})")), "Behandler");

    if (typeId is null || patientId is null || behandlerId is null)
        return;

    await useCase.Udfør(new OpretKonsultationRequest(fra, til, typeId.Value, patientId.Value, behandlerId.Value));
    Console.WriteLine("Konsultation oprettet.");
}

static async Task AfslutKonsultationAsync(IServiceProvider sp)
{
    var queries = sp.GetRequiredService<IKonsultationReader>();
    var useCase = sp.GetRequiredService<IAfslutKonsultationUseCase>();

    var liste = await queries.HentAlleAsync();
    var aktive = liste.Where(k => k.Status != "Aflyst").ToList();

    if (aktive.Count == 0)
    {
        Console.WriteLine("Ingen aktive konsultationer.");
        return;
    }

    var konsultationId = VælgKonsultationGuid(aktive);
    if (konsultationId is null)
        return;

    Console.Write("Notat: ");
    var notat = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(notat))
    {
        Console.WriteLine("Notat er påkrævet.");
        return;
    }

    await useCase.Udfør(new AfslutKonsultationRequest(konsultationId.Value, notat));
    Console.WriteLine("Konsultation afsluttet.");
}

static async Task AflysKonsultationAsync(IServiceProvider sp)
{
    var queries = sp.GetRequiredService<IKonsultationReader>();
    var useCase = sp.GetRequiredService<IAflysKonsultationUseCase>();

    var liste = await queries.HentAlleAsync();
    var aktive = liste.Where(k => k.Status != "Aflyst").ToList();

    if (aktive.Count == 0)
    {
        Console.WriteLine("Ingen aktive konsultationer.");
        return;
    }

    var konsultationId = VælgKonsultationGuid(aktive);
    if (konsultationId is null)
        return;

    await useCase.Udfør(new AflysKonsultationRequest(konsultationId.Value));
    Console.WriteLine("Konsultation aflyst.");
}

static Guid? VælgGuid<T>(IEnumerable<(Guid Id, T Label)> items, string label)
{
    var liste = items.ToList();
    for (var i = 0; i < liste.Count; i++)
        Console.WriteLine($"  {i + 1}. {liste[i].Label}");

    Console.Write($"Vælg {label} (nr): ");
    if (!int.TryParse(Console.ReadLine(), out var nr) || nr < 1 || nr > liste.Count)
    {
        Console.WriteLine("Ugyldigt valg.");
        return null;
    }

    return liste[nr - 1].Id;
}

static Guid? VælgKonsultationGuid(List<KonsultationDto> liste)
{
    for (var i = 0; i < liste.Count; i++)
    {
        var k = liste[i];
        Console.WriteLine($"  {i + 1}. {k.Fra:yyyy-MM-dd HH:mm} - {k.PatientNavn} - {k.BehandlerNavn} ({k.Status})");
    }

    Console.Write("Vælg konsultation (nr): ");
    if (!int.TryParse(Console.ReadLine(), out var nr) || nr < 1 || nr > liste.Count)
    {
        Console.WriteLine("Ugyldigt valg.");
        return null;
    }

    return liste[nr - 1].Id;
}

// === Seed mange konsultationer (kap. 23 — benchmark-data) =================

static async Task SeedManyKonsultationerAsync(IServiceProvider sp)
{
    Console.Write("Antal konsultationer at oprette (default 12000): ");
    var input = Console.ReadLine()?.Trim();
    if (!int.TryParse(input, out var antal) || antal <= 0)
        antal = 12000;

    var db = sp.GetRequiredService<AppDbContext>();


    // Sikr stamdata findes
    var behandlingstyper = await db.Behandlingstyper.ToListAsync();
    var patienter = await db.Patienter.ToListAsync();
    var behandlere = await db.Behandlere.ToListAsync();

    if (behandlingstyper.Count == 0 || patienter.Count == 0 || behandlere.Count == 0)
    {
        Console.WriteLine("Mangler stamdata. Initialiser DB først.");
        return;
    }

    var sw = Stopwatch.StartNew();
    var rng = new Random(42);
    var basetid = DateTime.UtcNow.AddDays(1).Date.AddHours(8);

    Console.WriteLine($"Opretter {antal} konsultationer …");

    var batch = new List<Konsultation>(1000);
    for (var i = 0; i < antal; i++)
    {
        var bt = behandlingstyper[rng.Next(behandlingstyper.Count)];
        var patient = patienter[rng.Next(patienter.Count)];
        var behandler = behandlere[rng.Next(behandlere.Count)];

        // Spred på 365 dage med tilfældige minutter — vi ignorerer overlaps
        // for benchmark-formålet (vi kalder ikke Konsultation.Opret med
        // overlap-listen; vi seed'er direkte for hastighed).
        var fra = basetid.AddDays(i % 365).AddMinutes(rng.Next(60 * 8));
        var til = fra.AddMinutes(15);
        var tidspunkt = new Tidsinterval(fra, til);

        var konsultation = Konsultation.Opret(
            tidspunkt,
            bt.Id,
            patient.Id,
            behandler.Id,
            eksisterendeForPatient: Array.Empty<Konsultation>(),
            eksisterendeForBehandler: Array.Empty<Konsultation>());

        batch.Add(konsultation);

        if (batch.Count >= 1000)
        {
            db.Konsultationer.AddRange(batch);
            await db.SaveChangesAsync();
            batch.Clear();
        }
    }

    if (batch.Count > 0)
    {
        db.Konsultationer.AddRange(batch);
        await db.SaveChangesAsync();
    }

    sw.Stop();
    var totalNu = await db.Konsultationer.CountAsync();
    Console.WriteLine($"Færdig. Tilføjet {antal} på {sw.ElapsedMilliseconds:N0} ms. Total i DB: {totalNu:N0}.");
}

// === Benchmark Black Friday-rabat (kap. 23 §23.4) =========================



