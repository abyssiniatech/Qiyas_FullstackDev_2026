using Tms.Api.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    Task<CourseDto> GetCourseAsync(
        string code,
        CancellationToken ct);

    Task<List<CourseDto>> GetAllCoursesAsync(
        CancellationToken ct);

    Task<IReadOnlyList<CourseResponseDto>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}