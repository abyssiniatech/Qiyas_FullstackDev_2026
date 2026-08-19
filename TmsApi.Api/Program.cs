
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Hubs;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Workers;
using TmsApi.Application.Notifications;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Common;
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configuration
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("TmsDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'TmsDatabase' was not found. " +
        "Check appsettings.json or appsettings.Development.json.");
}

// ============================================================
// MVC / API
// ============================================================

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ============================================================
// Antiforgery / XSRF
// ============================================================

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// ============================================================
// SignalR
// ============================================================

builder.Services.AddSignalR();

// ============================================================
// API Versioning
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
    });

// ============================================================
// OpenAPI
// ============================================================

builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.ShouldInclude =
            description =>
                description.GroupName == "v1";
    });

builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options.ShouldInclude =
            description =>
                description.GroupName == "v2";
    });

// ============================================================
// Database
// ONE DbContext ONLY
// ============================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseNpgsql(connectionString);
    });

// ============================================================
// MediatR / CQRS
// ============================================================

builder.Services.AddMediatR(
    configuration =>
    {
        configuration.RegisterServicesFromAssembly(
            typeof(EnrollStudentCommand).Assembly);
    });

// ============================================================
// FluentValidation
// ============================================================

builder.Services.AddValidatorsFromAssemblyContaining<
    EnrollStudentCommand>();

// ============================================================
// MediatR Pipeline Behaviors
// ============================================================

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// ============================================================
// Hybrid Cache
// ============================================================

builder.Services.AddHybridCache(
    options =>
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
// Application / Infrastructure Services
// ============================================================

// Student
builder.Services.AddScoped<StudentService>();

// Course
builder.Services.AddScoped<
    ICourseService,
    CourseService>();

builder.Services.AddScoped<
    ICachedCourseService,
    CachedCourseService>();

// Enrollment
builder.Services.AddScoped<
    IEnrollmentService,
    EnrollmentService>();

// Grade
builder.Services.AddScoped<
    IGradeService,
    GradeService>();

// ============================================================
// Transcript Processing
// ============================================================

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();

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

builder.Services.AddHostedService<
    TranscriptWorker>();

// ============================================================
// CORS
// ============================================================

var allowedOrigins =
    builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "TmsClient",
            policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetPreflightMaxAge(
                        TimeSpan.FromMinutes(10));
            });
    });

// ============================================================
// Rate Limiting
// ============================================================

builder.Services.AddSingleton<
    ApiKeyResolver>();

builder.Services.AddRateLimiter(
    options =>
    {
        // ----------------------------------------------------
        // Global API Rate Limiter
        // ----------------------------------------------------

        options.GlobalLimiter =
            PartitionedRateLimiter.Create<
                HttpContext,
                string>(
                httpContext =>
                {
                    var resolver =
                        httpContext
                            .RequestServices
                            .GetRequiredService<
                                ApiKeyResolver>();

                    var tier =
                        resolver.Resolve(
                            httpContext);

                    return tier switch
                    {
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

        // ----------------------------------------------------
        // Transcript Concurrency Limiter
        // ----------------------------------------------------

        options.AddConcurrencyLimiter(
            "transcripts",
            limiter =>
            {
                limiter.PermitLimit = 5;

                limiter.QueueLimit = 20;

                limiter.QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst;
            });

        // ----------------------------------------------------
        // Search Rate Limiter
        // ----------------------------------------------------

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

        // ----------------------------------------------------
        // Rejection Response
        // ----------------------------------------------------

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

                context.HttpContext
                    .Response
                    .Headers
                    .RetryAfter =
                    retryAfter;

                context.HttpContext
                    .Response
                    .ContentType =
                    "application/problem+json";

                await context.HttpContext
                    .Response
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
// Build Application
// ============================================================

var app = builder.Build();

// ============================================================
// ProblemDetails / Exception Handling
// ============================================================

app.UseStatusCodePages();

app.UseExceptionHandler();

// ============================================================
// OpenAPI
// ============================================================

app.MapOpenApi();

// ============================================================
// Scalar API Documentation
// ============================================================

app.MapScalarApiReference(
    options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(
                ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient)
            .AddDocument(
                "v1",
                "API Version 1.0")
            .AddDocument(
                "v2",
                "API Version 2.0");
    });

// ============================================================
// CORS
// ============================================================

app.UseCors("TmsClient");

// ============================================================
// Rate Limiting
// ============================================================

app.UseRateLimiter();

// ============================================================
// Authentication
// ============================================================

app.UseAuthentication();

// ============================================================
// Authorization
// ============================================================

app.UseAuthorization();

// ============================================================
// XSRF Cookie
// ============================================================

app.Use(
    async (context, next) =>
    {
        if (context.Request.Cookies.ContainsKey("tms_auth"))
        {
            var antiforgery =
                context.RequestServices
                    .GetRequiredService<IAntiforgery>();

            var tokens =
                antiforgery.GetAndStoreTokens(
                    context);

            if (!string.IsNullOrWhiteSpace(
                    tokens.RequestToken))
            {
                context.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokens.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false,

                        Secure =
                            !app.Environment
                                .IsDevelopment(),

                        SameSite =
                            SameSiteMode.Strict,

                        Path = "/"
                    });
            }
        }

        await next(context);
    });

// ============================================================
// SignalR
// Module 10 - Part C
// ============================================================

app.MapHub<TmsHub>("/hubs/tms")
   .RequireCors("TmsClient");

// ============================================================
// Controllers
// ============================================================

app.MapControllers();

// ============================================================
// Run
// ============================================================

app.Run();