using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SideReport.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOcrAndDrugEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── OcrImages: 새 컬럼 추가 ─────────────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "RawOcrText",
                table: "OcrImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParsedResultJson",
                table: "OcrImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OcrImages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "idx_ocr_images_user_id",
                table: "OcrImages",
                column: "UserId");

            // ─── KnownSideEffects ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "KnownSideEffects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrugName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SymptomName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FrequencyRank = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "KAERS"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownSideEffects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_known_side_effects_drug_name",
                table: "KnownSideEffects",
                column: "DrugName");

            // ─── DrugCaches ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "DrugCaches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrugName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OfficialName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ingredient = table.Column<string>(type: "text", nullable: true),
                    Efficacy = table.Column<string>(type: "text", nullable: true),
                    UsageInfo = table.Column<string>(type: "text", nullable: true),
                    SideEffects = table.Column<string>(type: "text", nullable: true),
                    ApiSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "mfds"),
                    CachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_drug_caches_drug_name_expires",
                table: "DrugCaches",
                columns: new[] { "DrugName", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DrugCaches");
            migrationBuilder.DropTable(name: "KnownSideEffects");

            migrationBuilder.DropIndex(
                name: "idx_ocr_images_user_id",
                table: "OcrImages");

            migrationBuilder.DropColumn(name: "Status", table: "OcrImages");
            migrationBuilder.DropColumn(name: "ParsedResultJson", table: "OcrImages");
            migrationBuilder.DropColumn(name: "RawOcrText", table: "OcrImages");
        }
    }
}
