using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using Tms.Api.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService enrollmentService;


    public EnrollmentsController(
        IEnrollmentService enrollmentService)
    {
        this.enrollmentService = enrollmentService;
    }



    // GET api/courses/{courseId}/enrollments

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
        typeof(IReadOnlyList<EnrollmentResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrollments for a course")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        var enrollments =
            await enrollmentService.GetByCourseAsync(
                courseId,
                ct);

        return Ok(enrollments);
    }



    // GET api/courses/{courseId}/enrollments/{id}

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrollment for a course")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment =
            await enrollmentService.GetByIdAsync(
                courseId,
                id,
                ct);


        if (enrollment is null)
        {
            return NotFound();
        }


        return Ok(enrollment);
    }



    // POST api/courses/{courseId}/enrollments

    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student in a course")]
    [EndpointDescription(
        "Creates a new enrollment. Returns 404 if the course does not exist and 409 if enrollment is not allowed.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        [FromBody] EnrollStudentRequest request,
        CancellationToken ct)
    {

        var result =
            await enrollmentService.CreateAsync(
                courseId,
                request,
                ct);


        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId = courseId,
                id = result.Id
            },
            result);
    }
}