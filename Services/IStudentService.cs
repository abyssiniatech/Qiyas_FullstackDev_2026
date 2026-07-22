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
}

public class StudentResponseDto
{
    public int Id { get; internal set; }
    public string? Name { get; internal set; }
    public string? Email { get; internal set; }
}