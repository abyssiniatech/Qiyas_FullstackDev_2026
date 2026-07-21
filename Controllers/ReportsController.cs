using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Persistence;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext db;


    public ReportsController(AppDbContext db)
    {
        this.db = db;
    }



    // GET api/reports/courses/5
    [HttpGet("courses/{courseId:int}")]
    public async Task<IActionResult> GetCourseReport(
        int courseId,
        CancellationToken ct)
    {
        var courseExists =
            await db.Courses
                .AnyAsync(
                    c => c.Id == courseId,
                    ct);


        if (!courseExists)
        {
            return NotFound(new
            {
                message = $"Course {courseId} not found"
            });
        }


        var enrollmentCount =
            await db.Enrollments
                .CountAsync(
                    e => e.CourseId == courseId,
                    ct);



        return Ok(new
        {
            courseId,
            enrollmentCount
        });
    }
}