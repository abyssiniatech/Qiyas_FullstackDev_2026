using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

using Tms.Api.Filters;
using Tms.Api.Persistence;
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


// API Error Handling
builder.Services.AddProblemDetails();


// Controllers + Global Audit Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();

})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});



// =============================
// DATABASE
// =============================


builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration
            .GetConnectionString("TmsDatabase")
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
// APPLICATION SERVICES
// =============================


builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();



// =============================
// OPENAPI
// =============================


builder.Services.AddOpenApi("v1");



// =============================
// BUILD APPLICATION
// =============================


var app = builder.Build();



// =============================
// DEVELOPMENT
// =============================


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}



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


// Exception handling FIRST
app.UseExceptionHandler();


// HTTPS
app.UseHttpsRedirection();


// Status code pages
app.UseStatusCodePages();



// =============================
// OPENAPI + SCALAR
// =============================


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");


    app.MapScalarApiReference(options =>
    {
        options.Title = "TMS API Documentation";

        options.OpenApiRoutePattern =
            "/openapi/{documentName}.json";
    });
}



// =============================
// AUTHORIZATION
// =============================


app.UseAuthorization();



// =============================
// CONTROLLERS
// =============================


app.MapControllers();



// =============================
// RUN
// =============================


app.Run();