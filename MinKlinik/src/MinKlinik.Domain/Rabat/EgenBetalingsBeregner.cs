using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public class EgenBetalingsBeregner : IEgenBetalingsBeregner
{
    private readonly IRabatStrategi[] _rabatStrategier;

    public EgenBetalingsBeregner(IRabatStrategi[] rabatStrategier)
    {
        _rabatStrategier = rabatStrategier;
    }

    EgenbetalingsBeløb IEgenBetalingsBeregner.BeregnEgenbetalingsBeløb(Tidsinterval tidspunkt,
        Behandlingstype behandlingstype, Patient patient)
    {
        if (!behandlingstype.ErBetalingsYdelse)
            return new EgenbetalingsBeløb(0);

        var rabatter = _rabatStrategier.Select(a => a.Beregn(tidspunkt, behandlingstype, patient));
        var bedsteRabat = rabatter.MaxBy(a => a.Beløb) ?? new BeregnetRabat { Beløb = 0 };

        var resultatBeløb = behandlingstype.EgenbetalingsBeløb.Beløb - bedsteRabat.Beløb;

        // Ensure we don't return a negative egenbetaling
        resultatBeløb = Math.Max(0, resultatBeløb);

        return new EgenbetalingsBeløb(resultatBeløb);
    }
}