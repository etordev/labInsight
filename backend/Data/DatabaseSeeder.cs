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
        await SeedGraphItemsAsync(cancellationToken);
        await SeedLabAnalysesAsync(cancellationToken);
    }

    private async Task SeedGraphTypesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.GraphTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        string[] names =
        [
            "BAR_CHART",
            "LINE_CHART",
            "PIE_CHART",
            "DOUGHNUT_CHART",
            "DATA_GRID"
        ];

        dbContext.GraphTypes.AddRange(names.Select(name => new GraphTypeEntity { TechnicalName = name }));
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} graph types.", names.Length);
    }

    private async Task SeedGraphDataTypesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.GraphDataTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        string[] names =
        [
            "ANALYSIS_VOLUME",
            "ANALYSIS_STATUS",
            "PROCESSING_TIME",
            "ANALYSIS_CATEGORY",
            "LABORATORY_WORKLOAD",
            "PRIORITY_DISTRIBUTION",
            "COMPLETION_RATE",
            "DELAYED_ANALYSES"
        ];

        dbContext.GraphDataTypes.AddRange(names.Select(name => new GraphDataTypeEntity { TechnicalName = name }));
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} graph data types.", names.Length);
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

    private async Task SeedGraphItemsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.GraphItems.AnyAsync(cancellationToken))
        {
            return;
        }

        var graphTypes = await dbContext.GraphTypes.ToDictionaryAsync(
            t => t.TechnicalName,
            cancellationToken);
        var graphDataTypes = await dbContext.GraphDataTypes.ToDictionaryAsync(
            t => t.TechnicalName,
            cancellationToken);

        dbContext.GraphItems.AddRange(
            CreateGraphItem(
                "Analysis Volume",
                "Laboratory analysis volume over time",
                graphTypes,
                graphDataTypes,
                "LINE_CHART",
                "ANALYSIS_VOLUME"),
            CreateGraphItem(
                "Analysis Status",
                "Distribution of laboratory analysis statuses",
                graphTypes,
                graphDataTypes,
                "DOUGHNUT_CHART",
                "ANALYSIS_STATUS"),
            CreateGraphItem(
                "Average Processing Time",
                "Average processing time by analysis category",
                graphTypes,
                graphDataTypes,
                "BAR_CHART",
                "PROCESSING_TIME"),
            CreateGraphItem(
                "Laboratory Workload",
                "Current analysis workload by laboratory",
                graphTypes,
                graphDataTypes,
                "BAR_CHART",
                "LABORATORY_WORKLOAD"));

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded default graph items.");
    }

    private static GraphItemEntity CreateGraphItem(
        string name,
        string description,
        IReadOnlyDictionary<string, GraphTypeEntity> graphTypes,
        IReadOnlyDictionary<string, GraphDataTypeEntity> graphDataTypes,
        string graphTypeName,
        string graphDataTypeName)
    {
        if (!graphTypes.TryGetValue(graphTypeName, out var graphType))
        {
            throw new InvalidOperationException($"Graph type '{graphTypeName}' was not found.");
        }

        if (!graphDataTypes.TryGetValue(graphDataTypeName, out var graphDataType))
        {
            throw new InvalidOperationException($"Graph data type '{graphDataTypeName}' was not found.");
        }

        return new GraphItemEntity
        {
            Name = name,
            Description = description,
            Content = null,
            GraphTypeId = graphType.Id,
            GraphDataTypeId = graphDataType.Id
        };
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
