using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SideReport.Domain.Entities;
using SideReport.Domain.Enums;
using SideReport.Infrastructure.Identity;

namespace SideReport.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext — Identity 기반 확장
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<AdverseReport> AdverseReports => Set<AdverseReport>();
    public DbSet<ReportSymptom> ReportSymptoms => Set<ReportSymptom>();
    public DbSet<ReportMedication> ReportMedications => Set<ReportMedication>();
    public DbSet<OcrImage> OcrImages => Set<OcrImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ─── ApplicationUser ─────────────────────────────────────────────────
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(u => u.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("NOW()");
        });

        // ─── RefreshToken ─────────────────────────────────────────────────────
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token)
                  .IsRequired()
                  .HasMaxLength(512);

            entity.HasIndex(rt => rt.Token)
                  .IsUnique()
                  .HasDatabaseName("idx_refresh_tokens_token");

            entity.HasIndex(rt => rt.UserId)
                  .HasDatabaseName("idx_refresh_tokens_user_id");

            entity.Property(rt => rt.IsRevoked)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(rt => rt.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("NOW()");

            entity.HasOne<ApplicationUser>()
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Medication ───────────────────────────────────────────────────────
        builder.Entity<Medication>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(m => m.DrugName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(m => m.Dosage)
                  .HasMaxLength(100);

            entity.Property(m => m.Frequency)
                  .HasMaxLength(100);

            entity.Property(m => m.StartDate)
                  .IsRequired();

            entity.Property(m => m.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(m => m.UserId)
                  .HasDatabaseName("idx_medications_user_id");

            entity.HasOne<ApplicationUser>()
                  .WithMany(u => u.Medications)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── AdverseReport ────────────────────────────────────────────────────
        builder.Entity<AdverseReport>(entity =>
        {
            entity.HasKey(ar => ar.Id);

            entity.Property(ar => ar.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(ar => ar.Severity)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(ar => ar.ReportedAt)
                  .IsRequired()
                  .HasDefaultValueSql("NOW()");

            entity.HasIndex(ar => ar.UserId)
                  .HasDatabaseName("idx_adverse_reports_user_id");

            entity.HasIndex(ar => ar.ReportedAt)
                  .IsDescending()
                  .HasDatabaseName("idx_adverse_reports_reported_at");

            entity.HasOne<ApplicationUser>()
                  .WithMany(u => u.AdverseReports)
                  .HasForeignKey(ar => ar.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── ReportSymptom ────────────────────────────────────────────────────
        builder.Entity<ReportSymptom>(entity =>
        {
            entity.HasKey(rs => rs.Id);

            entity.Property(rs => rs.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(rs => rs.SymptomName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(rs => rs.IsOfficial)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.HasOne(rs => rs.Report)
                  .WithMany(ar => ar.ReportSymptoms)
                  .HasForeignKey(rs => rs.ReportId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── ReportMedication ─────────────────────────────────────────────────
        builder.Entity<ReportMedication>(entity =>
        {
            entity.HasKey(rm => rm.Id);

            entity.Property(rm => rm.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(rm => rm.Report)
                  .WithMany(ar => ar.ReportMedications)
                  .HasForeignKey(rm => rm.ReportId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.Medication)
                  .WithMany(m => m.ReportMedications)
                  .HasForeignKey(rm => rm.MedicationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── OcrImage ─────────────────────────────────────────────────────────
        builder.Entity<OcrImage>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(oi => oi.StorageUrl)
                  .IsRequired();

            entity.Property(oi => oi.DeleteAfterProcessing)
                  .IsRequired()
                  .HasDefaultValue(true);

            entity.Property(oi => oi.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("NOW()");

            entity.HasOne<ApplicationUser>()
                  .WithMany(u => u.OcrImages)
                  .HasForeignKey(oi => oi.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
