using MinKlinik.Domain.Exceptions;
using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Konsultationer;

internal sealed class AfslutKonsultationUseCase : IAfslutKonsultationUseCase
{
    private readonly IKonsultationRepository _repo;

    public AfslutKonsultationUseCase(IKonsultationRepository repo)
    {
        _repo = repo;
    }

    public async Task Udfør(AfslutKonsultationRequest request)
    {
        var konsultation = await _repo.HentAsync(request.KonsultationId)
            ?? throw new NotFoundException("Konsultation ikke fundet.");

        konsultation.Afslut(request.Notat);

        await _repo.GemAsync();
    }
}
