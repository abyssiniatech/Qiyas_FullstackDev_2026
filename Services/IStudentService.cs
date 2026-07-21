// using Tms.Api.Dtos;
// using TmsApi.DTOs;

// namespace TmsApi.Services;

// public interface IStudentService
// {
//     Task<IReadOnlyList<StudentDto>> GetStudentsAsync(
//         int page,
//         int pageSize,
//         CancellationToken ct);


//     Task<StudentDto?> GetStudentByIdAsync(
//         int id,
//         CancellationToken ct);
// }


using Tms.Api.Dtos;

namespace TmsApi.Services;

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponseDto>> GetStudentsAsync(
        int page,
        int pageSize,
        CancellationToken ct);

    Task<StudentResponseDto?> GetStudentByIdAsync(
        int id,
        CancellationToken ct);

    Task<StudentResponseDto> CreateStudentAsync(
        StudentRequestDto request,
        CancellationToken ct);

    Task<StudentResponseDto?> UpdateStudentAsync(
        int id,
        StudentRequestDto request,
        CancellationToken ct);

    Task<bool> DeleteStudentAsync(
        int id,
        CancellationToken ct);
    Task UpdateStudentAsync(int id, Tms.Api.Dtos.StudentRequestDto request, CancellationToken ct);
    Task<dynamic> CreateStudentAsync(StudentRequestDto request, object entity, CancellationToken ct);
}

public class StudentRequestDto
{
    public string Name { get; internal set; }
    public object Email { get; internal set; }
}