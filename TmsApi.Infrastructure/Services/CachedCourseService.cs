using Tms.Api.Dtos;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;
using TmsApi.Application;
namespace TmsApi.Infrastructure.Services;

public sealed class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{
    public async Task<CourseDto> GetCourseAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var key = CacheKeys.Course(code);

        var cacheMiss = false;

        var course = await cache.GetOrCreateAsync(
            key,
            (courseService, code),
            async (state, token) =>
            {
                cacheMiss = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching from database.",
                    key);

                var result =
                    await state.courseService.GetByCodeAsync(
                        state.code,
                        token);

                if (result is null)
                {
                    throw new KeyNotFoundException(
                        $"Course '{state.code}' was not found.");
                }

                return new CourseDto
                {
                    Id = result.Id,
                    Code = result.Code,
                    Title = result.Title,
                    Description = result.Description,
                    MaxCapacity = result.MaxCapacity,
                    EnrollmentCount =
                        result.Enrollments.Count
                };
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: cancellationToken);

        if (!cacheMiss)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return course;
    }

    public async Task<IReadOnlyList<CourseResponseDto>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var key = CacheKeys.Courses(page, pageSize);

        var cacheMiss = false;

        var courses = await cache.GetOrCreateAsync(
            key,
            (courseService, page, pageSize),
            async (state, token) =>
            {
                cacheMiss = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching courses.",
                    key);

                var result =
                    await state.courseService.GetAllCoursesAsync(
                        state.page,
                        state.pageSize,
                        token);

                return result
                    .Select(course => new CourseResponseDto
                    {
                        Id = course.Id,
                        Code = course.Code,
                        Title = course.Title,
                        Description = course.Description,
                        MaxCapacity = course.MaxCapacity,
                        EnrollmentCount =
                            course.EnrollmentCount
                    })
                    .ToList();
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: cancellationToken);

        if (!cacheMiss)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return courses;
    }

    public async Task<List<CourseDto>> GetAllCoursesAsync(
        CancellationToken cancellationToken)
    {
        var key = CacheKeys.CoursesAll;

        var cacheMiss = false;

        var courses = await cache.GetOrCreateAsync(
            key,
            courseService,
            async (service, token) =>
            {
                cacheMiss = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching all courses.",
                    key);

                var result =
                    await service.GetAllCoursesAsync(
                        page: 1,
                        pageSize: 100,
                        token);

                return result.ToList();
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: cancellationToken);

        if (!cacheMiss)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return courses;
    }

    public async Task InvalidateCourseCacheAsync(
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);

        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            cancellationToken);
    }

    Task<List<CourseDto>> ICachedCourseService.GetAllCoursesAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    Task<IReadOnlyList<CourseResponseDto>> ICachedCourseService.GetCoursesAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}



