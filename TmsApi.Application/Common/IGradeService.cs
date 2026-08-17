using TmsApi.Application.Dtos;

namespace TmsApi.Application.Common;

public interface IGradeService
{
    Task<GradeReportDto> CreateAsync(
        int studentId,
        int courseId,
        string assessmentType,
        decimal score,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GradeReportDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<GradeReportDto?> GetByIdAsync(
        int gradeId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int gradeId,
        CancellationToken cancellationToken = default);
}
