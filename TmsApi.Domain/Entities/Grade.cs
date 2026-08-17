namespace TmsApi.Domain.Entities;

public sealed class Grade
{
    public int GradeId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string AssessmentType { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}