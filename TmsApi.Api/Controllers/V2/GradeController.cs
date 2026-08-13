using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Grade.Commands.SubmitGrade;
using TmsApi.Application.Grade.Queries.GetGradeById;
using TmsApi.Application.Grade.Queries.GetGrades;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v2/grades")]
public sealed class GradesController : ControllerBase
{
    private readonly ISender _sender;

    public GradesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Submit a grade for a student.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitGrade(
        [FromBody] SubmitGradeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitGradeCommand(
            request.StudentId,
            request.CourseId,
            request.AssessmentType,
            request.Score
        );

        var result = await _sender.Send(
            command,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetGrade),
            new { id = result.Id },
            result
        );
    }

    /// <summary>
    /// Get all grades, optionally filtered by student or course.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrades(
        [FromQuery] int? studentId,
        [FromQuery] int? courseId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetGradesQuery(studentId, courseId),
            cancellationToken
        );

        return Ok(result);
    }

    /// <summary>
    /// Get a submitted grade by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGrade(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetGradeByIdQuery(id),
            cancellationToken
        );

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

/// <summary>
/// HTTP request body for submitting a grade.
/// </summary>
public sealed record SubmitGradeRequest(
    int StudentId,
    int CourseId,
    string AssessmentType,
    decimal Score
);