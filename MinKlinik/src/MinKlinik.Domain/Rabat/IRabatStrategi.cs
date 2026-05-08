using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat
{
    public interface IRabatStrategi
    {
        string Navn { get; }

        BeregnetRabat Beregn(Tidsinterval tidspunkt,
            Behandlingstype behandlingstype,
            Patient patient);
    }

    // Concrete strategies
}