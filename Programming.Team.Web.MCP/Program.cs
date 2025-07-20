using Programming.Team.AI.MCP;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMcpServer().WithHttpTransport().
    WithTools<PercentMatchTool>().WithTools<SkillExtractionTool>().
    WithTools<TailorBioTool>().WithTools<TailorPositionTool>().WithTools<GenerateCoverLetterTool>().
    WithTools<ExtractCompanyNameTool>().WithTools<ResearchCompanyTool>().WithTools<ExtractPostingTitleTool>().WithTools<GenerateInterviewQuestionsTool>();
builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();

var app = builder.Build();
app.UseHttpsRedirection();

app.MapMcp();

app.Run();
