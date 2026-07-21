

// // using Tms.Api.Dtos;

// // namespace TmsApi.Services;

// // public interface ICourseService
// // {
// //     Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
// //         int page,
// //         int pageSize,
// //         CancellationToken ct);


// //     Task<CourseDto?> GetCourseByIdAsync(
// //         int id,
// //         CancellationToken ct);
// //     Task GetByIdAsync(int id, CancellationToken ct);
// //     Task GetCoursesAsync(PagedRequest request, CancellationToken ct);
// // }






// using Tms.Api.Dtos;

// namespace TmsApi.Services;


// public interface ICourseService
// {

//     Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
//         int page,
//         int pageSize,
//         CancellationToken ct);



//     Task<CourseDto?> GetCourseByIdAsync(
//         int id,
//         CancellationToken ct);

// }

// public class CourseDto
// {
//     public int MaxCapacity { get; internal set; }
//     public string Title { get; internal set; }
//     public string Code { get; internal set; }
//     public int Id { get; internal set; }
// }


using Tms.Api.Dtos;

namespace TmsApi.Services;

public interface ICourseService
{

    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken ct);


    Task<CourseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct);



    Task<CourseDto> CreateCourseAsync(
        CourseRequestDto request,
        CancellationToken ct);



    Task<CourseDto?> UpdateCourseAsync(
        int id,
        CourseRequestDto request,
        CancellationToken ct);



    Task<bool> DeleteCourseAsync(
        int id,
        CancellationToken ct);

}