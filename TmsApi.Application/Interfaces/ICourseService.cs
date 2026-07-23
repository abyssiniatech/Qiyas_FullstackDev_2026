using Tms.Api.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> GetAllCoursesAsync(
        int page,
        int pageSize,
        CancellationToken ct);


    Task<CourseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct);


    Task<CourseDto> CreateCourseAsync(
        CourseDto request,
        CancellationToken ct);


    Task<CourseDto?> UpdateCourseAsync(
        int id,
        CourseDto request,
        CancellationToken ct);


    Task DeleteCourseAsync(
        int id,
        CancellationToken ct);
}