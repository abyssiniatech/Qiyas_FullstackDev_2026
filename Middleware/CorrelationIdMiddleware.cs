namespace TmsApi.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        // Check if client already sent correlation id
        var correlationId = context.Request.Headers["X-Correlation-Id"]
            .FirstOrDefault();


        // Create new one if missing
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }


        // Add to response header
        context.Response.Headers["X-Correlation-Id"] = correlationId;


        // Continue pipeline
        await _next(context);
    }
}