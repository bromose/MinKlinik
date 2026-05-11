using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Notifikation;

namespace MinKlinik.UseCases.Konsultationer;

internal sealed class OpretKonsultationUseCase : IOpretKonsultationUseCase
{
    private readonly IKonsultationRepository _konsultationRepo;
    private readonly IBehandlingstypeRepository _behandlingstypeRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IBehandlerRepository _behandlerRepo;
    private readonly IKonsultationsNotifier _notifier;

    public OpretKonsultationUseCase(
        IKonsultationRepository konsultationRepo,
        IBehandlingstypeRepository behandlingstypeRepo,
        IPatientRepository patientRepo,
        IBehandlerRepository behandlerRepo,
        IKonsultationsNotifier notifier)
    {
        _konsultationRepo = konsultationRepo;
        _behandlingstypeRepo = behandlingstypeRepo;
        _patientRepo = patientRepo;
        _behandlerRepo = behandlerRepo;
        _notifier = notifier;
    }

    public async Task Udfør(OpretKonsultationRequest request)
    {
        // 1. Materialiser: verificér at de refererede aggregater eksisterer
        var behandlingstype = await _behandlingstypeRepo.HentAsync(request.BehandlingstypeId)
                               ?? throw new NotFoundException("Behandlingstype ikke fundet.");
        var patient = await _patientRepo.HentAsync(request.PatientId)
                      ?? throw new NotFoundException("Patient ikke fundet.");
        _ = await _behandlerRepo.HentAsync(request.BehandlerId)
            ?? throw new NotFoundException("Behandler ikke fundet.");

        var patientBookinger = await _konsultationRepo.HentForPatientAsync(request.PatientId);
        var behandlerBookinger = await _konsultationRepo.HentForBehandlerAsync(request.BehandlerId);

        // 2. Forretningslogik via factory-metode på Aggregate Root
        //    Konsultation modtager Guid'er — IKKE objektreferencer.
        var tidspunkt = new Tidsinterval(request.Fra, request.Til);
        var konsultation = Konsultation.Opret(
            tidspunkt,
            behandlingstype.Id,
            patient.Id,
            request.BehandlerId,
            patientBookinger,
            behandlerBookinger
            );

        // 3. Persistér
        await _konsultationRepo.TilføjAsync(konsultation);
        await _konsultationRepo.GemAsync();

        // 4. Notifikér på alle registrerede kanaler (kap. 2 §2.3.3 OCP — nye
        //    kanaler tilføjes uden at ændre denne kode).
        await _notifier.NotifierOpretAsync(konsultation);
    }
}
