
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
    private readonly LinkGenerator linkGenerator;



    public CoursesController(
        ICourseService courseService,
        LinkGenerator linkGenerator)
    {
        this.courseService = courseService;
        this.linkGenerator = linkGenerator;
    }

    // GET ALL

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CourseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses(
        [FromQuery]int page = 1,
        [FromQuery]int pageSize = 10,
        CancellationToken ct = default)
    {

        if(page <=0)
            return BadRequest("Page must be greater than zero");


        if(pageSize <=0)
            return BadRequest("PageSize must be greater than zero");



        var courses =
            await courseService.GetCoursesAsync(
                page,
                pageSize,
                ct);



        return Ok(courses);

    }






    // GET BY ID

    [HttpGet("{id:int}",
        Name=nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {

        var course =
            await courseService
            .GetCourseByIdAsync(id,ct);



        if(course is null)
            return NotFound();



        return Ok(course);

    }







    // CREATE

    [HttpPost]
    [ProducesResponseType(
        typeof(CourseDto),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CourseRequestDto request,
        CancellationToken ct)
    {


        var course =
            await courseService
            .CreateCourseAsync(request,ct);



        return CreatedAtRoute(
            nameof(GetCourseById),
            new {id = course.Id},
            course);

    }







    // UPDATE

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        CourseRequestDto request,
        CancellationToken ct)
    {

        var course =
            await courseService
            .UpdateCourseAsync(
                id,
                request,
                ct);



        if(course is null)
            return NotFound();



        return Ok(course);

    }







    // DELETE

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {


        var deleted =
            await courseService
            .DeleteCourseAsync(id,ct);



        if(!deleted)
            return NotFound();



        return NoContent();

    }


}