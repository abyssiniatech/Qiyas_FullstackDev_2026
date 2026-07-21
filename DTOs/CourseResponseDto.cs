
using System.ComponentModel.DataAnnotations;

namespace Tms.Api.Dtos;

public class CourseRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;


    [MaxLength(500)]
    public string? Description { get; set; }


    [Range(1, 1000)]
    public int MaxCapacity { get; set; }
}