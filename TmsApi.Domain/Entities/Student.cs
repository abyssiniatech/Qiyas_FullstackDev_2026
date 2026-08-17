namespace TmsApi.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal GPA { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();

    public ICollection<Grade> Grades { get; set; }
        = new List<Grade>();
}