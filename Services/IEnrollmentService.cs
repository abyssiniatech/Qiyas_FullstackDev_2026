using Tms.Dtos;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct);


    Task<EnrollmentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);
}