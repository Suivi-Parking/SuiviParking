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
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<Abonnement> Abonnements => Set<Abonnement>();
    public DbSet<Parking> Parkings => Set<Parking>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<HistoriqueParking> HistoriqueParkings => Set<HistoriqueParking>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Employe> Employes => Set<Employe>();
    public DbSet<Vehicule> Vehicules => Set<Vehicule>();

    public DbSet<Acces> Acces => Set<Acces>();
    public DbSet<Notification> Notifications => Set<Notification>();

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


        // ===== Acces =====
        // Relation Accès → Véhicule
        modelBuilder.Entity<Acces>()
            .HasOne(a => a.Vehicule)
            .WithMany()
            .HasForeignKey(a => a.VehiculeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation Accès → Parking
        modelBuilder.Entity<Acces>()
            .HasOne(a => a.Parking)
            .WithMany()
            .HasForeignKey(a => a.ParkingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation Accès → Abonnement
        modelBuilder.Entity<Acces>()

        .HasOne(a => a.Abonnement)
        .WithMany()
        .HasForeignKey(a => a.AbonnementId)
        .OnDelete(DeleteBehavior.SetNull);


        // ===== Notification =====
        modelBuilder.Entity<Notification>().Property(n => n.Type).HasConversion<string>();
        modelBuilder.Entity<Notification>().Property(n => n.Canal).HasConversion<string>();
        modelBuilder.Entity<Notification>().Property(n => n.Statut).HasConversion<string>();

        // Relation Notification → Paiement (rappel de paiement)
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Paiement)
            .WithMany()
            .HasForeignKey(n => n.PaiementId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relation Notification → Employe / Vehicule (rappel mise a jour infos)
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Employe)
            .WithMany()
            .HasForeignKey(n => n.EmployeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Vehicule)
            .WithMany()
            .HasForeignKey(n => n.VehiculeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

