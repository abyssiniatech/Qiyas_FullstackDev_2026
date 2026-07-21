using Microsoft.EntityFrameworkCore;
using TmsApi.DTOs;
using TmsApi.Persistence;

namespace TmsApi.Services;

public class DashboardService
{
    private readonly AppDbContext context;

    public DashboardService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<List<StudentDto>> GetStudentsPagedAsync(
        int pageNumber,
        CancellationToken cancellationToken)
    {
        const int pageSize = 20;

        return await context.Students
            .OrderBy(s => EF.Property<string>(s, "Name"))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto
            {
                Id = EF.Property<int>(s, "Id"),
                Name = EF.Property<string>(s, "Name") ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CourseEnrollmentSummaryDto>> GetTopCoursesAsync(
        CancellationToken cancellationToken)
    {
        return await context.Courses
            .Select(c => new CourseEnrollmentSummaryDto
            {
                CourseTitle = c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);
    }
}

public class CourseEnrollmentSummaryDto
{
    public string CourseTitle { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}