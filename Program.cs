

using Scalar.AspNetCore;
using TmsApi.Middleware;
using TmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI JSON
app.MapOpenApi();

// Scalar UI
app.MapScalarApiReference();

// Controllers
app.MapControllers();

app.Run();