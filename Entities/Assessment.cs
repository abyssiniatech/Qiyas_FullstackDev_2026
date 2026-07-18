using TmsApi.Entities;

public class Assessment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;
}