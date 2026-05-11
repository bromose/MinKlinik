using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public interface IEgenBetalingsBeregner
{
    EgenbetalingsBeløb BeregnEgenbetalingsBeløb(Tidsinterval tidspunkt,
        Behandlingstype behandlingstype,
        Patient patient);
}