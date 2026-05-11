using MinKlinik.UseCases.Dtos;

namespace MinKlinik.UseCases.Statistik;

public interface IBeregnMånedsstatistikUseCase
{
    Task<MånedsstatistikDto> Udfør(int år, int måned);
}