using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Konsultationer;

public interface IAfslutKonsultationUseCase
{
    Task Udfør(AfslutKonsultationRequest request);
}
