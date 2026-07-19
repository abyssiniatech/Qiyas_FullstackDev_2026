using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Tms.Api.Persistence;
using Tms.Api.Services;
using Tms.Api.Filters;
using TmsApi.Data;
using TmsApi.Services;


// =============================
// BUILDER
// =============================

var builder = WebApplication.CreateBuilder(args);


// =============================
// SERVICES
// =============================


// Problem Details for API errors
builder.Services.AddProblemDetails();


// Controllers + Global Audit Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});



// =============================
// DATABASE
// =============================


// EF Core PostgreSQL

builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration
        .GetConnectionString("TmsDatabase")
    );


    // Development only
    options.EnableSensitiveDataLogging();


    // SQL query logging
    options.LogTo(
        Console.WriteLine,
        LogLevel.Information
    );
});



// =============================
// APPLICATION SERVICES
// =============================


// Course Service
builder.Services.AddScoped<
    ICourseService,
    CourseService>();


// Enrollment Service
builder.Services.AddScoped<
    IEnrollmentService,
    EnrollmentService>();



// =============================
// OPENAPI
// =============================

builder.Services.AddOpenApi();



// =============================
// BUILD APPLICATION
// =============================

var app = builder.Build();



// =============================
// DATABASE SEEDER
// =============================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    await DataSeeder.SeedAsync(context);
}



// =============================
// HTTP PIPELINE
// =============================


// Global exception handling

app.UseExceptionHandler();


// HTTPS

app.UseHttpsRedirection();


// Status code handling

app.UseStatusCodePages();



// =============================
// OPENAPI + SCALAR
// =============================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();


    app.MapScalarApiReference(options =>
    {
        options.Title = "TMS API Documentation";
    });
}



// =============================
// AUTHORIZATION
// =============================


// Authentication will come later

// app.UseAuthentication();


app.UseAuthorization();



// =============================
// CONTROLLERS
// =============================

app.MapControllers();



// =============================
// RUN
// =============================

app.Run();