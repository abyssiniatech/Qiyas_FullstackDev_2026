namespace TmsApi.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public required int StudentId { get; set; }


    // Navigation properties
    public Course Course { get; set; } = null!;

    public Student Student { get; set; } = null!;
}