using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly TmsDbContext context;

    public ReportsController(TmsDbContext context)
    {
        this.context = context;
    }

    // 1. Active students GPA >= 3.0
    [HttpGet("active-students-count")]
    public async Task<IActionResult> GetActiveStudentsCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new
        {
            ActiveStudentsCount = count
        });
    }

    // 2. Courses with enrollment count
    [HttpGet("courses-by-enrollment")]
    public async Task<IActionResult> CoursesByEnrollment()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // 3. Average GPA per course
    [HttpGet("average-gpa")]
    public async Task<IActionResult> AverageGpa()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // 4A. NOT EXISTS query
    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> StudentsWithoutEnrollments()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // 4B. LEFT JOIN query
    [HttpGet("students-without-enrollments-leftjoin")]
    public async Task<IActionResult> StudentsWithoutEnrollmentsLeftJoin()
    {
        var list = await (
            from student in context.Students
            join enrollment in context.Enrollments
                on student.Id equals enrollment.StudentId into enrollments
            from enrollment in enrollments.DefaultIfEmpty()
            where enrollment == null
            select student.Name
        ).ToListAsync();

        return Ok(list);
    }
}