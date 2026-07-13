using Microsoft.EntityFrameworkCore;
using Parkini.Entities;

namespace Parkini.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Entreprise> Entreprises => Set<Entreprise>();
    public DbSet<Abonnement> Abonnements => Set<Abonnement>();
    public DbSet<Parking> Parkings => Set<Parking>();
    public DbSet<Zone> Zones => Set<Zone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft-delete global : les entreprises et parkings "supprimes"
        // n'apparaissent plus jamais dans aucune requete.
        modelBuilder.Entity<Entreprise>().HasQueryFilter(e => !e.EstSupprime);
        modelBuilder.Entity<Parking>().HasQueryFilter(p => !p.EstSupprime);

        // La colonne "statut" en base est un VARCHAR (ex: 'Actif'), pas un entier.
        // Sans cette ligne, EF Core essaie par defaut de lire l'enum comme un int
        // et plante avec "Reading as System.Int32 is not supported...".
        modelBuilder.Entity<Abonnement>()
            .Property(a => a.Statut)
            .HasConversion<string>();
    }
}