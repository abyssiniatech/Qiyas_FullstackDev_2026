using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/grades")]
public sealed class GradesController(AppDbContext db) : ControllerBase
{
    // ============================================
    // POST: api/v2/grades
    // ============================================

    [HttpPost]
    public async Task<IActionResult> SubmitGrade(
        [FromBody] SubmitGradeRequest request,
        CancellationToken cancellationToken)
    {
        // Validate score
        if (request.Score < 0 || request.Score > 100)
        {
            return BadRequest(new
            {
                message = "Score must be between 0 and 100."
            });
        }

        // Validate assessment type
        if (string.IsNullOrWhiteSpace(request.AssessmentType))
        {
            return BadRequest(new
            {
                message = "AssessmentType is required."
            });
        }

        // Check student
        var studentExists = await db.Students
            .AnyAsync(
                s => s.Id == request.StudentId,
                cancellationToken);

        if (!studentExists)
        {
            return NotFound(new
            {
                message = $"Student with ID {request.StudentId} was not found."
            });
        }

        // Check course
        var courseExists = await db.Courses
            .AnyAsync(
                c => c.Id == request.CourseId,
                cancellationToken);

        if (!courseExists)
        {
            return NotFound(new
            {
                message = $"Course with ID {request.CourseId} was not found."
            });
        }

        // Create grade
        var grade = new Grade
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            AssessmentType = request.AssessmentType.Trim(),
            Score = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        db.Grades.Add(grade);

        await db.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/v2/grades/{grade.GradeId}",
            new GradeResponse(
                grade.GradeId,
                grade.StudentId,
                grade.CourseId,
                grade.AssessmentType,
                grade.Score,
                grade.CreatedAt));
    }


    // ============================================
    // GET: api/v2/grades/{id}
    // ============================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGrade(
        int id,
        CancellationToken cancellationToken)
    {
        var grade = await db.Grades
            .AsNoTracking()
            .Where(g => g.GradeId == id)
            .Select(g => new GradeResponse(
                g.GradeId,
                g.StudentId,
                g.CourseId,
                g.AssessmentType,
                g.Score,
                g.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (grade is null)
        {
            return NotFound(new
            {
                message = $"Grade with ID {id} was not found."
            });
        }

        return Ok(grade);
    }


    // ============================================
    // GET: api/v2/grades
    // ============================================

    [HttpGet]
    public async Task<IActionResult> GetGrades(
        CancellationToken cancellationToken)
    {
        var grades = await db.Grades
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GradeResponse(
                g.GradeId,
                g.StudentId,
                g.CourseId,
                g.AssessmentType,
                g.Score,
                g.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(grades);
    }
}


// ============================================
// Request DTO
// ============================================

public sealed record SubmitGradeRequest(
    int StudentId,
    int CourseId,
    string AssessmentType,
    decimal Score);


// ============================================
// Response DTO
// ============================================

public sealed record GradeResponse(
    int GradeId,
    int StudentId,
    int CourseId,
    string AssessmentType,
    decimal Score,
    DateTime CreatedAt);