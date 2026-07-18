using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Middleware;


var builder = WebApplication.CreateBuilder(args);


// =============================
// SERVICES
// =============================

builder.Services.AddControllers();


// PostgreSQL + EF Core
builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")
    );


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


// =============================
// MIDDLEWARE PIPELINE
// =============================

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();


// API documentation
app.MapOpenApi();

app.MapScalarApiReference();


app.MapControllers();



// =============================
// DATABASE INITIALIZATION
// =============================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    // Apply migrations
    context.Database.Migrate();


    // Seed only empty database
    SeedDatabase(context);
}



// =============================
// START APPLICATION
// =============================

app.Run();





// =============================
// SEED METHOD
// =============================

static void SeedDatabase(TmsDbContext context)
{

    if (context.Students.Any())
    {
        return;
    }



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
        }
    };


    context.Students.AddRange(students);

    context.SaveChanges();



    var courses = new List<Course>
    {
        new()
        {
            Code="CS-101",
            Title="Introduction to Computer Science",
            Capacity=30
        },

        new()
        {
            Code="CS-201",
            Title="Data Structures and Algorithms",
            Capacity=25
        }
    };


    context.Courses.AddRange(courses);

    context.SaveChanges();



    var enrollments = new List<Enrollment>
    {
        new()
        {
            StudentId=students[0].Id,
            CourseId=courses[0].Id,
            Grade=4.0m
        },

        new()
        {
            StudentId=students[1].Id,
            CourseId=courses[1].Id,
            Grade=3.5m
        }
    };


    context.Enrollments.AddRange(enrollments);

    context.SaveChanges();
}