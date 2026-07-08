using Microsoft.EntityFrameworkCore;
using SuiviParking.API.Models;

namespace SuiviParking.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Employe> Employes { get; set; }
        public DbSet<Vehicule> Vehicules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Email unique pour Employé
            modelBuilder.Entity<Employe>()
                .HasIndex(e => e.Email)
                .IsUnique();

            // Immatriculation unique pour Véhicule
            modelBuilder.Entity<Vehicule>()
                .HasIndex(v => v.Immatriculation)
                .IsUnique();

            // Relation Employé → Véhicules
            modelBuilder.Entity<Employe>()
                .HasMany(e => e.Vehicules)
                .WithOne(v => v.Employe)
                .HasForeignKey(v => v.EmployeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}