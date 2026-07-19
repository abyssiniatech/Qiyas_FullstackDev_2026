using Microsoft.EntityFrameworkCore;
using Tms.Dtos;
using TmsApi.Data;
using TmsApi.Entities;

namespace Tms.Api.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext context;
    private readonly ILogger<CourseService> logger;


    public CourseService(
        TmsDbContext context,
        ILogger<CourseService> logger)
    {
        this.context = context;
        this.logger = logger;
    }



    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count
            ))
            .FirstOrDefaultAsync(ct);
    }



    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };


        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);


        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);


        return (await GetByIdAsync(course.Id, ct))!;
    }
}

