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
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entreprise>().HasQueryFilter(e => !e.EstSupprime);

        modelBuilder.Entity<Abonnement>()
            .Property(a => a.Statut)
            .HasConversion<string>();

        modelBuilder.Entity<Utilisateur>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Utilisateur>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Utilisateurs)
            .HasForeignKey(u => u.RoleId);
    }
}