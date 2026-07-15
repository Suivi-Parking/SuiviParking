using Microsoft.EntityFrameworkCore;
using Parkini.Entities;
using SuiviParking.API.Entities;

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
    public DbSet<HistoriqueParking> HistoriqueParkings => Set<HistoriqueParking>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Employe> Employes => Set<Employe>();
    public DbSet<Vehicule> Vehicules => Set<Vehicule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Soft-delete (archivage) =====
        modelBuilder.Entity<Entreprise>().HasQueryFilter(e => !e.EstSupprime);
        modelBuilder.Entity<Parking>().HasQueryFilter(p => !p.EstSupprime);

        // Si une entreprise est archivee, ses abonnements ne doivent plus apparaitre non plus
        modelBuilder.Entity<Abonnement>().HasQueryFilter(a => !a.Entreprise!.EstSupprime);

        // ===== Conversions enum -> string (colonnes VARCHAR en base) =====
        modelBuilder.Entity<Abonnement>()
            .Property(a => a.Statut)
            .HasConversion<string>();

        modelBuilder.Entity<Parking>()
            .Property(p => p.Statut)
            .HasConversion<string>();

        // ===== Utilisateurs / Roles (travail du collegue) =====
        modelBuilder.Entity<Utilisateur>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Utilisateur>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Utilisateurs)
            .HasForeignKey(u => u.RoleId);

        // ===== Employes / Vehicules (travail du collegue) =====
        modelBuilder.Entity<Employe>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Vehicule>()
            .HasIndex(v => v.Immatriculation)
            .IsUnique();

        modelBuilder.Entity<Employe>()
            .HasMany(e => e.Vehicules)
            .WithOne(v => v.Employe)
            .HasForeignKey(v => v.EmployeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
