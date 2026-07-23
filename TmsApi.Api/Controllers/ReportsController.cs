using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

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
        var report = await db.Courses
            .Where(c => c.Id == courseId)
            .Select(c => new
            {
                CourseId = c.Id,
                CourseCode = c.Code,
                CourseTitle = c.Title,
                MaxCapacity = c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count(),
                AvailableSeats = c.MaxCapacity - c.Enrollments.Count()
            })
            .FirstOrDefaultAsync(ct);

        if (report == null)
        {
            return NotFound(new
            {
                message = $"Course {courseId} not found"
            });
        }

        return Ok(report);
    }
}