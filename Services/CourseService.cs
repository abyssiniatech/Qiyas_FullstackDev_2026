using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using Tms.Dtos;
using Tms.Api.Dtos;
using TmsApi.Data;
using TmsApi.Services;

namespace Tms.Api.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext context;


    public CourseService(TmsDbContext context)
    {
        this.context = context;
    }



    // =====================================
    // GET COURSE BY ID
    // =====================================

    public async Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c =>
                new CourseResponseDto(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.MaxCapacity,
                    c.Enrollments.Count
                ))
            .FirstOrDefaultAsync(ct);
    }




    // =====================================
    // CREATE COURSE
    // =====================================

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };


        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);



        return new CourseResponseDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity,
            0
        );
    }





    // =====================================
    // CHECK COURSE CODE EXISTS
    // =====================================

    public async Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        return await context.Courses
            .AnyAsync(
                c => c.Code == code,
                ct);
    }





    // =====================================
    // PAGINATION + FILTERING + SORTING
    // =====================================

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {


        // 1. Start IQueryable without tracking

        IQueryable<Course> query =
            context.Courses
            .AsNoTracking();



        // 2. Search filter

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(
                    c.Title,
                    $"%{request.Search}%")
                ||
                EF.Functions.ILike(
                    c.Code,
                    $"%{request.Search}%"));
        }





        // 3. Count BEFORE Skip/Take

        var totalCount =
            await query.CountAsync(ct);






        // 4. Sorting

        query = request.OrderBy?.ToLower()
        switch
        {

            "code" =>
                request.Descending
                ?
                query.OrderByDescending(c => c.Code)
                :
                query.OrderBy(c => c.Code),



            "maxcapacity" =>
                request.Descending
                ?
                query.OrderByDescending(c => c.MaxCapacity)
                :
                query.OrderBy(c => c.MaxCapacity),



            _ =>
                request.Descending
                ?
                query.OrderByDescending(c => c.Title)
                :
                query.OrderBy(c => c.Title)

        };







        // 5. Paging + Projection

        var items =
            await query

            .Skip(
                (request.Page - 1)
                * request.PageSize)

            .Take(request.PageSize)


            .Select(c =>
                new CourseResponseDto(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.MaxCapacity,
                    c.Enrollments.Count
                ))

            .ToListAsync(ct);







        // 6. Return paged response

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,

            TotalCount = totalCount,

            Page = request.Page,

            PageSize = request.PageSize
        };
    }

    public Task GetAsync(PagedRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}