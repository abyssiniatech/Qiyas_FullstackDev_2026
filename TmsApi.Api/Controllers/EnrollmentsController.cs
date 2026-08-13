
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Api.Hubs;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
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
        var result = await mediator.Send(
            command,
            ct);

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
    public async Task<IActionResult> Approve(
        string id,
        CancellationToken ct)
    {
        // --------------------------------------------------
        // Your existing approval logic goes here.
        // The database approval must succeed BEFORE
        // broadcasting the SignalR event.
        // --------------------------------------------------

        // TODO:
        // Replace this with your actual approval command/service.
        //
        // Example:
        // var result = await mediator.Send(
        //     new ApproveEnrollmentCommand(id),
        //     ct);


        // --------------------------------------------------
        // Broadcast enrollment status update
        // --------------------------------------------------

        await hubContext.Clients.All
            .ReceiveEnrollmentStatusUpdated(
                id,
                "Approved");


        return NoContent();
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

