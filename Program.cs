using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Asp.Versioning;

using TmsApi.Data;
using TmsApi.Persistence;
using TmsApi.Services;
using Tms.Api.Services;
using TmsApi.Middleware;
using Tms.Api.Filters;


var builder = WebApplication.CreateBuilder(args);


// =============================
// DATABASE
// =============================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    );

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();

        options.LogTo(
            Console.WriteLine,
            LogLevel.Information
        );
    }
});


// =============================
// CONTROLLERS + FILTERS
// =============================

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<AuditLogFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });


// =============================
// API VERSIONING
// =============================

builder.Services
    .AddApiVersioning(options =>
    {
        // Default API version
        options.DefaultApiVersion =
            new ApiVersion(1, 0);


        // Allow:
        // /api/courses
        // while migrating clients
        options.AssumeDefaultVersionWhenUnspecified = true;


        // Adds:
        // api-supported-versions: 1.0,2.0
        options.ReportApiVersions = true;


        // VERSION READERS
        //
        // Primary:
        // /api/v1/courses
        // /api/v2/courses
        //
        // Partner escape hatch:
        // GET /api/courses
        // Header:
        // X-Api-Version: 2.0

        options.ApiVersionReader =
            ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );

    })
    .AddApiExplorer(options =>
    {
        // Scalar groups:
        // v1
        // v2

        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });


// =============================
// OPENAPI
// =============================

builder.Services.AddOpenApi();


// =============================
// APPLICATION SERVICES
// =============================

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<IStudentService, StudentService>();


// =============================
// BUILD APP
// =============================

var app = builder.Build();


// =============================
// V1 DEPRECATION HEADERS
// MUST BE BEFORE MapControllers
// =============================

app.UseMiddleware<V1DeprecationMiddleware>();


// =============================
// DATABASE MIGRATION + SEED
// =============================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();


    await context.Database.MigrateAsync();


    await DataSeeder.SeedAsync(context);
}


// =============================
// OPENAPI + SCALAR
// =============================

app.MapOpenApi();


app.MapScalarApiReference(options =>
{
    options.Title = "TMS API Documentation";
});


// =============================
// CONTROLLERS
// =============================

app.MapControllers();


app.Run();