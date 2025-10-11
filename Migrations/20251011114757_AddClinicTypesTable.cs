using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicTypesTable : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTypes_Name",
                table: "ClinicTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicTypes");
        }
    }
}
