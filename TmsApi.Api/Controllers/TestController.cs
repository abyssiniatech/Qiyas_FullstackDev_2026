using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
[Tags("Test")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class TestController : ControllerBase
{
    private readonly AppDbContext context;

    public TestController(AppDbContext context)
    {
        this.context = context;
    }

    // GET api/test/deferred

    [HttpGet("deferred")]
    [ProducesResponseType(
        typeof(IReadOnlyList<Student>),
        StatusCodes.Status200OK)]
    [EndpointSummary("Demonstrate deferred execution")]
    [EndpointDescription(
        "Shows that LINQ queries are not executed until they are materialized.")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building query (no SQL executed yet)...");

        var query =
            context.Students
                .Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Adding OrderBy...");

        var orderedQuery =
            query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Calling ToList()...");

        var students =
            orderedQuery.ToList();

        Console.WriteLine(">>> STEP 4: Query executed.\n");

        return Ok(students);
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    // GET api/test/translation-fail

    [HttpGet("translation-fail")]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [EndpointSummary("Demonstrate EF Core translation failure")]
    [EndpointDescription(
        "Shows what happens when EF Core cannot translate a C# method into SQL.")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Executing non-translatable query...");

        try
        {
            var students =
                context.Students
                    .Where(s => IsHonorRoll(s.GPA))
                    .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION: {ex.Message}\n");

            return BadRequest(new ProblemDetails
            {
                Title = "EF Core Translation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}