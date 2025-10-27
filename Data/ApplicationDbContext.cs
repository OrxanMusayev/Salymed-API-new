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
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<PlanFeatureMapping> PlanFeatureMappings { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        
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
            });

            // ClinicType configuration
            modelBuilder.Entity<ClinicType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // SubscriptionPlan configuration
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            // PlanFeature configuration
            modelBuilder.Entity<PlanFeature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // PlanFeatureMapping configuration
            modelBuilder.Entity<PlanFeatureMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship with SubscriptionPlan
                entity.HasOne(m => m.Plan)
                      .WithMany(p => p.PlanFeatureMappings)
                      .HasForeignKey(m => m.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure relationship with PlanFeature
                entity.HasOne(m => m.Feature)
                      .WithMany(f => f.PlanFeatureMappings)
                      .HasForeignKey(m => m.FeatureId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate mappings
                entity.HasIndex(e => new { e.PlanId, e.FeatureId }).IsUnique();
            });

            // Subscription configuration
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.AmountPaid).HasPrecision(18, 2);

                // Index for better query performance
                entity.HasIndex(e => e.ClinicId);
                entity.HasIndex(e => e.PlanId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.ClinicId, e.Status });
            });

            // PaymentMethod configuration
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for better query performance
                entity.HasIndex(e => e.ClinicId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.ClinicId, e.IsDefault });
                entity.HasIndex(e => new { e.ClinicId, e.IsActive });
            });

            // Invoice configuration
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for better query performance
                entity.HasIndex(e => e.ClinicId);
                entity.HasIndex(e => e.SubscriptionId);
                entity.HasIndex(e => e.PlanId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaddleTransactionId);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.ClinicId, e.Status });
            });

            // Seed initial clinic types
            var seedDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<ClinicType>().HasData(
                new ClinicType { Id = 1, Name = "Xüsusi Klinika", TranslationKey = "clinic_types.private_clinic", Description = "Özəl sağlamlıq xidmətləri göstərən klinika", CreatedAt = seedDate },
                new ClinicType { Id = 2, Name = "Dövlət Klinikası", TranslationKey = "clinic_types.public_clinic", Description = "Dövlət tərəfindən idarə olunan sağlamlıq müəssisəsi", CreatedAt = seedDate },
                new ClinicType { Id = 3, Name = "Poliklinika", TranslationKey = "clinic_types.polyclinic", Description = "Ümumi sağlamlıq xidmətləri göstərən müəssisə", CreatedAt = seedDate },
                new ClinicType { Id = 4, Name = "Diş Klinikası", TranslationKey = "clinic_types.dental_clinic", Description = "Diş sağlamlığı xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 5, Name = "Gözəllik Mərkəzi", TranslationKey = "clinic_types.beauty_center", Description = "Estetik və gözəllik xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 6, Name = "Laboratoriya", TranslationKey = "clinic_types.laboratory", Description = "Tibbi test və analiz xidmətləri", CreatedAt = seedDate },
                new ClinicType { Id = 7, Name = "Xəstəxana", TranslationKey = "clinic_types.hospital", Description = "Yataqlı müalicə müəssisəsi", CreatedAt = seedDate },
                new ClinicType { Id = 8, Name = "Digər", TranslationKey = "clinic_types.other", Description = "Digər sağlamlıq xidmətləri", CreatedAt = seedDate }
            );

            // Seed subscription plans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = 1, Name = "Başlanğıc", Description = "Tək həkimli kliniklər üçün", Price = 45.00m, Currency = "USD", Period = 2, IsActive = true, IsFeatured = false, DisplayOrder = 1, CreatedAt = seedDate },
                new SubscriptionPlan { Id = 2, Name = "Professional", Description = "3 həkimə qədər olan kliniklər üçün", Price = 75.00m, Currency = "USD", Period = 2, IsActive = true, IsFeatured = true, DisplayOrder = 2, CreatedAt = seedDate },
                new SubscriptionPlan { Id = 3, Name = "Premium", Description = "10 həkimə qədər olan kliniklər üçün", Price = 125.00m, Currency = "USD", Period = 2, IsActive = true, IsFeatured = false, DisplayOrder = 3, CreatedAt = seedDate }
            );

            // Seed plan features
            modelBuilder.Entity<PlanFeature>().HasData(
                new PlanFeature { Id = 1, Name = "İlk 1 Ay Pulsuz", Description = "İlk ay üçün pulsuz istifadə", IsPremium = false, IsActive = true, DisplayOrder = 1, CreatedAt = seedDate },
                new PlanFeature { Id = 2, Name = "Görüş Ajanda İstifadəsi", Description = "Görüşlərin idarə edilməsi", IsPremium = false, IsActive = true, DisplayOrder = 2, CreatedAt = seedDate },
                new PlanFeature { Id = 3, Name = "Süni İntellekt Avtomatik Görüş Xatırlatması", Description = "AI ilə avtomatik xatırlatma", IsPremium = false, IsActive = true, DisplayOrder = 3, CreatedAt = seedDate },
                new PlanFeature { Id = 4, Name = "Whatsapp Süni İntellekt Ağıllı Rəqəmsal Köməkçisi", Description = "WhatsApp AI köməkçisi", IsPremium = false, IsActive = true, DisplayOrder = 4, CreatedAt = seedDate },
                new PlanFeature { Id = 5, Name = "Google Təqvim Əlaqəsi", Description = "Google Calendar inteqrasiyası", IsPremium = false, IsActive = true, DisplayOrder = 5, CreatedAt = seedDate },
                new PlanFeature { Id = 6, Name = "Ətraflı Görüş Təhlil Modulu", Description = "Detallı təhlil və hesabatlar", IsPremium = false, IsActive = true, DisplayOrder = 6, CreatedAt = seedDate },
                new PlanFeature { Id = 7, Name = "Gündəlik həftəlik və aylıq kliniklərdəki fəaliyyət hesabatı", Description = "Müntəzəm hesabatlar", IsPremium = false, IsActive = true, DisplayOrder = 7, CreatedAt = seedDate },
                new PlanFeature { Id = 8, Name = "Ağıllı Süni İntellekt Köməkçisi ilə Avtomatik Ad Günü, Xüsusi Gün, Kampaniya və Xatırlatma Mesajları", Description = "AI avtomatik mesajlaşma", IsPremium = false, IsActive = true, DisplayOrder = 8, CreatedAt = seedDate },
                new PlanFeature { Id = 9, Name = "Sınırsız Ağıllı Kampaniya Xüsusiyyəti", Description = "Limitsiz kampaniya yaratma", IsPremium = false, IsActive = true, DisplayOrder = 9, CreatedAt = seedDate },
                new PlanFeature { Id = 10, Name = "Görüşləri süni intellekt ilə telefon zəngi edərək təsdiqləmə", Description = "AI telefon təsdiqi", IsPremium = false, IsActive = true, DisplayOrder = 10, CreatedAt = seedDate },
                new PlanFeature { Id = 11, Name = "AI Avtomatik Nömrə Axtarışı və Hesabat Sistemi", Description = "Avtomatik nömrə axtarışı", IsPremium = false, IsActive = true, DisplayOrder = 11, CreatedAt = seedDate },
                new PlanFeature { Id = 12, Name = "1 Mütəxəssis Həkim Qeydiyyatı", Description = "1 həkim hesabı", IsPremium = false, IsActive = true, DisplayOrder = 12, CreatedAt = seedDate },
                new PlanFeature { Id = 13, Name = "3 Mütəxəssis Həkim Qeydiyyatı", Description = "3 həkim hesabı", IsPremium = false, IsActive = true, DisplayOrder = 13, CreatedAt = seedDate },
                new PlanFeature { Id = 14, Name = "10 Mütəxəssis Həkim Qeydiyyatı", Description = "10 həkim hesabı", IsPremium = true, IsActive = true, DisplayOrder = 14, CreatedAt = seedDate }
            );

            // Seed plan feature mappings
            // Başlanğıc plan features (1-12)
            for (int i = 1; i <= 12; i++)
            {
                modelBuilder.Entity<PlanFeatureMapping>().HasData(
                    new PlanFeatureMapping { Id = i, PlanId = 1, FeatureId = i, CreatedAt = seedDate }
                );
            }

            // Professional plan features (1-11, 13)
            int mappingId = 13;
            for (int i = 1; i <= 11; i++)
            {
                modelBuilder.Entity<PlanFeatureMapping>().HasData(
                    new PlanFeatureMapping { Id = mappingId++, PlanId = 2, FeatureId = i, CreatedAt = seedDate }
                );
            }
            modelBuilder.Entity<PlanFeatureMapping>().HasData(
                new PlanFeatureMapping { Id = mappingId++, PlanId = 2, FeatureId = 13, CreatedAt = seedDate }
            );

            // Premium plan features (1-11, 14)
            for (int i = 1; i <= 11; i++)
            {
                modelBuilder.Entity<PlanFeatureMapping>().HasData(
                    new PlanFeatureMapping { Id = mappingId++, PlanId = 3, FeatureId = i, CreatedAt = seedDate }
                );
            }
            modelBuilder.Entity<PlanFeatureMapping>().HasData(
                new PlanFeatureMapping { Id = mappingId, PlanId = 3, FeatureId = 14, CreatedAt = seedDate }
            );
        }
    }
}