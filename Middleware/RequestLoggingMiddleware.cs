using System.Diagnostics;

namespace TmsApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine(
            $"Incoming Request: {context.Request.Method} {context.Request.Path}"
        );

        await _next(context);

        stopwatch.Stop();

        Console.WriteLine(
            $"Response Status: {context.Response.StatusCode}, Time: {stopwatch.ElapsedMilliseconds}ms"
        );
    }
}