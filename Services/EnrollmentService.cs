using Microsoft.EntityFrameworkCore;
using Tms.Dtos;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Exceptions;

namespace TmsApi.Services;


public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext context;


    public EnrollmentService(
        TmsDbContext context)
    {
        this.context = context;
    }



    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct)
    {

        // 1. Check course exists FIRST
        var courseExists =
            await context.Courses
                .AnyAsync(
                    c => c.Id == courseId,
                    ct);


        if (!courseExists)
        {
            throw new KeyNotFoundException(
                "Course not found");
        }



        if (!int.TryParse(request.StudentId, out var studentId))
        {
            throw new ArgumentException(
                "StudentId must be a valid integer.",
                nameof(request.StudentId));
        }


        // 2. Check duplicate enrollment
        var duplicate =
            await context.Enrollments
                .AnyAsync(
                    e =>
                    e.CourseId == courseId &&
                    e.StudentId == studentId,
                    ct);


        if (duplicate)
        {
            throw new ConflictException(
                "Student already enrolled in this course");
        }



        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId
        };


        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);



        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.StudentId.ToString()
        );
    }



    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId.ToString()
            ))
            .FirstOrDefaultAsync(ct);
    }
}