using Scalar.AspNetCore;
using TmsApi.Middleware;
using TmsApi.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

var builder = WebApplication.CreateBuilder(args);


// -------------------------
// Services Configuration
// -------------------------

// Controllers
builder.Services.AddControllers();


// Database - PostgreSQL + EF Core
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    ));


// OpenAPI
builder.Services.AddOpenApi();



var app = builder.Build();


// -------------------------
// HTTP Request Pipeline
// -------------------------

// Correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();


// OpenAPI JSON endpoint
app.MapOpenApi();


// Scalar API Documentation
app.MapScalarApiReference();


// API Controllers
app.MapControllers();


app.Run();