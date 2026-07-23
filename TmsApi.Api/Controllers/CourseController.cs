using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CourseController : ControllerBase
{
    private readonly ICourseService courseService;

    public CourseController(
        ICourseService courseService)
    {
        this.courseService = courseService;
    }


    // GET api/courses?page=1&pageSize=10

    [HttpGet(Name = "ListCourses")]
    [ProducesResponseType(
        typeof(IReadOnlyList<CourseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page <= 0)
        {
            return BadRequest(
                "Page must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            return BadRequest(
                "PageSize must be greater than zero.");
        }


        var courses =
            await courseService.GetAllCoursesAsync(
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
    public async Task<IActionResult> CreateCourse(
        [FromBody] CourseDto request,
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
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] CourseDto request,
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
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct)
    {
        await courseService.DeleteCourseAsync(
            id,
            ct);


        return NoContent();
    }
}