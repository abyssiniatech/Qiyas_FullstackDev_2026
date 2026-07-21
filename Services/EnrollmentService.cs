using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Entities;
using TmsApi.Persistence;

namespace Tms.Api.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext context;


    public EnrollmentService(AppDbContext context)
    {
        this.context = context 
            ?? throw new ArgumentNullException(nameof(context));
    }



    public async Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrolledAt = e.EnrolledAt
            })
            .ToListAsync(ct);
    }



    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.CourseId == courseId &&
                e.Id == id)
            .Select(e => new EnrollmentResponseDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrolledAt = e.EnrolledAt
            })
            .FirstOrDefaultAsync(ct);
    }



    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Verify course exists
        var courseExists =
            await context.Courses
                .AnyAsync(
                    c => c.Id == courseId,
                    ct);


        if (!courseExists)
        {
            throw new KeyNotFoundException(
                $"Course with id {courseId} was not found.");
        }



        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };


        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);



        return new EnrollmentResponseDto
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            StudentId = enrollment.StudentId,
            EnrolledAt = enrollment.EnrolledAt
        };
    }
}