using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Konsultationer;

public interface IOpretKonsultationUseCase
{
    Task Udfør(OpretKonsultationRequest request);
}
