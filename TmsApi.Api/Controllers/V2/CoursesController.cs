using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;


namespace TmsApi.Api.Controllers.V2;


[ApiController]
[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
[Route("api/v{version:apiVersion}/courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext context;


    public CoursesController(AppDbContext context)
    {
        this.context = context;
    }



    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {

        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);



        var baseQuery = context.Courses
            .AsNoTracking();



        var totalCount = await baseQuery
            .CountAsync(ct);



        var rows = await baseQuery

            .OrderBy(c => c.Title)

            .Skip((page - 1) * pageSize)

            .Take(pageSize)

            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.MaxCapacity,

                EnrollmentCount = c.Enrollments.Count
            })

            .ToListAsync(ct);



        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );


        return Ok(new
        {
            data = rows,

            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext = page < totalPages,
                hasPrevious = page > 1
            },

            links = new
            {
                self =
                $"/api/v2/courses?page={page}&pageSize={pageSize}",

                next = page < totalPages
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : null,

                prev = page > 1
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : null,

                enroll = "/api/v2/enrollments"
            }
        });
    }
}