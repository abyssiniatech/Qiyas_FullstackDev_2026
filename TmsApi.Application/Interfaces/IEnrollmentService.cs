using Tms.Api.Dtos;
using Tms.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);


    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);


    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct);


    Task<EnrollmentResponseDto> UpdateAsync(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct);


    Task<EnrollmentResponseDto> PatchAsync(
        int courseId,
        int id,
        PatchEnrollmentRequest request,
        CancellationToken ct);


    Task DeleteAsync(
        int courseId,
        int id,
        CancellationToken ct);



    Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken cancellationToken);

    Task AddAsync(
        Enrollment enrollment,
        CancellationToken cancellationToken);


    Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);

}

public class PatchEnrollmentRequest
{
}
