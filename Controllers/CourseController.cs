using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using TmsApi.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController : ControllerBase
{
    private readonly ICourseService courseService;

    public CoursesController(
        ICourseService courseService)
    {
        this.courseService = courseService;
    }

    // GET api/courses

    [HttpGet(Name = "ListCourses")]
    [ProducesResponseType(
        typeof(IReadOnlyList<CourseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [EndpointSummary("List all courses")]
    [EndpointDescription(
        "Returns a paginated list of available courses.")]
    public async Task<IActionResult> GetCourses(
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

        var courses =
            await courseService.GetCoursesAsync(
                page,
                pageSize,
                ct);

        return Ok(courses);
    }

    // GET api/courses/{id}

    [HttpGet("{id:int}", Name = nameof(GetCourse))]
    [ProducesResponseType(
        typeof(CourseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course")]
    [EndpointDescription(
        "Returns a single course by its unique identifier.")]
    public async Task<IActionResult> GetCourse(
        int id,
        CancellationToken ct)
    {
        var course =
            await courseService.GetCourseByIdAsync(
                id,
                ct);

        if (course is null)
        {
            return NotFound();
        }

        return Ok(course);
    }

    // POST api/courses

    [HttpPost]
    [ProducesResponseType(
        typeof(CourseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a course")]
    [EndpointDescription(
        "Creates a new course and returns the created resource.")]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CourseRequestDto request,
        CancellationToken ct)
    {
        var course =
            await courseService.CreateCourseAsync(
                request,
                ct);

        return CreatedAtAction(
            nameof(GetCourse),
            new
            {
                id = course.Id
            },
            course);
    }

    // PUT api/courses/{id}

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(CourseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Update a course")]
    [EndpointDescription(
        "Updates all editable information for an existing course.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] CourseRequestDto request,
        CancellationToken ct)
    {
        var course =
            await courseService.UpdateCourseAsync(
                id,
                request,
                ct);

        if (course is null)
        {
            return NotFound();
        }

        return Ok(course);
    }

    // DELETE api/courses/{id}

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription(
        "Deletes a course using its unique identifier.")]
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct)
    {
        var deleted =
            await courseService.DeleteCourseAsync(
                id,
                ct);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}