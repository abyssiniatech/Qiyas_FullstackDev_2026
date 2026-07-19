using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Tms.Api.Services;
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


// Controllers
builder.Services.AddControllers();


// EF Core PostgreSQL
builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
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
builder.Services.AddScoped<ICourseService, CourseService>();


// Future Module 7 services
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();


// =============================
// OPENAPI
// =============================

builder.Services.AddOpenApi();



// =============================
// BUILD APPLICATION
// =============================

var app = builder.Build();



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
// AUTHENTICATION / AUTHORIZATION
// =============================

// Add Authentication here later
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