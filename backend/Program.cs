using System.Text.Json.Serialization;
using LabInsight.Api.Data;
using LabInsight.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string angularCorsPolicy = "AngularDevelopment";
var angularOrigin = builder.Configuration["Cors:AngularOrigin"] ?? "http://localhost:4200";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LabInsightDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<ILaboratoryService, LaboratoryService>();
builder.Services.AddScoped<IAnalysisCategoryService, AnalysisCategoryService>();
builder.Services.AddScoped<ILabAnalysisService, LabAnalysisService>();
builder.Services.AddScoped<IGraphTypeService, GraphTypeService>();
builder.Services.AddScoped<IGraphDataTypeService, GraphDataTypeService>();
builder.Services.AddScoped<IGraphItemService, GraphItemService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(angularCorsPolicy, policy =>
    {
        policy.WithOrigins(angularOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(angularCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LabInsightDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await dbContext.Database.MigrateAsync();
    await seeder.SeedAsync();
}

app.Run();
