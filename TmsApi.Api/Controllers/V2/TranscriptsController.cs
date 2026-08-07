using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript(
        [FromBody] TranscriptRequest request)
    {
        // Session 3 will replace this.
        // It will enqueue background work and return 202.

        return Ok(new
        {
            message = "Transcript generation started",
            studentId = request.StudentId
        });
    }
}


public record TranscriptRequest(int StudentId);