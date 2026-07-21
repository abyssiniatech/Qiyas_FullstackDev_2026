// using Microsoft.AspNetCore.Mvc;
// using Tms.Api.Dtos;
// using TmsApi.DTOs;
// using TmsApi.Services;

// namespace Tms.Api.Controllers;

// [ApiController]
// [Route("api/students")]
// [Tags("Students")]
// [Produces("application/json")]
// [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
// public class StudentsController : ControllerBase
// {
//     private readonly IStudentService studentService;
//     private readonly LinkGenerator linkGenerator;


//     public StudentsController(
//         IStudentService studentService,
//         LinkGenerator linkGenerator)
//     {
//         this.studentService = studentService;
//         this.linkGenerator = linkGenerator;
//     }



//     // GET api/students
//     [HttpGet]
//     [ProducesResponseType(typeof(IReadOnlyList<StudentDto>), StatusCodes.Status200OK)]
//     [EndpointSummary("List students")]
//     [EndpointDescription(
//         "Returns a paginated list of students.")]
//     public async Task<IActionResult> GetStudents(
//         [FromQuery] int page = 1,
//         [FromQuery] int pageSize = 10,
//         CancellationToken ct = default)
//     {

//         if (page <= 0)
//         {
//             return BadRequest("Page must be greater than zero.");
//         }


//         if (pageSize <= 0)
//         {
//             return BadRequest("PageSize must be greater than zero.");
//         }


//         var students =
//             await studentService.GetStudentsAsync(
//                 page,
//                 pageSize,
//                 ct);


//         return Ok(students);
//     }



//     // GET api/students/{id}
//     [HttpGet("{id:int}", Name = nameof(GetStudentById))]
//     [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [EndpointSummary("Get student by ID")]
//     [EndpointDescription(
//         "Returns a single student.")]
//     public async Task<IActionResult> GetStudentById(
//         int id,
//         CancellationToken ct)
//     {

//         var student =
//             await studentService.GetStudentByIdAsync(
//                 id,
//                 ct);


//         if (student is null)
//         {
//             return NotFound();
//         }


//         return Ok(student);
//     }



// }







using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using TmsApi.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class StudentsController(
    IStudentService studentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    private readonly IStudentService studentService = studentService;
    private readonly LinkGenerator linkGenerator = linkGenerator;
    private readonly object? entity;

    // GET ALL

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<StudentResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page <= 0)
            return BadRequest("Page must be greater than zero");

        if (pageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        var students = await studentService.GetStudentsAsync(
            page,
            pageSize,
            ct);

        return Ok(students);
    }

    // GET BY ID

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(
        int id,
        CancellationToken ct)
    {
        var student = await studentService.GetStudentByIdAsync(id, ct);

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    // CREATE

    [HttpPost]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        TmsApi.Services.StudentRequestDto request,
        CancellationToken ct)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        var student = await studentService.CreateStudentAsync(request, entity: entity, ct);
#pragma warning restore CS8604 // Possible null reference argument.

        return CreatedAtRoute(
            nameof(GetStudentById),
            new { id = ((dynamic)student).Id },
            student);
    }

    // UPDATE

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(StudentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        TmsApi.Services.StudentRequestDto request,
        CancellationToken ct)
    {
        await studentService.UpdateStudentAsync(
            id,
            request,
            ct);

        var student = await studentService.GetStudentByIdAsync(id, ct);

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    // DELETE

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var deleted = await studentService.DeleteStudentAsync(id, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}