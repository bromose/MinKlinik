using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public class SeniorRabat : IRabatStrategi
{
    private readonly int _minSeniorAlder = 60;
    private readonly double _rabatProcent = 15;
    public string Navn => "Senior-rabat";

    public BeregnetRabat Beregn(Tidsinterval tidspunkt, Behandlingstype behandlingstype, Patient patient)
    {
        if (!behandlingstype.ErBetalingsYdelse) return new BeregnetRabat(0);

        var alder = new CprNummer(patient.CprNummer).Alder;

        if (alder < _minSeniorAlder) return new BeregnetRabat(0);

        var rabat = behandlingstype.EgenbetalingsBeløb.Beløb * _rabatProcent / 100;
        return new BeregnetRabat(rabat);
    }
}