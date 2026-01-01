using Serilog;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IDictionary<string, string>>(new Dictionary<string, string>());
builder.Services.AddScoped<IAuditService, AuditService>();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    );

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );

var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 10,
    maxQueuingActions: 20
);

var fallbackPolicy = Policy<HttpResponseMessage>
    .Handle<Exception>()
    .FallbackAsync(
        new HttpResponseMessage(System.Net.HttpStatusCode.Accepted),
        onFallbackAsync: async ex =>
        {
            // log fallback
            await Task.CompletedTask;
        });

builder.Services.AddHttpClient("AuditClient", client =>
{
    client.BaseAddress = new Uri("https://example.com/");
})
.AddPolicyHandler(fallbackPolicy)
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(timeoutPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddPolicyHandler(bulkheadPolicy);




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
