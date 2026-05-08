using MinKlinik.Domain.Entities;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Rabat;

public class BlackFridayRabat : IRabatStrategi
{
    private readonly double _rabatProcent = 20;

    public string Navn => "BlackFriday";

    public BeregnetRabat Beregn(Tidsinterval tidspunkt, Behandlingstype behandlingstype, Patient patient)
    {
        if (!behandlingstype.ErBetalingsYdelse) return new BeregnetRabat(0);

        if(!ErBlackFridayDag(tidspunkt.Fra)) return new BeregnetRabat(0);

        var rabat = behandlingstype.EgenbetalingsBeløb.Beløb * _rabatProcent / 100;

        return new BeregnetRabat(rabat);
    }

    private bool ErBlackFridayDag(DateTime tidspunkt)
    {
        var iDag = tidspunkt.Date;
        return iDag.Month == 11 && iDag.Day == BlackFridayDag(iDag.Year);
    }

    private static int BlackFridayDag(int år)
    {
        // Start ved 1. november
        var november1 = new DateTime(år, 11, 1);

        // Find ud af hvor mange dage der er til den første fredag
        // DayOfWeek.Friday er 5. 
        // Vi bruger (7 + mål - start) % 7 formlen for at finde afstanden
        var dageTilFørsteFredag = ((int)DayOfWeek.Friday - (int)november1.DayOfWeek + 7) % 7;

        // Den første fredag i måneden
        var førsteFredag = 1 + dageTilFørsteFredag;

        // Black Friday er den fjerde fredag (første fredag + 3 uger)
        return førsteFredag + 21;
    }
}