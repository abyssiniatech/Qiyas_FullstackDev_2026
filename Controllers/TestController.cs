using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Persistence;
namespace TmsApi.Controllers;

// Minimal Student model to satisfy compilation when the Student type
// is not available from other project files. Adjust or remove if a
// proper Student model exists elsewhere in the project.
internal class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal GPA { get; set; }
    public object Email { get; internal set; }
}

[ApiController]
[Route("api/test")]
public class TestController(AppDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Set<Student>().Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggered here
        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Set<Student>()
                .Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }
}