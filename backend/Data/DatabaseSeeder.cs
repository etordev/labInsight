using LabInsight.Api.Catalog;
using LabInsight.Api.Entities;
using LabInsight.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Data;

public class DatabaseSeeder(LabInsightDbContext dbContext, ILogger<DatabaseSeeder> logger)
{
    private const int SyntheticAnalysisCount = 15_000;
    private const int RandomSeed = 42;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedGraphTypesAsync(cancellationToken);
        await SeedGraphDataTypesAsync(cancellationToken);
        await SeedLaboratoriesAsync(cancellationToken);
        await SeedAnalysisCategoriesAsync(cancellationToken);
        await SeedLabAnalysesAsync(cancellationToken);
        await SeedDemoGraphItemsAsync(cancellationToken);
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

    private async Task SeedLabAnalysesAsync(CancellationToken cancellationToken)
    {
        var existingCount = await dbContext.LabAnalyses.CountAsync(cancellationToken);
        if (existingCount == SyntheticAnalysisCount)
        {
            logger.LogInformation("Synthetic lab analyses already exist ({Count}). Skipping analysis seed.", existingCount);
            return;
        }

        if (existingCount == 10_000)
        {
            logger.LogInformation("Replacing previous 10,000-row analysis seed with {Count} records.", SyntheticAnalysisCount);
            await dbContext.LabAnalyses.ExecuteDeleteAsync(cancellationToken);
        }
        else if (existingCount > 0)
        {
            logger.LogInformation(
                "Lab analyses already exist ({Count}) and were not replaced. Skipping analysis seed.",
                existingCount);
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
        var monthWeights = BuildMonthWeights(random);
        var analyses = new List<LabAnalysis>(SyntheticAnalysisCount);

        for (var i = 1; i <= SyntheticAnalysisCount; i++)
        {
            var laboratory = PickLaboratory(laboratories, random);
            var category = PickCategory(categories, random);
            var receivedAt = RandomWeightedDate(random, windowStart, now, monthWeights);
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

    private async Task SeedDemoGraphItemsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.GraphItems.AnyAsync(cancellationToken))
        {
            return;
        }

        var graphTypes = await dbContext.GraphTypes.ToDictionaryAsync(
            type => type.TechnicalName,
            cancellationToken);
        var graphDataTypes = await dbContext.GraphDataTypes.ToDictionaryAsync(
            type => type.TechnicalName,
            cancellationToken);

        dbContext.GraphItems.AddRange(
            CreateDemoGraphItem(
                "Analysis Volume",
                "Laboratory analysis volume over the last 12 months",
                "LINE_CHART",
                "ANALYSIS_VOLUME",
                """{"groupBy":"MONTH"}""",
                graphTypes,
                graphDataTypes),
            CreateDemoGraphItem(
                "Analysis Status",
                "Distribution of laboratory analysis statuses",
                "DOUGHNUT_CHART",
                "ANALYSIS_STATUS",
                null,
                graphTypes,
                graphDataTypes),
            CreateDemoGraphItem(
                "Average Processing Time",
                "Average processing time by analysis category",
                "BAR_CHART",
                "PROCESSING_TIME",
                null,
                graphTypes,
                graphDataTypes),
            CreateDemoGraphItem(
                "Laboratory Workload",
                "Current analysis workload by laboratory",
                "BAR_CHART",
                "LABORATORY_WORKLOAD",
                null,
                graphTypes,
                graphDataTypes));

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded default dashboard graph items.");
    }

    private static GraphItemEntity CreateDemoGraphItem(
        string name,
        string description,
        string graphTypeName,
        string graphDataTypeName,
        string? content,
        IReadOnlyDictionary<string, GraphTypeEntity> graphTypes,
        IReadOnlyDictionary<string, GraphDataTypeEntity> graphDataTypes)
    {
        return new GraphItemEntity
        {
            Name = name,
            Description = description,
            Content = content,
            GraphTypeId = graphTypes[graphTypeName].Id,
            GraphDataTypeId = graphDataTypes[graphDataTypeName].Id
        };
    }

    private static Laboratory PickLaboratory(IReadOnlyList<Laboratory> laboratories, Random random)
    {
        var roll = random.NextDouble();
        var preferredName = roll < 0.52
            ? "NovaLab Frankfurt"
            : roll < 0.80
                ? "NovaLab Mainz"
                : "NovaLab Darmstadt";

        return laboratories.FirstOrDefault(lab => lab.Name == preferredName) ?? laboratories[random.Next(laboratories.Count)];
    }

    private static AnalysisCategory PickCategory(IReadOnlyList<AnalysisCategory> categories, Random random)
    {
        var roll = random.NextDouble();
        var preferredName = roll < 0.28
            ? "Hematology"
            : roll < 0.60
                ? "Clinical Chemistry"
                : roll < 0.78
                    ? "Microbiology"
                    : "Molecular Diagnostics";

        return categories.FirstOrDefault(category => category.Name == preferredName)
               ?? categories[random.Next(categories.Count)];
    }

    private static double[] BuildMonthWeights(Random random)
    {
        var weights = new double[12];
        for (var index = 0; index < 12; index++)
        {
            weights[index] = 0.82 + (0.28 * Math.Sin(index * Math.PI / 6.0)) + NextDouble(random, -0.04, 0.04);
            if (weights[index] < 0.55)
            {
                weights[index] = 0.55;
            }
        }

        return weights;
    }

    private static DateTime RandomWeightedDate(
        Random random,
        DateTime windowStart,
        DateTime now,
        IReadOnlyList<double> monthWeights)
    {
        var total = monthWeights.Sum();
        var pick = random.NextDouble() * total;
        var cumulative = 0.0;
        var monthOffset = 11;

        for (var index = 0; index < 12; index++)
        {
            cumulative += monthWeights[index];
            if (pick <= cumulative)
            {
                monthOffset = index;
                break;
            }
        }

        var monthStart = new DateTime(windowStart.Year, windowStart.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(monthOffset);
        var monthEnd = monthStart.AddMonths(1);
        if (monthStart < windowStart)
        {
            monthStart = windowStart;
        }

        if (monthEnd > now)
        {
            monthEnd = now;
        }

        if (monthEnd <= monthStart)
        {
            return now;
        }

        return RandomDate(random, monthStart, monthEnd);
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
