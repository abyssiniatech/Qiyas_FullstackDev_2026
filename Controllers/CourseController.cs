
using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using Tms.Api.Services;
using Tms.Dtos;
using TmsApi.DTOs;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }


    // GET: api/courses/{id}
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(id, ct);

        return course is not null
            ? Ok(course)
            : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses(
     [FromQuery] PagedRequest request,
     CancellationToken ct)
    {
        var result = await _courseService.GetCoursesAsync(request, ct);

        return Ok(result);
    }

    // POST: api/courses
    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var result = await _courseService.CreateAsync(request, ct);


        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }
}


