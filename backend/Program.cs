using System.Text.Json.Serialization;
using LabInsight.Api.Data;
using LabInsight.Api.Repositories;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

BindKestrelToPlatformPort(builder);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set ConnectionStrings__DefaultConnection to the PostgreSQL connection string.");
}

const string frontendCorsPolicy = "FrontendCors";
var allowedOrigins = ResolveCorsOrigins(builder);
if (allowedOrigins.Count == 0)
{
    throw new InvalidOperationException(
        "Set Cors__AllowedOrigin to the deployed frontend origin (for example https://your-app.vercel.app).");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LabInsightDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddScoped<ILaboratoryRepository, LaboratoryRepository>();
builder.Services.AddScoped<IAnalysisCategoryRepository, AnalysisCategoryRepository>();
builder.Services.AddScoped<ILabAnalysisRepository, LabAnalysisRepository>();
builder.Services.AddScoped<IGraphTypeRepository, GraphTypeRepository>();
builder.Services.AddScoped<IGraphDataTypeRepository, GraphDataTypeRepository>();
builder.Services.AddScoped<IGraphItemRepository, GraphItemRepository>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<ILaboratoryService, LaboratoryService>();
builder.Services.AddScoped<IAnalysisCategoryService, AnalysisCategoryService>();
builder.Services.AddScoped<ILabAnalysisService, LabAnalysisService>();
builder.Services.AddScoped<IGraphTypeService, GraphTypeService>();
builder.Services.AddScoped<IGraphDataTypeService, GraphDataTypeService>();
builder.Services.AddScoped<IGraphItemService, GraphItemService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.WithOrigins([.. allowedOrigins])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(frontendCorsPolicy);
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LabInsightDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await MigrateAndSeedAsync(dbContext, seeder, app.Environment.IsDevelopment());
}

app.Run();

static void BindKestrelToPlatformPort(WebApplicationBuilder builder)
{
    if (builder.Environment.IsDevelopment())
    {
        return;
    }

    var port = Environment.GetEnvironmentVariable("PORT");
    if (string.IsNullOrWhiteSpace(port))
    {
        port = "8080";
    }

    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

static List<string> ResolveCorsOrigins(WebApplicationBuilder builder)
{
    var origins = new List<string>();
    var configured = builder.Configuration["Cors:AllowedOrigin"]?.Trim().TrimEnd('/');

    if (builder.Environment.IsDevelopment())
    {
        origins.Add("http://localhost:4200");
        if (!string.IsNullOrWhiteSpace(configured) &&
            !string.Equals(configured, "http://localhost:4200", StringComparison.OrdinalIgnoreCase))
        {
            origins.Add(configured);
        }

        return origins;
    }

    if (!string.IsNullOrWhiteSpace(configured))
    {
        origins.Add(configured);
    }

    return origins;
}

static async Task MigrateAndSeedAsync(
    LabInsightDbContext dbContext,
    DatabaseSeeder seeder,
    bool isDevelopment)
{
    var maxAttempts = isDevelopment ? 1 : 8;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            await seeder.SeedAsync();
            return;
        }
        catch (Exception) when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
