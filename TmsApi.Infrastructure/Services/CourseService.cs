using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext context;

    public CourseService(AppDbContext context)
    {
        this.context = context;
    }


    public async Task<IReadOnlyList<CourseDto>> GetAllCoursesAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxCapacity = c.MaxCapacity
            })
            .ToListAsync(ct);
    }



    public async Task<CourseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxCapacity = c.MaxCapacity
            })
            .FirstOrDefaultAsync(ct);
    }



    public async Task<CourseDto> CreateCourseAsync(
        CourseDto request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Title = request.Title!,
            Description = request.Description!,
            MaxCapacity = request.MaxCapacity
        };


        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);


        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            MaxCapacity = course.MaxCapacity
        };
    }



    public async Task<CourseDto?> UpdateCourseAsync(
        int id,
        CourseDto request,
        CancellationToken ct)
    {
        var course =
            await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);


        if (course is null)
        {
            return null;
        }


        course.Title = request.Title!;
        course.Description = request.Description!;
        course.MaxCapacity = request.MaxCapacity;


        await context.SaveChangesAsync(ct);


        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            MaxCapacity = course.MaxCapacity
        };
    }



    public async Task DeleteCourseAsync(
        int id,
        CancellationToken ct)
    {
        var course =
            await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);


        if (course is null)
        {
            return;
        }


        context.Courses.Remove(course);

        await context.SaveChangesAsync(ct);
    }
    public async Task<Course?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Code == code,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken cancellationToken)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .AnyAsync(
                e =>
                    e.StudentId == studentId &&
                    e.Course.Code == courseCode,
                cancellationToken);
    }
}