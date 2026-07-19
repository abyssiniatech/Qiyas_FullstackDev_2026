using Microsoft.AspNetCore.Mvc;
using Tms.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;


[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(
    IEnrollmentService enrollmentService)
    : ControllerBase
{


    [HttpPost]
    public async Task<IActionResult> Create(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct)
    {

        var result =
            await enrollmentService.CreateAsync(
                courseId,
                request,
                ct);


        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }



    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result =
            await enrollmentService.GetByIdAsync(
                id,
                ct);


        return result is null
            ? NotFound()
            : Ok(result);
    }
}