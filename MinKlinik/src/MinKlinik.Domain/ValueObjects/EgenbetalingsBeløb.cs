namespace MinKlinik.Domain.ValueObjects;

public record EgenbetalingsBeløb
{
    public double Beløb { get; init; }

    // Parameterløs constructor til EF Core
    private EgenbetalingsBeløb()
    {

    }

    public EgenbetalingsBeløb(double beløb)
    {
        Beløb = beløb;
    }

}