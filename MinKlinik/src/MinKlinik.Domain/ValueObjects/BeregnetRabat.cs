namespace MinKlinik.Domain.ValueObjects;

public record BeregnetRabat
{
    public double Beløb { get; init; }

    // Parameterløs constructor til EF Core
    public BeregnetRabat()
    {
    }

    public BeregnetRabat(double beløb)
    {
        Beløb = beløb;
    }
}