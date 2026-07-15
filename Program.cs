using TmsApi.Middleware;
using TmsApi.Services;
using TmsApi.Models;


var builder = WebApplication.CreateBuilder(args);


// ======================================
// Service Registration (Dependency Injection)
// ======================================

builder.Services.AddAuthentication("Bearer");

builder.Services.AddAuthorization();

builder.Services.AddControllers();


// Register Enrollment Service
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();



var app = builder.Build();



// ======================================
// HTTP Request Pipeline
// ======================================


// Disable if HTTPS is not configured
// app.UseHttpsRedirection();


app.UseMiddleware<CorrelationIdMiddleware>();


app.UseRouting();


app.UseAuthentication();


app.UseAuthorization();



// ======================================
// Minimal API Endpoints
// ======================================


// GET ALL enrollments
app.MapGet("/api/enrollments",
async (IEnrollmentService service) =>
{
    var enrollments = await service.GetAllAsync();

    return Results.Ok(enrollments);
});




// GET enrollment by ID
app.MapGet("/api/enrollments/{id}",
async (
    string id,
    IEnrollmentService service) =>
{
    var enrollment = await service.GetByIdAsync(id);


    if (enrollment is null)
    {
        return Results.NotFound(new
        {
            message = "Enrollment not found"
        });
    }


    return Results.Ok(enrollment);
});




// CREATE enrollment
app.MapPost("/api/enrollments",
async (
    EnrollmentRequest request,
    IEnrollmentService service) =>
{

    var enrollment =
        await service.EnrollAsync(
            request.StudentId,
            request.CourseCode);



    return Results.Created(
        $"/api/enrollments/{enrollment.Id}",
        enrollment);
});




// DELETE enrollment
app.MapDelete("/api/enrollments/{id}",
async (
    string id,
    IEnrollmentService service) =>
{

    var deleted =
        await service.DeleteAsync(id);



    if (!deleted)
    {
        return Results.NotFound(new
        {
            message = "Enrollment not found"
        });
    }



    return Results.NoContent();
});




// Future MVC Controllers
app.MapControllers();



app.Run();





// ======================================
// Request DTO
// ======================================

public record EnrollmentRequest(
    string StudentId,
    string CourseCode);