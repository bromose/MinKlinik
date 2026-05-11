using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinKlinik.Domain.Entities;

namespace MinKlinik.Infrastructure.Configurations;

public class BehandlingstypeConfiguration : IEntityTypeConfiguration<Behandlingstype>
{
    public void Configure(EntityTypeBuilder<Behandlingstype> b)
    {
        b.HasKey(k => k.Id);
        b.ComplexProperty(c => c.EgenbetalingsBeløb, t => t.ToJson());
    }
}