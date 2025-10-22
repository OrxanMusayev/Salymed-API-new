using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend;

public partial class SalymedDbContext : DbContext
{
    public SalymedDbContext()
    {
    }

    public SalymedDbContext(DbContextOptions<SalymedDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<ClinicType> ClinicTypes { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<PlanFeature> PlanFeatures { get; set; }

    public virtual DbSet<PlanFeatureMapping> PlanFeatureMappings { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasIndex(e => e.OwnerId, "IX_Clinics_OwnerId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(255);
            entity.Property(e => e.ZipCode).HasMaxLength(20);

            entity.HasOne(d => d.Owner).WithMany(p => p.Clinics)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClinicType>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_ClinicTypes_Name").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasIndex(e => e.ClinicId, "IX_Doctors_ClinicId");

            entity.HasIndex(e => e.Email, "IX_Doctors_Email").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.Specialty).HasMaxLength(100);
            entity.Property(e => e.WorkingHours).HasMaxLength(50);

            entity.HasOne(d => d.Clinic).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlanFeature>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<PlanFeatureMapping>(entity =>
        {
            entity.HasIndex(e => e.FeatureId, "IX_PlanFeatureMappings_FeatureId");

            entity.HasIndex(e => new { e.PlanId, e.FeatureId }, "IX_PlanFeatureMappings_PlanId_FeatureId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Feature).WithMany(p => p.PlanFeatureMappings).HasForeignKey(d => d.FeatureId);

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanFeatureMappings).HasForeignKey(d => d.PlanId);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasIndex(e => e.ClinicId, "IX_Subscriptions_ClinicId");

            entity.HasIndex(e => new { e.ClinicId, e.Status }, "IX_Subscriptions_ClinicId_Status");

            entity.HasIndex(e => e.PlanId, "IX_Subscriptions_PlanId");

            entity.HasIndex(e => e.Status, "IX_Subscriptions_Status");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PaddleSubscriptionId).HasMaxLength(255);
            entity.Property(e => e.PaddleTransactionId).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(255);

            entity.HasOne(d => d.Clinic).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PaddlePriceId).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneCountryCode).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
