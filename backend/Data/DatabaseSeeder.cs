using LabInsight.Api.Catalog;
using LabInsight.Api.Entities;
using LabInsight.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Data;

public class DatabaseSeeder(LabInsightDbContext dbContext, ILogger<DatabaseSeeder> logger)
{
    private const int SyntheticAnalysisCount = 10_000;
    private const int RandomSeed = 42;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedGraphTypesAsync(cancellationToken);
        await SeedGraphDataTypesAsync(cancellationToken);
        await SeedLaboratoriesAsync(cancellationToken);
        await SeedAnalysisCategoriesAsync(cancellationToken);
        await RemoveLegacyDefaultGraphItemsAsync(cancellationToken);
        await SeedLabAnalysesAsync(cancellationToken);
    }

    private async Task SeedGraphTypesAsync(CancellationToken cancellationToken)
    {
        var existing = await dbContext.GraphTypes
            .Select(type => type.TechnicalName)
            .ToListAsync(cancellationToken);
        var existingNames = existing.ToHashSet(StringComparer.Ordinal);

        var missing = GraphMetadata.GraphTypeTechnicalNames
            .Where(name => !existingNames.Contains(name))
            .Select(name => new GraphTypeEntity { TechnicalName = name })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        dbContext.GraphTypes.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Upserted {Count} missing graph types.", missing.Count);
    }

    private async Task SeedGraphDataTypesAsync(CancellationToken cancellationToken)
    {
        var existing = await dbContext.GraphDataTypes
            .Select(type => type.TechnicalName)
            .ToListAsync(cancellationToken);
        var existingNames = existing.ToHashSet(StringComparer.Ordinal);

        var missing = GraphMetadata.GraphDataTypeTechnicalNames
            .Where(name => !existingNames.Contains(name))
            .Select(name => new GraphDataTypeEntity { TechnicalName = name })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        dbContext.GraphDataTypes.AddRange(missing);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Upserted {Count} missing graph data types.", missing.Count);
    }

    private async Task SeedLaboratoriesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Laboratories.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Laboratories.AddRange(
            new Laboratory { Name = "NovaLab Frankfurt", City = "Frankfurt" },
            new Laboratory { Name = "NovaLab Mainz", City = "Mainz" },
            new Laboratory { Name = "NovaLab Darmstadt", City = "Darmstadt" });

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded laboratories.");
    }

    private async Task SeedAnalysisCategoriesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.AnalysisCategories.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.AnalysisCategories.AddRange(
            new AnalysisCategory { Name = "Hematology", ExpectedProcessingHours = 4 },
            new AnalysisCategory { Name = "Clinical Chemistry", ExpectedProcessingHours = 6 },
            new AnalysisCategory { Name = "Microbiology", ExpectedProcessingHours = 24 },
            new AnalysisCategory { Name = "Molecular Diagnostics", ExpectedProcessingHours = 48 });

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded analysis categories.");
    }

    private static readonly string[] LegacyDefaultGraphItemNames =
    [
        "Analysis Volume",
        "Analysis Status",
        "Average Processing Time",
        "Laboratory Workload"
    ];

    private async Task RemoveLegacyDefaultGraphItemsAsync(CancellationToken cancellationToken)
    {
        var removed = await dbContext.GraphItems
            .Where(item => LegacyDefaultGraphItemNames.Contains(item.Name) && item.Content == null)
            .ExecuteDeleteAsync(cancellationToken);

        if (removed > 0)
        {
            logger.LogInformation("Removed {Count} placeholder graph items from the dashboard.", removed);
        }
    }

    private async Task SeedLabAnalysesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.LabAnalyses.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Synthetic lab analyses already exist. Skipping analysis seed.");
            return;
        }

        var laboratories = await dbContext.Laboratories.AsNoTracking().ToListAsync(cancellationToken);
        var categories = await dbContext.AnalysisCategories.AsNoTracking().ToListAsync(cancellationToken);

        if (laboratories.Count == 0 || categories.Count == 0)
        {
            throw new InvalidOperationException("Laboratories and analysis categories must be seeded before lab analyses.");
        }

        var random = new Random(RandomSeed);
        var now = DateTime.UtcNow;
        var windowStart = now.AddMonths(-12);
        var analyses = new List<LabAnalysis>(SyntheticAnalysisCount);

        for (var i = 1; i <= SyntheticAnalysisCount; i++)
        {
            var laboratory = laboratories[random.Next(laboratories.Count)];
            var category = categories[random.Next(categories.Count)];
            var receivedAt = RandomDate(random, windowStart, now);
            var priority = NextPriority(random);
            var status = NextStatus(random, now, receivedAt);

            var (startedAt, completedAt) = BuildTimeline(
                random,
                now,
                receivedAt,
                status,
                priority,
                category.ExpectedProcessingHours);

            analyses.Add(new LabAnalysis
            {
                AnalysisNumber = $"LI-{i:D6}",
                LaboratoryId = laboratory.Id,
                AnalysisCategoryId = category.Id,
                ReceivedAt = receivedAt,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Status = status,
                Priority = priority
            });
        }

        const int batchSize = 1_000;
        for (var offset = 0; offset < analyses.Count; offset += batchSize)
        {
            var batch = analyses.Skip(offset).Take(batchSize);
            await dbContext.LabAnalyses.AddRangeAsync(batch, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        logger.LogInformation("Seeded {Count} synthetic lab analyses.", SyntheticAnalysisCount);
    }

    private static AnalysisPriority NextPriority(Random random)
    {
        var roll = random.NextDouble();
        if (roll < 0.80)
        {
            return AnalysisPriority.Normal;
        }

        if (roll < 0.95)
        {
            return AnalysisPriority.High;
        }

        return AnalysisPriority.Urgent;
    }

    private static AnalysisStatus NextStatus(Random random, DateTime now, DateTime receivedAt)
    {
        var ageHours = (now - receivedAt).TotalHours;
        var roll = random.NextDouble();

        if (ageHours < 12)
        {
            if (roll < 0.40) return AnalysisStatus.Pending;
            if (roll < 0.75) return AnalysisStatus.Processing;
            if (roll < 0.88) return AnalysisStatus.Completed;
            if (roll < 0.95) return AnalysisStatus.Delayed;
            return AnalysisStatus.Cancelled;
        }

        if (ageHours < 72)
        {
            if (roll < 0.10) return AnalysisStatus.Pending;
            if (roll < 0.28) return AnalysisStatus.Processing;
            if (roll < 0.82) return AnalysisStatus.Completed;
            if (roll < 0.93) return AnalysisStatus.Delayed;
            return AnalysisStatus.Cancelled;
        }

        if (roll < 0.88) return AnalysisStatus.Completed;
        if (roll < 0.95) return AnalysisStatus.Delayed;
        if (roll < 0.99) return AnalysisStatus.Cancelled;
        return AnalysisStatus.Processing;
    }

    private static (DateTime? StartedAt, DateTime? CompletedAt) BuildTimeline(
        Random random,
        DateTime now,
        DateTime receivedAt,
        AnalysisStatus status,
        AnalysisPriority priority,
        decimal expectedProcessingHours)
    {
        var expectedHours = (double)expectedProcessingHours;
        var startDelayHours = priority switch
        {
            AnalysisPriority.Urgent => NextDouble(random, 0.05, 0.4),
            AnalysisPriority.High => NextDouble(random, 0.2, 1.5),
            _ => NextDouble(random, 0.5, 4)
        };

        var processingHours = status == AnalysisStatus.Delayed
            ? expectedHours * NextDouble(random, 1.2, 2.4)
            : expectedHours * priority switch
            {
                AnalysisPriority.Urgent => NextDouble(random, 0.35, 0.7),
                AnalysisPriority.High => NextDouble(random, 0.55, 0.9),
                _ => NextDouble(random, 0.7, 1.05)
            };

        DateTime? startedAt = null;
        DateTime? completedAt = null;

        switch (status)
        {
            case AnalysisStatus.Pending:
                break;

            case AnalysisStatus.Processing:
                startedAt = ClampToRange(receivedAt.AddHours(startDelayHours), receivedAt, now);
                break;

            case AnalysisStatus.Completed:
                startedAt = ClampToRange(receivedAt.AddHours(startDelayHours), receivedAt, now);
                completedAt = ClampToRange(startedAt.Value.AddHours(processingHours), startedAt.Value, now);
                break;

            case AnalysisStatus.Delayed:
                startedAt = ClampToRange(receivedAt.AddHours(startDelayHours), receivedAt, now);
                if ((now - startedAt.Value).TotalHours < expectedHours)
                {
                    startedAt = ClampToRange(now.AddHours(-expectedHours * NextDouble(random, 1.1, 1.8)), receivedAt, now);
                }

                if (random.NextDouble() < 0.55)
                {
                    completedAt = ClampToRange(startedAt.Value.AddHours(processingHours), startedAt.Value, now);
                }

                break;

            case AnalysisStatus.Cancelled:
                if (random.NextDouble() < 0.35)
                {
                    startedAt = ClampToRange(receivedAt.AddHours(startDelayHours), receivedAt, now);
                }

                break;
        }

        return (startedAt, completedAt);
    }

    private static DateTime RandomDate(Random random, DateTime start, DateTime end)
    {
        var rangeTicks = end.Ticks - start.Ticks;
        var offset = (long)(random.NextDouble() * rangeTicks);
        return new DateTime(start.Ticks + offset, DateTimeKind.Utc);
    }

    private static double NextDouble(Random random, double min, double max)
    {
        return min + (random.NextDouble() * (max - min));
    }

    private static DateTime ClampToRange(DateTime value, DateTime min, DateTime max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
