using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Konsultationer;
using MinKlinik.UseCases.Notifikation;
using Moq;
using Xunit;

namespace MinKlinik.UseCases.Tests;

public class OpretKonsultationUseCaseTests
{
    private readonly Mock<IBehandlerRepository> _mockBehandlerRepo = new();
    private readonly Mock<IBehandlingstypeRepository> _mockBehandTypeRepo = new();
    private readonly Mock<IKonsultationRepository> _mockKonsRepo = new();
    private readonly Mock<IKonsultationsNotifier> _mockNotifier = new();
    private readonly Mock<IPatientRepository> _mockPatientRepo = new();

    private OpretKonsultationUseCase CreateSut()
    {
        return new OpretKonsultationUseCase(
            _mockKonsRepo.Object,
            _mockBehandTypeRepo.Object,
            _mockPatientRepo.Object,
            _mockBehandlerRepo.Object,
            _mockNotifier.Object);
    }

    private static Behandlingstype NewTreatmentType(double beløb = 300)
    {
        return new Behandlingstype("Undersøgelse");
    }

    private static Patient NewPatient()
    {
        return new Patient("Jens", "010190-1234");
    }

    private static Tidsinterval TimeRange(DateTime fra, DateTime til)
    {
        return new Tidsinterval(fra, til);
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

        var eksisterende = Konsultation.Opret(
            TimeRange(fra, til),
            behandlingstype.Id, patient.Id, behandlerId1,
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());

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
            new Tidsinterval(
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)),
            new Behandlingstype("Opfølgning").Id,
            new Patient("Jens", "010190-1234").Id,
            Guid.NewGuid(),
            Array.Empty<Konsultation>(), Array.Empty<Konsultation>());
            

        var mockRepo = new Mock<IKonsultationRepository>();
        mockRepo.Setup(r => r.HentAsync(konsultation.Id)).ReturnsAsync(konsultation);

        var useCase = new AfslutKonsultationUseCase(mockRepo.Object);
        await useCase.Udfør(new AfslutKonsultationRequest(konsultation.Id, "Alt OK"));

        Assert.Equal(KonsultationStatus.Afsluttet, konsultation.Status);
        mockRepo.Verify(r => r.GemAsync(), Times.Once);
    }
}