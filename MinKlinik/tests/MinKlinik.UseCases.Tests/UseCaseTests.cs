using Moq;
using Xunit;
using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.Rabat;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases;
using MinKlinik.UseCases.Konsultationer;
using MinKlinik.UseCases.Notifikation;

namespace MinKlinik.UseCases.Tests;

public class OpretKonsultationUseCaseTests
{
    private readonly Mock<IKonsultationRepository> _mockKonsRepo = new();
    private readonly Mock<IBehandlingstypeRepository> _mockBehandTypeRepo = new();
    private readonly Mock<IPatientRepository> _mockPatientRepo = new();
    private readonly Mock<IBehandlerRepository> _mockBehandlerRepo = new();
    private readonly Mock<IEgenBetalingsBeregner> _mockEgenbetalingsBeregner = new();
    private readonly Mock<IKonsultationsNotifier> _mockNotifier = new();

    private OpretKonsultationUseCase CreateSut() => new(
        _mockKonsRepo.Object,
        _mockBehandTypeRepo.Object,
        _mockPatientRepo.Object,
        _mockBehandlerRepo.Object,
        _mockEgenbetalingsBeregner.Object,
        _mockNotifier.Object);

    private static Behandlingstype NewTreatmentType(double beløb = 300)
        => new("Undersøgelse", new EgenbetalingsBeløb(beløb));

    private static Patient NewPatient()
        => new("Jens", "010190-1234");

    private static Domain.ValueObjects.Tidsinterval TimeRange(DateTime fra, DateTime til)
        => new(fra, til);

    private void SetupCoPayment(double beløb = 0)
    {
        _mockEgenbetalingsBeregner
            .Setup(r => r.BeregnEgenbetalingsBeløb(It.IsAny<Domain.ValueObjects.Tidsinterval>(), It.IsAny<Behandlingstype>(), It.IsAny<Patient>()))
            .Returns(new EgenbetalingsBeløb(beløb));
    }

    [Fact]
    public async Task GivenValidRequest_WhenExecutingUseCase_ThenAddsConsultationAndSaves()
    {
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var behandlerId = Guid.NewGuid();

        _mockBehandTypeRepo.Setup(r => r.HentAsync(typeId))
            .ReturnsAsync(NewTreatmentType());
        _mockPatientRepo.Setup(r => r.HentAsync(patientId))
            .ReturnsAsync(NewPatient());
        _mockBehandlerRepo.Setup(r => r.HentAsync(behandlerId))
            .ReturnsAsync(new Behandler("Dr. Pia", "Almen medicin"));
        _mockKonsRepo.Setup(r => r.HentForPatientAsync(patientId))
            .ReturnsAsync(new List<Konsultation>());
        _mockKonsRepo.Setup(r => r.HentForBehandlerAsync(behandlerId))
            .ReturnsAsync(new List<Konsultation>());
        SetupCoPayment();

        var request = new OpretKonsultationRequest(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            typeId, patientId, behandlerId);

        await CreateSut().Udfør(request);

        _mockKonsRepo.Verify(r => r.TilføjAsync(It.IsAny<Konsultation>()), Times.Once);
        _mockKonsRepo.Verify(r => r.GemAsync(), Times.Once);
        _mockNotifier.Verify(
            n => n.NotifierOpretAsync(It.IsAny<Konsultation>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenUnknownTreatmentType_WhenExecutingUseCase_ThenThrowsNotFoundException()
    {
        _mockBehandTypeRepo.Setup(r => r.HentAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Behandlingstype?)null);
        SetupCoPayment();

        var request = new OpretKonsultationRequest(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().Udfør(request));
    }

    [Fact]
    public async Task GivenPatientOverlap_WhenExecutingUseCase_ThenThrowsDomainException()
    {
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var behandlerId1 = Guid.NewGuid();
        var behandlerId2 = Guid.NewGuid();

        var fra = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        var til = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var behandlingstype = NewTreatmentType();
        var patient = NewPatient();
        var egenbetalingsBeregner = Mock.Of<IEgenBetalingsBeregner>(r =>
            r.BeregnEgenbetalingsBeløb(It.IsAny<Domain.ValueObjects.Tidsinterval>(), It.IsAny<Behandlingstype>(), It.IsAny<Patient>()) == new EgenbetalingsBeløb(0));

        var eksisterende = Konsultation.Opret(
            TimeRange(fra, til),
            behandlingstype, patient, behandlerId1,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(), egenbetalingsBeregner);

        _mockBehandTypeRepo.Setup(r => r.HentAsync(typeId))
            .ReturnsAsync(behandlingstype);
        _mockPatientRepo.Setup(r => r.HentAsync(patientId))
            .ReturnsAsync(patient);
        _mockBehandlerRepo.Setup(r => r.HentAsync(behandlerId2))
            .ReturnsAsync(new Behandler("Dr. Lars", "Ortopædi"));
        _mockKonsRepo.Setup(r => r.HentForPatientAsync(patientId))
            .ReturnsAsync(new List<Konsultation> { eksisterende });
        _mockKonsRepo.Setup(r => r.HentForBehandlerAsync(behandlerId2))
            .ReturnsAsync(new List<Konsultation>());
        SetupCoPayment();

        var request = new OpretKonsultationRequest(
            fra.AddMinutes(30), til.AddMinutes(30),
            typeId, patientId, behandlerId2);

        await Assert.ThrowsAsync<DomainException>(() => CreateSut().Udfør(request));
        _mockKonsRepo.Verify(r => r.TilføjAsync(It.IsAny<Konsultation>()), Times.Never);
    }
}

public class AfslutKonsultationUseCaseTests
{
    [Fact]
    public async Task GivenValidRequest_WhenExecutingUseCase_ThenCompletesConsultation()
    {
        var konsultation = Konsultation.Opret(
            new Domain.ValueObjects.Tidsinterval(
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)),
            new Behandlingstype("Opfølgning", new EgenbetalingsBeløb(100)),
            new Patient("Jens", "010190-1234"),
            Guid.NewGuid(),
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>(),
            Mock.Of<IEgenBetalingsBeregner>(r =>
                r.BeregnEgenbetalingsBeløb(It.IsAny<Domain.ValueObjects.Tidsinterval>(), It.IsAny<Behandlingstype>(), It.IsAny<Patient>()) == new EgenbetalingsBeløb(0)));

        var mockRepo = new Mock<IKonsultationRepository>();
        mockRepo.Setup(r => r.HentAsync(konsultation.Id)).ReturnsAsync(konsultation);

        var useCase = new AfslutKonsultationUseCase(mockRepo.Object);
        await useCase.Udfør(new AfslutKonsultationRequest(konsultation.Id, "Alt OK"));

        Assert.Equal(Domain.Enums.KonsultationStatus.Afsluttet, konsultation.Status);
        mockRepo.Verify(r => r.GemAsync(), Times.Once);
    }
}
