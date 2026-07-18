using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Middleware;


// =============================
// BUILDER
// =============================

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
// DATABASE MIGRATION + SEED
// =============================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();


    // Apply EF Core migrations automatically
    await context.Database.MigrateAsync();


    // Insert initial data
    await SeedDatabase(context);
}



// =============================
// MIDDLEWARE PIPELINE
// =============================

app.UseMiddleware<CorrelationIdMiddleware>();


app.UseHttpsRedirection();


app.MapOpenApi();

app.MapScalarApiReference();


app.MapControllers();



// =============================
// RUN APPLICATION
// =============================

app.Run();





// =============================
// DATABASE SEED METHOD
// =============================

static async Task SeedDatabase(TmsDbContext context)
{

    // =============================
    // STUDENTS
    // =============================

    if (!await context.Students.AnyAsync())
    {

        var students = new List<Student>
        {
            new()
            {
                Name = "Surafel Mengist",
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
                Name = "Henoke Ketema",
                RegistrationNumber = "TMS-2026-0003",
                GPA = 3.4m,
                IsActive = false
            }
        };


        await context.Students.AddRangeAsync(students);

        await context.SaveChangesAsync();
    }



    // =============================
    // COURSES
    // =============================

    if (!await context.Courses.AnyAsync())
    {

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
            }
        };


        await context.Courses.AddRangeAsync(courses);

        await context.SaveChangesAsync();

    }




    // =============================
    // ENROLLMENTS
    // =============================

    if (!await context.Enrollments.AnyAsync())
    {

        var student1 = await context.Students.FirstAsync();

        var student2 = await context.Students
            .Skip(1)
            .FirstAsync();


        var course1 = await context.Courses.FirstAsync();

        var course2 = await context.Courses
            .Skip(1)
            .FirstAsync();



        var enrollments = new List<Enrollment>
        {
            new()
            {
                StudentId = student1.Id,
                CourseId = course1.Id,
                Grade = 4.0m
            },


            new()
            {
                StudentId = student2.Id,
                CourseId = course2.Id,
                Grade = 3.5m
            }
        };


        await context.Enrollments.AddRangeAsync(enrollments);

        await context.SaveChangesAsync();

    }

}