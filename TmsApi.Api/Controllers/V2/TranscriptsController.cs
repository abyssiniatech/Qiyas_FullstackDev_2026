
using System.Threading.Channels;

using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using TmsApi.Application.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore)
    : ControllerBase
{
    // =========================================================
    // POST: /api/v2/transcripts
    // Request a transcript
    // =========================================================

    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")]
        string? idempotencyKey,
        CancellationToken ct)
    {
        // =====================================================
        // 1. VALIDATE IDEMPOTENCY KEY
        // =====================================================

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Missing Idempotency-Key",
                    Detail =
                        "The Idempotency-Key header is required.",
                    Status =
                        StatusCodes.Status400BadRequest
                });
        }

        // =====================================================
        // 2. CHECK IDEMPOTENCY KEY
        // =====================================================

        var existingStatus =
            await statusStore.GetByIdempotencyKeyAsync(
                idempotencyKey,
                ct);

        if (existingStatus is not null)
        {
            Response.Headers.RetryAfter = "5";

            return Accepted(
                Url.Action(
                    nameof(GetStatus),
                    new
                    {
                        id = existingStatus.ReportId
                    }),
                existingStatus);
        }

        // =====================================================
        // 3. CREATE REPORT ID
        // =====================================================

        var reportId =
            Guid.NewGuid()
                .ToString("N")[..12];

        // =====================================================
        // 4. CREATE QUEUED STATUS
        // =====================================================

        var status =
            new TranscriptStatus(
                ReportId: reportId,
                StudentId: request.StudentId,
                State: TranscriptState.Queued,
                RequestedAt: DateTimeOffset.UtcNow,
                StartedAt: null,
                CompletedAt: null,
                DownloadUrl: null,
                ErrorMessage: null);

        // =====================================================
        // 5. SAVE STATUS
        // =====================================================

        await statusStore.CreateAsync(
            idempotencyKey,
            status,
            ct);

        // =====================================================
        // 6. QUEUE BACKGROUND WORK
        // =====================================================

        await channel.Writer.WriteAsync(
            request.WithReportId(reportId),
            ct);

        // =====================================================
        // 7. TELL CLIENT WHEN TO POLL
        // =====================================================

        Response.Headers.RetryAfter = "5";

        // =====================================================
        // 8. RETURN 202 ACCEPTED
        // =====================================================

        return Accepted(
            Url.Action(
                nameof(GetStatus),
                new
                {
                    id = reportId
                }),
            status);
    }

    // =========================================================
    // GET: /api/v2/transcripts/{id}/status
    // Get transcript status
    // =========================================================

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken ct)
    {
        var status =
            await statusStore.GetAsync(
                id,
                ct);

        if (status is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Transcript not found",

                    Detail =
                        $"No transcript request with id '{id}'.",

                    Status =
                        StatusCodes.Status404NotFound
                });
        }

        return Ok(status);
    }
}

