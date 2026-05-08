using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Entities;

namespace MinKlinik.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Konsultation> Konsultationer => Set<Konsultation>();
    public DbSet<Behandlingstype> Behandlingstyper => Set<Behandlingstype>();
    public DbSet<Patient> Patienter => Set<Patient>();
    public DbSet<Behandler> Behandlere => Set<Behandler>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Behandlingstype>(b => b.HasKey(bt => bt.Id));
        modelBuilder.Entity<Patient>(b => b.HasKey(p => p.Id));
        modelBuilder.Entity<Behandler>(b => b.HasKey(bh => bh.Id));
    }
}
