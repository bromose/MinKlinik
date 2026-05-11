using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Konsultationer;

public interface IAflysKonsultationUseCase
{
    Task Udfør(AflysKonsultationRequest request);
}
