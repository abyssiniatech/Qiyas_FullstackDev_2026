namespace TmsApi.Domain.Entities;

public class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string AssessmentType { get; set; } = string.Empty;

    public decimal Score { get; set; }
}