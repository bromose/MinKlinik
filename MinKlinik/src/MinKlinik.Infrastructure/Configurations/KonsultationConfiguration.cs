using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinKlinik.Domain.Entities;

namespace MinKlinik.Infrastructure.Configurations;

public class KonsultationConfiguration : IEntityTypeConfiguration<Konsultation>
{
    public void Configure(EntityTypeBuilder<Konsultation> b)
    {
        b.HasKey(k => k.Id);
        b.ComplexProperty(k => k.Tidspunkt, t => t.ToJson());
        b.ComplexProperty(c => c.EgenbetalingsBeløb, t => t.ToJson());
        b.Property(k => k.Status).HasConversion<string>();
    }
}