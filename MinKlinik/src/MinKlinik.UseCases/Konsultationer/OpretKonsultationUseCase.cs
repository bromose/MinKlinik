using MinKlinik.Domain.Entities;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.Rabat;
using MinKlinik.Domain.ValueObjects;
using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.UseCases;

namespace MinKlinik.UseCases.Konsultationer;

public class OpretKonsultationUseCase : IOpretKonsultationUseCase
{
    private readonly IKonsultationRepository _konsultationRepo;
    private readonly IBehandlingstypeRepository _behandlingstypeRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IBehandlerRepository _behandlerRepo;
    private readonly IEgenBetalingsBeregner _egenBetalingsBeregner;

    public OpretKonsultationUseCase(
        IKonsultationRepository konsultationRepo,
        IBehandlingstypeRepository behandlingstypeRepo,
        IPatientRepository patientRepo,
        IBehandlerRepository behandlerRepo,
        IEgenBetalingsBeregner egenBetalingsBeregner)
    {
        _konsultationRepo = konsultationRepo;
        _behandlingstypeRepo = behandlingstypeRepo;
        _patientRepo = patientRepo;
        _behandlerRepo = behandlerRepo;
        _egenBetalingsBeregner = egenBetalingsBeregner;
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
            behandlingstype,
            patient,
            request.BehandlerId,
            patientBookinger, 
            behandlerBookinger, 
            _egenBetalingsBeregner
            );

        // 3. Persistér
        await _konsultationRepo.TilføjAsync(konsultation);
        await _konsultationRepo.GemAsync();
    }
}
