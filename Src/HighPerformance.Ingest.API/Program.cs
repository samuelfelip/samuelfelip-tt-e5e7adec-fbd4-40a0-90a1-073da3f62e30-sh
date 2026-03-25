using System.Reflection;
using HighPerformance.Ingest.API.Handlers;
using HighPerformance.Ingest.Application;
using HighPerformance.Ingest.Application.Settings;
using HighPerformance.Ingest.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HighPerformance Ingest API",
        Version = "v1",
        Description =
            "Leaderboard append-only ingest and read APIs. " +
            "When sending a timestamp, use ISO-8601 with timezone (UTC suffix Z or numeric offset). " +
            "Bulk ingest is limited by LeaderboardSettings:MaxScoreBatchSize."
    });

    var apiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(apiXml))
        options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);

    var applicationXml = Path.Combine(AppContext.BaseDirectory, "HighPerformance.Ingest.Application.xml");
    if (File.Exists(applicationXml))
        options.IncludeXmlComments(applicationXml);
});

builder.Services.Configure<LeaderboardSettings>(builder.Configuration.GetSection(LeaderboardSettings.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
