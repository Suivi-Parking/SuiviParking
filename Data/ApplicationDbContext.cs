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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Les entreprises "supprimees" (soft-delete) n'apparaissent plus jamais
        // dans aucune requete, sans avoir a le repeter partout dans le controller
        modelBuilder.Entity<Entreprise>().HasQueryFilter(e => !e.EstSupprime);

        // La colonne "statut" en base est un VARCHAR (ex: 'Actif'), pas un entier.
        // Sans cette ligne, EF Core essaie par defaut de lire l'enum comme un int
        // et plante avec "Reading as System.Int32 is not supported...".
        modelBuilder.Entity<Abonnement>()
            .Property(a => a.Statut)
            .HasConversion<string>();
    }
}