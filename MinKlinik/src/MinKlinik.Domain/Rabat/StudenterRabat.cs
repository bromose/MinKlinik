using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public class StudenterRabat : IRabatStrategi
{
    private readonly int _maxUngdomsAlder = 18;
    private readonly double _rabatProcent = 10;

    public string Navn => "Ungdoms-rabat";

    public BeregnetRabat Beregn(Tidsinterval tidspunkt, Behandlingstype behandlingstype, Patient patient)
    {
        if (!behandlingstype.ErBetalingsYdelse) return new BeregnetRabat(0);

        var alder = new CprNummer(patient.CprNummer).Alder;

        if (alder > _maxUngdomsAlder) return new BeregnetRabat(0);

        var rabat = behandlingstype.EgenbetalingsBeløb.Beløb * _rabatProcent / 100;
        return new BeregnetRabat(rabat);
    }
}