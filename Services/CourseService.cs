// using Microsoft.EntityFrameworkCore;
// using TmsApi.Dtos;
// using TmsApi.Persistence;

// namespace TmsApi.Services;


// public class CourseService : ICourseService
// {
//     private readonly AppDbContext context;


//     public CourseService(AppDbContext context)
//     {
//         this.context = context;
//     }



//     public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
//         int page,
//         int pageSize,
//         CancellationToken ct)
//     {
//         return await context.Courses
//             .AsNoTracking()
//             .OrderBy(c => c.Id)
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(c => new CourseDto
//             {
//                 Id = c.Id,
//                 Code = c.Code,
//                 Title = c.Title,
//                 MaxCapacity = c.MaxCapacity
//             })
//             .ToListAsync(ct);
//     }



//     public async Task<CourseDto?> GetCourseByIdAsync(
//         int id,
//         CancellationToken ct)
//     {
//         return await context.Courses
//             .AsNoTracking()
//             .Where(c => c.Id == id)
//             .Select(c => new CourseDto
//             {
//                 Id = c.Id,
//                 Code = c.Code,
//                 Title = c.Title,
//                 MaxCapacity = c.MaxCapacity
//             })
//             .FirstOrDefaultAsync(ct);
//     }

// }


using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Entities;
using TmsApi.Persistence;


namespace TmsApi.Services;


public class CourseService : ICourseService
{

    private readonly AppDbContext context;


    public CourseService(AppDbContext context)
    {
        this.context = context;
    }



    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {

        return await context.Courses
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxCapacity = c.MaxCapacity
            })
            .ToListAsync(ct);

    }





    public async Task<CourseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct)
    {

        return await context.Courses
            .Where(c => c.Id == id)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                MaxCapacity = c.MaxCapacity
            })
            .FirstOrDefaultAsync(ct);

    }





    public async Task<CourseDto> CreateCourseAsync(
        CourseRequestDto request,
        CancellationToken ct)
    {

#pragma warning disable CS8601 // Possible null reference assignment.
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            MaxCapacity = request.MaxCapacity
        };
#pragma warning restore CS8601 // Possible null reference assignment.


        context.Courses.Add(course);


        await context.SaveChangesAsync(ct);



        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            MaxCapacity = course.MaxCapacity
        };

    }






    public async Task<CourseDto?> UpdateCourseAsync(
        int id,
        CourseRequestDto request,
        CancellationToken ct)
    {

        var course =
            await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);


        if(course is null)
        {
            return null;
        }



        course.Title = request.Title;
#pragma warning disable CS8601 // Possible null reference assignment.
        course.Description = request.Description;
#pragma warning restore CS8601 // Possible null reference assignment.
        course.MaxCapacity = request.MaxCapacity;



        await context.SaveChangesAsync(ct);



        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            MaxCapacity = course.MaxCapacity
        };

    }







    public async Task<bool> DeleteCourseAsync(
        int id,
        CancellationToken ct)
    {

        var course =
            await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);


        if(course is null)
        {
            return false;
        }


        context.Courses.Remove(course);


        await context.SaveChangesAsync(ct);


        return true;

    }

}