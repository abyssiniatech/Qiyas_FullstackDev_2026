using Tms.Dtos;

namespace Tms.Api.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);


    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct);
}

