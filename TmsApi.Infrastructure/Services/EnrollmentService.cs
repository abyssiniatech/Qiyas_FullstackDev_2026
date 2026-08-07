using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using Tms.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext context;

    public EnrollmentService(AppDbContext context)
    {
        this.context = context;
    }

    public Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto> UpdateAsync(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentResponseDto> PatchAsync(
        int courseId,
        int id,
        PatchEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken cancellationToken)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .AnyAsync(
                e => e.StudentId == studentId &&
                     e.Course.Code == courseCode,
                cancellationToken);
    }

  public async Task AddAsync(
    Enrollment enrollment,
    CancellationToken cancellationToken)
{
    await context.Enrollments.AddAsync(enrollment, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
}

    public Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}