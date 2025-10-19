using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaddlePriceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Period = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneCountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatureMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatureMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanFeatureMappings_PlanFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "PlanFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanFeatureMappings_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneCountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneCountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YearsExperience = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WorkingHours = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Rating = table.Column<double>(type: "float(3)", precision: 3, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaddleTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaddleSubscriptionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsTrialPeriod = table.Column<bool>(type: "bit", nullable: false),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InvoiceDetails = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClinicTypes",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Özəl sağlamlıq xidmətləri göstərən klinika", true, "Xüsusi Klinika", null },
                    { 2, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Dövlət tərəfindən idarə olunan sağlamlıq müəssisəsi", true, "Dövlət Klinikası", null },
                    { 3, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Ümumi sağlamlıq xidmətləri göstərən müəssisə", true, "Poliklinika", null },
                    { 4, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Diş sağlamlığı xidmətləri", true, "Diş Klinikası", null },
                    { 5, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Estetik və gözəllik xidmətləri", true, "Gözəllik Mərkəzi", null },
                    { 6, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Tibbi test və analiz xidmətləri", true, "Laboratoriya", null },
                    { 7, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Yataqlı müalicə müəssisəsi", true, "Xəstəxana", null },
                    { 8, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Digər sağlamlıq xidmətləri", true, "Digər", null }
                });

            migrationBuilder.InsertData(
                table: "PlanFeatures",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsActive", "IsPremium", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "İlk ay üçün pulsuz istifadə", 1, true, false, "İlk 1 Ay Pulsuz", null },
                    { 2, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Görüşlərin idarə edilməsi", 2, true, false, "Görüş Ajanda İstifadəsi", null },
                    { 3, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "AI ilə avtomatik xatırlatma", 3, true, false, "Süni İntellekt Avtomatik Görüş Xatırlatması", null },
                    { 4, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "WhatsApp AI köməkçisi", 4, true, false, "Whatsapp Süni İntellekt Ağıllı Rəqəmsal Köməkçisi", null },
                    { 5, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Google Calendar inteqrasiyası", 5, true, false, "Google Təqvim Əlaqəsi", null },
                    { 6, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Detallı təhlil və hesabatlar", 6, true, false, "Ətraflı Görüş Təhlil Modulu", null },
                    { 7, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Müntəzəm hesabatlar", 7, true, false, "Gündəlik həftəlik və aylıq kliniklərdəki fəaliyyət hesabatı", null },
                    { 8, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "AI avtomatik mesajlaşma", 8, true, false, "Ağıllı Süni İntellekt Köməkçisi ilə Avtomatik Ad Günü, Xüsusi Gün, Kampaniya və Xatırlatma Mesajları", null },
                    { 9, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Limitsiz kampaniya yaratma", 9, true, false, "Sınırsız Ağıllı Kampaniya Xüsusiyyəti", null },
                    { 10, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "AI telefon təsdiqi", 10, true, false, "Görüşləri süni intellekt ilə telefon zəngi edərək təsdiqləmə", null },
                    { 11, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Avtomatik nömrə axtarışı", 11, true, false, "AI Avtomatik Nömrə Axtarışı və Hesabat Sistemi", null },
                    { 12, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "1 həkim hesabı", 12, true, false, "1 Mütəxəssis Həkim Qeydiyyatı", null },
                    { 13, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "3 həkim hesabı", 13, true, false, "3 Mütəxəssis Həkim Qeydiyyatı", null },
                    { 14, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "10 həkim hesabı", 14, true, true, "10 Mütəxəssis Həkim Qeydiyyatı", null }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "CreatedAt", "Currency", "Description", "DisplayOrder", "IsActive", "IsFeatured", "Name", "PaddlePriceId", "Period", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "USD", "Tək həkimli kliniklər üçün", 1, true, false, "Başlanğıc", null, 2, 45.00m, null },
                    { 2, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "USD", "3 həkimə qədər olan kliniklər üçün", 2, true, true, "Professional", null, 2, 75.00m, null },
                    { 3, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), "USD", "10 həkimə qədər olan kliniklər üçün", 3, true, false, "Premium", null, 2, 125.00m, null }
                });

            migrationBuilder.InsertData(
                table: "PlanFeatureMappings",
                columns: new[] { "Id", "CreatedAt", "FeatureId", "PlanId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 },
                    { 2, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1 },
                    { 3, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1 },
                    { 4, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1 },
                    { 5, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 5, 1 },
                    { 6, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 6, 1 },
                    { 7, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 7, 1 },
                    { 8, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 8, 1 },
                    { 9, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 9, 1 },
                    { 10, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 10, 1 },
                    { 11, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 11, 1 },
                    { 12, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 12, 1 },
                    { 13, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 },
                    { 14, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2 },
                    { 15, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 3, 2 },
                    { 16, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 17, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 5, 2 },
                    { 18, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 6, 2 },
                    { 19, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 7, 2 },
                    { 20, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 8, 2 },
                    { 21, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 9, 2 },
                    { 22, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 10, 2 },
                    { 23, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 11, 2 },
                    { 24, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 13, 2 },
                    { 25, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 26, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, 3 },
                    { 27, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3 },
                    { 28, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 4, 3 },
                    { 29, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 5, 3 },
                    { 30, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 6, 3 },
                    { 31, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 7, 3 },
                    { 32, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 8, 3 },
                    { 33, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 9, 3 },
                    { 34, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 10, 3 },
                    { 35, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 11, 3 },
                    { 36, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), 14, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_OwnerId",
                table: "Clinics",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTypes_Name",
                table: "ClinicTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ClinicId",
                table: "Doctors",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_Email",
                table: "Doctors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatureMappings_FeatureId",
                table: "PlanFeatureMappings",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatureMappings_PlanId_FeatureId",
                table: "PlanFeatureMappings",
                columns: new[] { "PlanId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClinicId",
                table: "Subscriptions",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClinicId_Status",
                table: "Subscriptions",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicTypes");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "PlanFeatureMappings");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
