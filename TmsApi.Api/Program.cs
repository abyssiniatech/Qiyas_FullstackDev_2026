using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Services;
using TmsApi.Infrastructure.Seeding;
using TmsApi.Infrastructure.Services;
using TmsApi.Middleware;



var builder = WebApplication.CreateBuilder(args);


// ======================================================
// CQRS - MEDIATR
// ======================================================

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly);
});


// ======================================================
// FLUENT VALIDATION
// ======================================================

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);


// ======================================================
// MEDIATR PIPELINES
// ======================================================

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


// ======================================================
// GLOBAL EXCEPTION HANDLING
// ======================================================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();



// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration
        .GetConnectionString("TmsDatabase"));


    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();

        options.LogTo(
            Console.WriteLine,
            LogLevel.Information);
    }

});




// ======================================================
// CONTROLLERS
// ======================================================

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<AuditLogFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions
        .ReferenceHandler =
        System.Text.Json.Serialization
        .ReferenceHandler.IgnoreCycles;
    });





// ======================================================
// API VERSIONING
// ======================================================

builder.Services
.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1, 0);


    options.AssumeDefaultVersionWhenUnspecified =
        true;


    options.ReportApiVersions = true;


    options.ApiVersionReader =
        ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader(
                "X-Api-Version"));
})

.AddApiExplorer(options =>
{
    options.GroupNameFormat =
        "'v'VVV";


    options.SubstituteApiVersionInUrl =
        true;
});




// ======================================================
// OPEN API
// ======================================================


builder.Services.AddOpenApi("v1",
options =>
{
    options.ShouldInclude =
    description =>
    description.GroupName == "v1";
});


builder.Services.AddOpenApi("v2",
options =>
{
    options.ShouldInclude =
    description =>
    description.GroupName == "v2";
});




// ======================================================
// APPLICATION SERVICES
// ======================================================

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<StudentService>();

builder.Services.AddScoped<IDataSeeder, DataSeeder>();




// ======================================================
// BUILD
// ======================================================

var app = builder.Build();




// ======================================================
// ERROR PIPELINE
// ======================================================

// app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}



// ======================================================
// CUSTOM MIDDLEWARE
// ======================================================

app.UseMiddleware<V1DeprecationMiddleware>();




// ======================================================
// DATABASE SEED
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var seeder =
        scope.ServiceProvider
        .GetRequiredService<IDataSeeder>();


    await seeder.SeedAsync();
}




// ======================================================
// OPENAPI
// ======================================================

app.MapOpenApi(
"/openapi/{documentName}.json");




// ======================================================
// SCALAR
// ======================================================

app.MapScalarApiReference(options =>
{

    options.Title =
        "TMS API Documentation";


    options
        .AddDocument(
        "v1",
        "API Version 1.0")

        .AddDocument(
        "v2",
        "API Version 2.0");

});





// ======================================================
// CONTROLLERS
// ======================================================

app.MapControllers();



app.Run();




// ======================================================
// AUDIT FILTER PLACEHOLDER
// ======================================================

internal class AuditLogFilter : IFilterMetadata
{

}