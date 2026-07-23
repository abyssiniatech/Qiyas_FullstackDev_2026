

using Tms.Api.Dtos;

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
}

