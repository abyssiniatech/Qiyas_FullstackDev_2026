using System.Threading.Channels;
using System.Threading.RateLimiting;

using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

using TmsApi.Api.Hubs;
using TmsApi.Api.RateLimiting;

using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Transcripts;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS + SIGNALR
// ============================================================

builder.Services.AddControllers();

builder.Services.AddSignalR();

// ============================================================
// API VERSIONING + API EXPLORER + OPENAPI
// ============================================================

builder.Services
.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
    new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified =
        true;

    options.ReportApiVersions =
        true;

    options.ApiVersionReader =
        new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat =
        "'v'VVV";

    options.SubstituteApiVersionInUrl =
        true;
})
.AddOpenApi();


// ============================================================
// LOGGING
// ============================================================

builder.Logging.AddConsole();

// ============================================================
// DATABASE - POSTGRESQL
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
    builder.Configuration
    .GetConnectionString("TmsDatabase"));
});

// ============================================================
// MEDIATR
// ============================================================

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
    typeof(EnrollStudentCommand).Assembly);
});

// ============================================================
// FLUENT VALIDATION
// ============================================================

builder.Services
.AddValidatorsFromAssemblyContaining<
EnrollStudentCommand>();

// ============================================================
// MEDIATR PIPELINE
// ============================================================

builder.Services.AddTransient(
typeof(IPipelineBehavior<,>),
typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
typeof(IPipelineBehavior<,>),
typeof(ValidationBehavior<,>));

// ============================================================
// HYBRID CACHE
// ============================================================

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
    new Microsoft.Extensions.Caching.Hybrid
    .HybridCacheEntryOptions
    {
        Expiration =
    TimeSpan.FromMinutes(10),


        LocalCacheExpiration =
                TimeSpan.FromMinutes(2)
    };


});

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
IStudentService,
StudentService>();

builder.Services.AddScoped<
ICourseService,
CourseService>();

builder.Services.AddScoped<
ICachedCourseService,
CachedCourseService>();

builder.Services.AddScoped<
IEnrollmentService,
EnrollmentService>();

// ============================================================
// TRANSCRIPT STATUS STORE
// ============================================================

// Shared in-memory status store.
//
// Singleton is required because the controller and the
// background worker must see the same transcript states.

builder.Services.AddSingleton<
ITranscriptStatusStore,
InMemoryTranscriptStatusStore>();

// ============================================================
// TRANSCRIPT NOTIFICATION SERVICE
// ============================================================

// SignalR IHubContext is safe to use from a singleton service.
// The service contains no request-specific mutable state.

builder.Services.AddSingleton<
ITranscriptNotificationService,
SignalRTranscriptNotificationService>();

// ============================================================
// TRANSCRIPT BACKGROUND PROCESSING
// ============================================================

// Shared channel between TranscriptsController
// and TranscriptWorker.

builder.Services.AddSingleton<
Channel<TranscriptRequest>>(
_ =>
Channel.CreateBounded<TranscriptRequest>(
new BoundedChannelOptions(100)
{
    FullMode =
BoundedChannelFullMode.Wait,


    SingleReader = true,

    SingleWriter = false
}));


// Background worker.

builder.Services.AddHostedService<
TranscriptWorker>();

// ============================================================
// PROBLEM DETAILS
// ============================================================

builder.Services.AddProblemDetails();

// ============================================================
// CORS - ANGULAR
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
    "AngularClient",
    policy =>
    {
        policy
    .WithOrigins(
    "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
    });
});

// ============================================================
// RATE LIMITING
// ============================================================

builder.Services.AddSingleton<ApiKeyResolver>();

builder.Services.AddRateLimiter(options =>
{
    // ========================================================
    // GLOBAL RATE LIMITER
    // ========================================================


    options.GlobalLimiter =
        PartitionedRateLimiter
            .Create<HttpContext, string>(
                httpContext =>
                {
                    var resolver =
                        httpContext.RequestServices
                            .GetRequiredService<
                                ApiKeyResolver>();

                    var tier =
                        resolver.Resolve(
                            httpContext);

                    return tier switch
                    {
                        // =================================================
                        // PAID
                        // =================================================

                        ApiKeyTier.Paid =>
                            RateLimitPartition
                                .GetTokenBucketLimiter(
                                    "paid",
                                    _ =>
                                        new TokenBucketRateLimiterOptions
                                        {
                                            TokenLimit = 200,

                                            TokensPerPeriod = 100,

                                            ReplenishmentPeriod =
                                                TimeSpan.FromSeconds(10),

                                            QueueLimit = 0,

                                            AutoReplenishment = true
                                        }),

                        // =================================================
                        // FREE
                        // =================================================

                        ApiKeyTier.Free =>
                            RateLimitPartition
                                .GetTokenBucketLimiter(
                                    "free",
                                    _ =>
                                        new TokenBucketRateLimiterOptions
                                        {
                                            TokenLimit = 30,

                                            TokensPerPeriod = 10,

                                            ReplenishmentPeriod =
                                                TimeSpan.FromSeconds(10),

                                            QueueLimit = 0,

                                            AutoReplenishment = true
                                        }),

                        // =================================================
                        // ANONYMOUS
                        // =================================================

                        _ =>
                            RateLimitPartition
                                .GetTokenBucketLimiter(
                                    "anonymous",
                                    _ =>
                                        new TokenBucketRateLimiterOptions
                                        {
                                            TokenLimit = 10,

                                            TokensPerPeriod = 5,

                                            ReplenishmentPeriod =
                                                TimeSpan.FromSeconds(10),

                                            QueueLimit = 0,

                                            AutoReplenishment = true
                                        })
                    };
                });

    // ========================================================
    // TRANSCRIPT CONCURRENCY LIMITER
    // ========================================================

    options.AddConcurrencyLimiter(
        "transcripts",
        limiter =>
        {
            limiter.PermitLimit = 5;

            limiter.QueueLimit = 20;

            limiter.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });

    // ========================================================
    // SEARCH TOKEN BUCKET
    // ========================================================

    options.AddTokenBucketLimiter(
        "search",
        limiter =>
        {
            limiter.TokenLimit = 10;

            limiter.TokensPerPeriod = 5;

            limiter.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);

            limiter.QueueLimit = 2;

            limiter.AutoReplenishment = true;
        });

    // ========================================================
    // REJECTION
    // ========================================================

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected =
        async (
            context,
            cancellationToken) =>
        {
            var retryAfter = "10";

            if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retry))
            {
                retryAfter =
                    ((int)retry.TotalSeconds)
                    .ToString();
            }

            context.HttpContext.Response
                .Headers.RetryAfter =
                retryAfter;

            context.HttpContext.Response
                .ContentType =
                "application/problem+json";

            await context.HttpContext.Response
                .WriteAsJsonAsync(
                    new
                    {
                        title =
                            "Rate limit exceeded",

                        detail =
                            $"Too many requests. " +
                            $"Retry after " +
                            $"{retryAfter} seconds.",

                        status = 429
                    },
                    cancellationToken);
        };


});

// ============================================================
// BUILD
// ============================================================

var app = builder.Build();

// ============================================================
// EXCEPTION HANDLING
// ============================================================

app.UseExceptionHandler();

// ============================================================
// OPENAPI + SCALAR
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
    .WithDocumentPerVersion();


    app.MapScalarApiReference(
        options =>
        {
            var descriptions =
                app.DescribeApiVersions();

            for (
                var i = 0;
                i < descriptions.Count;
                i++)
            {
                var description =
                    descriptions[i];

                var documentName =
                    description.GroupName!;

                var title =
                    $"TMS API {documentName.ToUpperInvariant()}";

                var isDefault =
                    documentName == "v2";

                options.AddDocument(
                    documentName,
                    title,
                    isDefault: isDefault);
            }
        });


}

// ============================================================
// HTTPS
// ============================================================

// Current testing uses:
// http://localhost:5071
//
// HTTPS redirection remains disabled for now.

// app.UseHttpsRedirection();

// ============================================================
// CORS
// ============================================================

app.UseCors("AngularClient");

// ============================================================
// RATE LIMITER
// ============================================================

app.UseRateLimiter();

// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();

// ============================================================
// SIGNALR HUB
// ============================================================

app.MapHub<TmsHub>(
"/hubs/tms");

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// DATABASE MIGRATION
// ============================================================

using (var scope =
app.Services.CreateScope())
{
    var db =
    scope.ServiceProvider
    .GetRequiredService<AppDbContext>();


    db.Database.Migrate();


}

// ============================================================
// RUN
// ============================================================

app.Run();
