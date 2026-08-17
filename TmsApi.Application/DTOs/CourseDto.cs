namespace Tms.Api.Dtos;

public class CourseDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }

    public int EnrollmentCount { get; set; }
}
