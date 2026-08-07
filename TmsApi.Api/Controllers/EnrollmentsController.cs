using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;


[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{

    // GET: api/v2/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var enrollments = await mediator.Send(
            new GetAllEnrollmentsQuery(),
            ct);

        return Ok(enrollments);
    }



    // POST: api/v2/enrollments
    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {

        var result = await mediator.Send(command, ct);


        return result.Match(

            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created),


            onFailure: error =>
            {

                var status = error.Code switch
                {
                    "course_not_found" =>
                        StatusCodes.Status404NotFound,

                    "course_full" or "already_enrolled" =>
                        StatusCodes.Status409Conflict,

                    _ =>
                        StatusCodes.Status400BadRequest
                };


                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type:
                    $"https://tms.local/errors/{error.Code}"
                );

            });

    }



    // POST: api/v2/enrollments/{id}/approve
    [HttpPost("{id}/approve")]
    public IActionResult Approve(
        string id)
    {

        return Ok(new
        {
            id,
            status = "Approved"
        });

    }



    // GET: api/v2/enrollments/{studentId}/schedule
    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {

        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);


        return Ok(schedule);

    }

}