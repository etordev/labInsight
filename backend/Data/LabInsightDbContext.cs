using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabInsight.Api.Data;

public class LabInsightDbContext(DbContextOptions<LabInsightDbContext> options) : DbContext(options)
{
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<AnalysisCategory> AnalysisCategories => Set<AnalysisCategory>();
    public DbSet<LabAnalysis> LabAnalyses => Set<LabAnalysis>();
    public DbSet<GraphTypeEntity> GraphTypes => Set<GraphTypeEntity>();
    public DbSet<GraphDataTypeEntity> GraphDataTypes => Set<GraphDataTypeEntity>();
    public DbSet<GraphItemEntity> GraphItems => Set<GraphItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Laboratory>(entity =>
        {
            entity.ToTable("laboratories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            ConfigureAudit(entity);
        });

        modelBuilder.Entity<AnalysisCategory>(entity =>
        {
            entity.ToTable("analysis_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ExpectedProcessingHours).IsRequired().HasPrecision(8, 2);
            ConfigureAudit(entity);
        });

        modelBuilder.Entity<LabAnalysis>(entity =>
        {
            entity.ToTable("lab_analyses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnalysisNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.AnalysisNumber).IsUnique();
            entity.Property(e => e.ReceivedAt).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(32);

            entity.HasOne(e => e.Laboratory)
                .WithMany(l => l.Analyses)
                .HasForeignKey(e => e.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.AnalysisCategory)
                .WithMany(c => c.Analyses)
                .HasForeignKey(e => e.AnalysisCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasIndex(e => e.LaboratoryId);
            entity.HasIndex(e => e.AnalysisCategoryId);
            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.Status);
            ConfigureAudit(entity);
        });

        modelBuilder.Entity<GraphTypeEntity>(entity =>
        {
            entity.ToTable("graph_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TechnicalName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TechnicalName).IsUnique();
            ConfigureAudit(entity);
        });

        modelBuilder.Entity<GraphDataTypeEntity>(entity =>
        {
            entity.ToTable("graph_data_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TechnicalName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TechnicalName).IsUnique();
            ConfigureAudit(entity);
        });

        modelBuilder.Entity<GraphItemEntity>(entity =>
        {
            entity.ToTable("graph_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Content);

            entity.HasOne(e => e.GraphType)
                .WithMany(t => t.GraphItems)
                .HasForeignKey(e => e.GraphTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.GraphDataType)
                .WithMany(d => d.GraphItems)
                .HasForeignKey(e => e.GraphDataTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            ConfigureAudit(entity);
        });
    }

    private static void ConfigureAudit<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : EntityBase
    {
        entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).IsRequired();
        entity.HasIndex(e => e.IsDeleted);
    }
}
