
using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;
[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class StudentsController(
    IStudentService studentService) : ControllerBase
{
    private readonly IStudentService studentService = studentService;

    // GET api/students


    [HttpGet(Name = "ListStudents")]
    [ProducesResponseType(
        typeof(IReadOnlyList<StudentResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [EndpointSummary("List students")]
    [EndpointDescription(
        "Returns a paginated list of students.")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page <= 0)
        {
            return BadRequest("Page must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            return BadRequest("PageSize must be greater than zero.");
        }

        var students =
            await studentService.GetStudentsAsync(
                page,
                pageSize,
                ct);

        return Ok(students);
    }

    // GET api/students/{id}

    [HttpGet("{id:int}", Name = nameof(GetStudent))]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student")]
    [EndpointDescription(
        "Returns a student by its unique identifier.")]
    public async Task<IActionResult> GetStudent(
        int id,
        CancellationToken ct)
    {
        var student =
            await studentService.GetStudentByIdAsync(
                id,
                ct);

        if (student is null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    // POST api/students

    [HttpPost]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a student")]
    [EndpointDescription(
        "Creates a new student.")]
    public async Task<IActionResult> CreateStudent(
        [FromBody] StudentRequestDto request,
        CancellationToken ct)
    {
        var student =
            await studentService.CreateStudentAsync(
                request,
                ct);

        return CreatedAtAction(
            nameof(GetStudent),
            new
            {
                id = student.Id
            },
            student);
    }

    // PUT api/students/{id}

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a student")]
    [EndpointDescription(
        "Updates an existing student.")]
    public async Task<IActionResult> UpdateStudent(
        int id,
        [FromBody] StudentRequestDto request,
        CancellationToken ct)
    {
        var student =
            await studentService.UpdateStudentAsync(
                id,
                request,
                ct);

        if (student is null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    // DELETE api/students/{id}

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription(
        "Deletes a student by its unique identifier.")]
    public async Task<IActionResult> DeleteStudent(
        int id,
        CancellationToken ct)
    {
        var deleted =
            await studentService.DeleteStudentAsync(
                id,
                ct);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

