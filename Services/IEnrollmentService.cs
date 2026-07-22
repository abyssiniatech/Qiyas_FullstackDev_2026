using Tms.Api.Controllers;
using Tms.Api.Dtos;

namespace Tms.Api.Services;

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
        EnrollStudentRequest request,
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
}