using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<ClinicType> ClinicTypes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // Clinic configuration
            modelBuilder.Entity<Clinic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship
                entity.HasOne(c => c.Owner)
                      .WithMany(u => u.Clinics)
                      .HasForeignKey(c => c.OwnerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Doctor configuration
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Rating).HasPrecision(3, 2); // 0.00 - 5.00

                // Configure relationship with Clinic
                entity.HasOne(d => d.Clinic)
                      .WithMany()
                      .HasForeignKey(d => d.ClinicId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ClinicType configuration
            modelBuilder.Entity<ClinicType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed initial clinic types
            var seedDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<ClinicType>().HasData(
                new ClinicType { Id = 1, Name = "Xüsusi Klinika", Description = "Özəl sağlamlıq xidmətləri göstərən klinika", CreatedAt = seedDate },
                new ClinicType { Id = 2, Name = "Dövlət Klinikası", Description = "Dövlət tərəfindən idarə olunan sağlamlıq müəssisəsi", CreatedAt = seedDate },
                new ClinicType { Id = 3, Name = "Poliklinika", Description = "Ümumi sağlamlıq xidmətləri göstərən müəssisə", CreatedAt = seedDate },
                new ClinicType { Id = 4, Name = "Diş Klinikası", Description = "Diş sağlamlığı xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 5, Name = "Gözəllik Mərkəzi", Description = "Estetik və gözəllik xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 6, Name = "Laboratoriya", Description = "Tibbi test və analiz xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 7, Name = "Xəstəxana", Description = "Yataqlı müalicə müəssisəsi", CreatedAt = seedDate },
                new ClinicType { Id = 8, Name = "Digər", Description = "Digər sağlamlıq xidmətləri", CreatedAt = seedDate }
            );
        }
    }
}