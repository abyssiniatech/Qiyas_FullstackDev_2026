// 
namespace Tms.Api.Dtos;

public class StudentRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
    public string? Name { get; internal set; }
    public decimal GPA { get; set; }
}