// namespace Tms.Api.Dtos;

// public record CourseDetailDto
// {
//     public required int Id { get; init; }

//     public required string Code { get; init; }

//     public required string Title { get; init; }

//     public required int MaxCapacity { get; init; }

//     public required int EnrollmentCount { get; init; }

//     public required IReadOnlyList<LinkDto> Links { get; init; }
//     public object Description { get; internal set; }
// }




namespace TmsApi.Dtos;

public class CourseDetailDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }

    public List<LinkDto> Links { get; set; } = new();
}