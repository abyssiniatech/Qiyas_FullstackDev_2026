using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService service,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{

    public async Task<CourseDto> GetCourseAsync(
        string code,
        CancellationToken ct)
    {
        var key = CacheKeys.Course(code);

        var dbHit = false;

        var dto = await cache.GetOrCreateAsync(
            key,
            (service, code),
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching from database.",
                    key);


                var course = await state.service.GetByCodeAsync(
                    state.code,
                    token);


                if (course is null)
                {
                    throw new KeyNotFoundException(
                        $"Course '{state.code}' was not found.");
                }


                return new CourseDto
                {
                    Id = course.Id,
                    Code = course.Code,
                    Title = course.Title,
                    Description = course.Description,
                    MaxCapacity = course.MaxCapacity,
                    EnrollmentCount = course.Enrollments.Count
                };
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return dto;
    }



    public async Task<IReadOnlyList<CourseResponseDto>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var key = CacheKeys.Courses(page, pageSize);

        var dbHit = false;


        var courses = await cache.GetOrCreateAsync(
            key,
            (service, page, pageSize),
            async (state, token) =>
            {
                dbHit = true;


                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching courses.",
                    key);


                var result =
                    await state.service.GetAllCoursesAsync(
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
                        EnrollmentCount = course.EnrollmentCount
                    })
                    .ToList();
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return courses;
    }



    public async Task<List<CourseDto>> GetAllCoursesAsync(
        CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;

        var dbHit = false;


        var courses = await cache.GetOrCreateAsync(
            key,
            service,
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key}. Fetching all courses.",
                    key);


                var result =
                    await state.GetAllCoursesAsync(
                        page: 1,
                        pageSize: 100,
                        ct: token);


                return result.ToList();
            },
            tags:
            [
                CacheKeys.CoursesTag
            ],
            cancellationToken: ct);



        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }


        return courses;
    }



    public async Task InvalidateCourseCacheAsync(
        CancellationToken ct)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);



        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            ct);
    }

    public Task<IReadOnlyList<CourseResponseDto>> GetCoursesAsync(object page, object pageSize, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}