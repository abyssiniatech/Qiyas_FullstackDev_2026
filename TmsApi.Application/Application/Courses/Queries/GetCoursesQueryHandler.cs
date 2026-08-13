using MediatR;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQuery, IReadOnlyList<CourseResponseDto>>
{
    private readonly ICachedCourseService _cachedCourseService;

    public GetCoursesQueryHandler(
        ICachedCourseService cachedCourseService)
    {
        _cachedCourseService = cachedCourseService;
    }

    public async Task<IReadOnlyList<CourseResponseDto>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        return await _cachedCourseService.GetCoursesAsync(
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}

public class GetCoursesQuery : IRequest<IReadOnlyList<CourseResponseDto>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}