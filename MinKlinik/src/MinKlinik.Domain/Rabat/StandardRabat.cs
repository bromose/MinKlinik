using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public class StandardRabat : IRabatStrategi
{
    public string Navn => "Standard";

    public BeregnetRabat Beregn(Tidsinterval tidspunkt, Behandlingstype behandlingstype, Patient patient)
    {
        return new BeregnetRabat(0);
    }
}