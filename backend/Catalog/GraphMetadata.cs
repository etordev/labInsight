using System.Collections.Generic;

namespace LabInsight.Api.Catalog;

public static class GraphMetadata
{
    public static readonly string[] GraphTypeTechnicalNames =
    [
        "BAR_CHART",
        "LINE_CHART",
        "PIE_CHART",
        "DOUGHNUT_CHART",
        "DATA_GRID"
    ];

    public static readonly string[] GraphDataTypeTechnicalNames =
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

    public static readonly HashSet<string> DateRangeGraphDataTypes = new(StringComparer.Ordinal)
    {
        "ANALYSIS_VOLUME",
        "PROCESSING_TIME",
        "COMPLETION_RATE"
    };

    public static bool SupportsDateRange(string graphDataTypeTechnicalName)
    {
        return DateRangeGraphDataTypes.Contains(graphDataTypeTechnicalName);
    }
}
