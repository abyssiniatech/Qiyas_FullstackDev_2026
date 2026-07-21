// namespace Tms.Api.Dtos;

// public record PagedResponse<T>
// {
//     public required IReadOnlyList<T> Items { get; init; }

//     public required int TotalCount { get; init; }

//     public required int Page { get; init; }

//     public required int PageSize { get; init; }


//     public int TotalPages =>
//         (int)Math.Ceiling(
//             TotalCount / (double)PageSize);


//     public bool HasNext =>
//         Page < TotalPages;


//     public bool HasPrevious =>
//         Page > 1;
// }
namespace Tms.Api.Dtos;

public class StudentResponseDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
    public string? Name { get; internal set; }
}