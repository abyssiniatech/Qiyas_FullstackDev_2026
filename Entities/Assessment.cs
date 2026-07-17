namespace TmsApi.Entities;

public class Assessment
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal MaxScore { get; set; }
    public decimal weight { get; set; }
    public decimal CourseId { get; set; }
    public Course Course { get; set; } = null!;
}