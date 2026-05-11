using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.Rabat;
using MinKlinik.Domain.ValueObjects;
using FixtureBuilder;
using Xunit;

namespace MinKlinik.Domain.Tests;

public class TidsintervalTests
{
    [Fact]
    public void GivenTilFoerFra_WhenCreatingInterval_ThenThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Tidsinterval(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(1)));
    }

    [Fact]
    public void GivenOverlappingIntervals_WhenCheckingOverlap_ThenReturnsTrue()
    {
        var førsteInterval = new Tidsinterval(DayAt(9), DayAt(10));
        var andetInterval = new Tidsinterval(DayAt(9, 30), DayAt(10, 30));
        Assert.True(førsteInterval.OverlapperMed(andetInterval));
        Assert.True(andetInterval.OverlapperMed(førsteInterval));
    }

    [Fact]
    public void GivenAdjacentIntervals_WhenCheckingOverlap_ThenReturnsFalse()
    {
        var førsteInterval = new Tidsinterval(DayAt(9), DayAt(10));
        var andetInterval = new Tidsinterval(DayAt(10), DayAt(11));
        Assert.False(førsteInterval.OverlapperMed(andetInterval));
    }

    [Fact]
    public void GivenValidInterval_WhenReadingDuration_ThenReturnsExpectedTimespan()
    {
        var interval = new Tidsinterval(DayAt(9), DayAt(10, 30));
        Assert.Equal(TimeSpan.FromMinutes(90), interval.Varighed);
    }

    private DateTime DayAt(int time, int min = 0)
        => DateTime.UtcNow.AddDays(1).Date.AddHours(time).AddMinutes(min);
}

public class KonsultationTests
{
    private readonly Guid _behandlerId = Guid.NewGuid();
    private readonly IEgenBetalingsBeregner _egenbetalingsBeregner = new StubEgenBetalingsBeregner();

    private Tidsinterval TimeRange(int fraTime, int tilTime)
        => new(DateTime.UtcNow.AddDays(1).Date.AddHours(fraTime),
               DateTime.UtcNow.AddDays(1).Date.AddHours(tilTime));

    private static Behandlingstype NewTreatmentType()
        => new("Undersøgelse", new EgenbetalingsBeløb(350));

    private static Patient NewPatient()
        => new("Jens Jensen", "010190-1234");

    private Konsultation CreateWithoutOverlap(
        Tidsinterval? tidspunkt = null,
        Patient? patient = null,
        Guid? behandlerId = null)
    {
        return Konsultation.Opret(
            tidspunkt ?? TimeRange(9, 10),
            NewTreatmentType(),
            patient ?? NewPatient(),
            behandlerId ?? _behandlerId,
            eksisterendeForPatient: Array.Empty<Konsultation>(),
            eksisterendeForBehandler: Array.Empty<Konsultation>(),
            egenbetalingsBeregner: _egenbetalingsBeregner);
    }

    [Fact]
    public void GivenPlannedConsultation_WhenCancelled_ThenStatusIsCancelled()
    {
        var konsultation = CreateWithoutOverlap();
        konsultation.Aflys();
        Assert.Equal(KonsultationStatus.Aflyst, konsultation.Status);
    }

    [Fact]
    public void GivenCompletedConsultation_WhenCancelled_ThenThrowsDomainException()
    {
        var afsluttet = new FixtureFactory()
            .New<Konsultation>()
            .CreateUninitialized()
            .With(k => k.Status, KonsultationStatus.Afsluttet)
            .Build();

        try
        {
            afsluttet.Aflys();
            Assert.Fail("Forventede DomainException blev ikke kastet.");
        }
        catch (DomainException)
        {
            // forventet
        }
    }

    [Fact]
    public void GivenValidInput_WhenCreatingConsultation_ThenStatusIsPlanned()
    {
        var konsultation = CreateWithoutOverlap();
        Assert.Equal(KonsultationStatus.Planlagt, konsultation.Status);
    }

    [Fact]
    public void GivenPastTime_WhenCreatingConsultation_ThenThrowsDomainException()
    {
        var fortid = new Tidsinterval(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(fortid, NewTreatmentType(), NewPatient(), _behandlerId,
                Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), _egenbetalingsBeregner));
    }

    [Fact]
    public void GivenPatientOverlap_WhenCreatingConsultation_ThenThrowsDomainException()
    {
        var patient = NewPatient();
        var eksisterende = Konsultation.Opret(
            TimeRange(9, 10), NewTreatmentType(), patient, _behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), _egenbetalingsBeregner);

        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(
                TimeRange(9, 11), NewTreatmentType(), patient, Guid.NewGuid(),
                eksisterendeForPatient: new[] { eksisterende },
                eksisterendeForBehandler: Array.Empty<Konsultation>(),
                egenbetalingsBeregner: _egenbetalingsBeregner));
    }

    [Fact]
    public void GivenPractitionerOverlap_WhenCreatingConsultation_ThenThrowsDomainException()
    {
        var behandlerId = Guid.NewGuid();
        var patient = NewPatient();
        var eksisterende = Konsultation.Opret(
            TimeRange(9, 10), NewTreatmentType(), patient, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), _egenbetalingsBeregner);

        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(
                TimeRange(9, 11), NewTreatmentType(), NewPatient(), behandlerId,
                eksisterendeForPatient: Array.Empty<Konsultation>(),
                eksisterendeForBehandler: new[] { eksisterende },
                egenbetalingsBeregner: _egenbetalingsBeregner));
    }

    [Fact]
    public void GivenNoOverlap_WhenCreatingConsultation_ThenSucceeds()
    {
        var patient = NewPatient();
        var behandlerId = Guid.NewGuid();
        var eksisterende = Konsultation.Opret(
            TimeRange(9, 10), NewTreatmentType(), patient, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), _egenbetalingsBeregner);

        var nyKonsultation = Konsultation.Opret(
            TimeRange(10, 11), NewTreatmentType(), patient, behandlerId,
            eksisterendeForPatient: new[] { eksisterende },
            eksisterendeForBehandler: new[] { eksisterende },
            egenbetalingsBeregner: _egenbetalingsBeregner);
        Assert.NotNull(nyKonsultation);
    }

    [Fact]
    public void GivenCancelledExistingBooking_WhenCreatingConsultation_ThenSucceeds()
    {
        var patient = NewPatient();
        var behandlerId = Guid.NewGuid();
        var aflyst = Konsultation.Opret(
            TimeRange(9, 10), NewTreatmentType(), patient, behandlerId,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), _egenbetalingsBeregner);
        aflyst.Aflys();

        var nyKonsultation = Konsultation.Opret(
            TimeRange(9, 10), NewTreatmentType(), patient, behandlerId,
            eksisterendeForPatient: new[] { aflyst },
            eksisterendeForBehandler: new[] { aflyst },
            egenbetalingsBeregner: _egenbetalingsBeregner);
        Assert.NotNull(nyKonsultation);
    }

    [Fact]
    public void GivenCompletedConsultation_WhenUpdatingTreatmentType_ThenThrowsDomainException()
    {
        var konsultation = CreateWithoutOverlap();
        konsultation.Afslut("Test-notat");
        Assert.Throws<DomainException>(() => konsultation.OpdaterBehandlingstype(Guid.NewGuid()));
    }

    [Fact]
    public void GivenEmptyNote_WhenCompletingConsultation_ThenThrowsDomainException()
    {
        var konsultation = CreateWithoutOverlap();
        Assert.Throws<DomainException>(() => konsultation.Afslut(""));
    }

    [Fact]
    public void GivenPlannedConsultation_WhenCompletedWithNote_ThenStatusIsCompleted()
    {
        var konsultation = CreateWithoutOverlap();
        konsultation.Afslut("Alt gik godt");
        Assert.Equal(KonsultationStatus.Afsluttet, konsultation.Status);
        Assert.Equal("Alt gik godt", konsultation.Notat);
    }

    [Fact]
    public void GivenCancelledConsultation_WhenCheckingIsActive_ThenReturnsFalse()
    {
        var konsultation = CreateWithoutOverlap();
        konsultation.Aflys();
        Assert.False(konsultation.ErAktiv);
    }
}

public class KonsultationOpretTests
{
    private static readonly IEgenBetalingsBeregner EgenbetalingsBeregner = new StubEgenBetalingsBeregner();

    [Fact]
    public void GivenEmptyPractitionerId_WhenCreatingConsultation_ThenThrowsDomainException()
    {
        var tidspunkt = new Tidsinterval(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        Assert.Throws<DomainException>(() =>
            Konsultation.Opret(
                tidspunkt,
                new Behandlingstype("Undersøgelse", new EgenbetalingsBeløb(200)),
                new Patient("Jens", "010190-1234"),
                Guid.Empty,
                Array.Empty<Konsultation>(),
                Array.Empty<Konsultation>(),
                EgenbetalingsBeregner));
    }
}

internal sealed class StubEgenBetalingsBeregner : IEgenBetalingsBeregner
{
    public EgenbetalingsBeløb BeregnEgenbetalingsBeløb(Tidsinterval tidspunkt, Behandlingstype behandlingstype, Patient patient)
        => new(behandlingstype.EgenbetalingsBeløb.Beløb);
}
