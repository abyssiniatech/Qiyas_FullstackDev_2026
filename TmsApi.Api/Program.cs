using System.Threading.RateLimiting;

using Asp.Versioning;
using FluentValidation;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

using Scalar.AspNetCore;

using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);


// =================================
// Controllers
// =================================

builder.Services.AddControllers();


// =================================
// OpenAPI / Scalar
// =================================

builder.Services.AddOpenApi();


// =================================
// API Versioning
// =================================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(2, 0);

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.ReportApiVersions = true;

    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;

    });



builder.Logging.AddConsole();



// =================================
// Database PostgreSQL
// =================================

builder.Services.AddDbContext<AppDbContext>(options =>
{

    options.UseNpgsql(
        builder.Configuration
        .GetConnectionString("TmsDatabase"));

});




// =================================
// MediatR
// =================================

builder.Services.AddMediatR(cfg =>
{

    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentCommand).Assembly);

});



// =================================
// Fluent Validation
// =================================

builder.Services
    .AddValidatorsFromAssemblyContaining<
        EnrollStudentCommand>();




// =================================
// MediatR Pipeline
// =================================


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));




// =================================
// Hybrid Cache
// =================================

builder.Services.AddHybridCache(options =>
{

    options.DefaultEntryOptions =
        new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
        {

            Expiration =
                TimeSpan.FromMinutes(10),


            LocalCacheExpiration =
                TimeSpan.FromMinutes(2)

        };

});




// =================================
// Application Services
// =================================


builder.Services.AddScoped<IStudentService, StudentService>();


builder.Services.AddScoped<ICourseService, CourseService>();


builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();




// =================================
// Exception Handling
// =================================

// Register Problem Details for consistent error responses
builder.Services.AddProblemDetails();




// =================================
// CORS Angular
// =================================


builder.Services.AddCors(options =>
{

    options.AddPolicy(
        "AngularClient",
        policy =>
        {

            policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://localhost:4200");

        });

});





// =================================
// RATE LIMITING
// =================================


builder.Services.AddSingleton<ApiKeyResolver>();


builder.Services.AddRateLimiter(options =>
{


    // -----------------------------
    // Global Tier Limiter
    // -----------------------------


    options.GlobalLimiter =
        PartitionedRateLimiter
        .Create<HttpContext, string>(
        httpContext =>
        {


            var resolver =
                httpContext.RequestServices
                .GetRequiredService<ApiKeyResolver>();


            var result =
                resolver.Resolve(httpContext);



            return result switch
            {


                ApiKeyTier.Paid =>

                RateLimitPartition
                .GetTokenBucketLimiter(
                $"paid:{result}",
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
                $"free:{result}",
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
                $"anonymous:{result}",
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





    // -----------------------------
    // Transcript Concurrency Limiter
    // -----------------------------


    options.AddConcurrencyLimiter(
        "transcripts",
        limiter =>
        {

            limiter.PermitLimit = 5;


            limiter.QueueLimit = 20;


            limiter.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;

        });





    // -----------------------------
    // Search Limiter
    // -----------------------------


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





    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;



    options.OnRejected =
        async (context, cancellationToken) =>
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
                    $"Too many requests. Retry after {retryAfter} seconds.",


                    status = 429

                },
                cancellationToken);

        };

});





// =================================
// Build Application
// =================================


var app = builder.Build();





// =================================
// OpenAPI
// =================================


if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();


    app.MapScalarApiReference(options =>
    {

        options.Title =
            "TMS API";

    });

}




// =================================
// Middleware Pipeline
// =================================


app.UseExceptionHandler();


app.UseHttpsRedirection();


app.UseCors("AngularClient");


// IMPORTANT
// Rate limiter before controllers

app.UseRateLimiter();


app.UseAuthorization();




app.MapControllers();





// =================================
// Database Migration
// =================================


using (var scope = app.Services.CreateScope())
{

    var db =
        scope.ServiceProvider
        .GetRequiredService<AppDbContext>();


    db.Database.Migrate();

}





app.Run();