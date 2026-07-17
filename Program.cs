using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Middleware;


var builder = WebApplication.CreateBuilder(args);


// ==================================================
// SERVICES CONFIGURATION
// ==================================================


// Controllers
builder.Services.AddControllers();


// Database - PostgreSQL + EF Core
builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    );


    // Development SQL logging only
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(
            Console.WriteLine,
            LogLevel.Information
        );

        options.EnableSensitiveDataLogging();
    }
});


// OpenAPI
builder.Services.AddOpenApi();



var app = builder.Build();



// ==================================================
// HTTP REQUEST PIPELINE
// ==================================================


// Correlation ID Middleware
app.UseMiddleware<CorrelationIdMiddleware>();


// HTTPS Redirect
app.UseHttpsRedirection();


// OpenAPI JSON endpoint
app.MapOpenApi();


// Scalar API Documentation
app.MapScalarApiReference();


// Controllers
app.MapControllers();




// ==================================================
// DATABASE MIGRATION + SEED DATA
// ==================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    // Apply pending migrations
    context.Database.Migrate();



    // Seed database only when empty
    if (!context.Students.Any())
    {

        // ----------------------------
        // Seed Students
        // ----------------------------

        var students = new List<Student>
        {
            new()
            {
                Name = "Student One",
                RegistrationNumber = "TMS-2026-0001",
                GPA = 3.8m,
                IsActive = true
            },

            new()
            {
                Name = "Student Two",
                RegistrationNumber = "TMS-2026-0002",
                GPA = 2.9m,
                IsActive = true
            },

            new()
            {
                Name = "Student Three",
                RegistrationNumber = "TMS-2026-0003",
                GPA = 3.4m,
                IsActive = false
            },

            new()
            {
                Name = "Student Four",
                RegistrationNumber = "TMS-2026-0004",
                GPA = 3.9m,
                IsActive = true
            },

            new()
            {
                Name = "Student Five",
                RegistrationNumber = "TMS-2026-0005",
                GPA = 2.5m,
                IsActive = true
            }
        };


        context.Students.AddRange(students);


        // Save students first
        // PostgreSQL generates IDs here
        context.SaveChanges();



        // ----------------------------
        // Seed Courses
        // ----------------------------

        var courses = new List<Course>
        {
            new()
            {
                Code = "CS-101",
                Title = "Introduction to Computer Science",
                Capacity = 30
            },

            new()
            {
                Code = "CS-201",
                Title = "Data Structures and Algorithms",
                Capacity = 25
            },

            new()
            {
                Code = "MAT-101",
                Title = "Calculus I",
                Capacity = 40
            }
        };


        context.Courses.AddRange(courses);


        // Save courses first
        // PostgreSQL generates IDs here
        context.SaveChanges();



        // ----------------------------
        // Seed Enrollments
        // ----------------------------

        var enrollments = new List<Enrollment>
        {
            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[0].Id,
                Grade = 4.0m
            },

            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[1].Id,
                Grade = 3.6m
            },

            new()
            {
                StudentId = students[1].Id,
                CourseId = courses[0].Id,
                Grade = 2.8m
            },

            new()
            {
                StudentId = students[3].Id,
                CourseId = courses[1].Id,
                Grade = 3.9m
            }
        };


        context.Enrollments.AddRange(enrollments);


        // Save enrollments
        context.SaveChanges();
    }
}



// ==================================================
// APPLICATION START
// ==================================================

app.Run();