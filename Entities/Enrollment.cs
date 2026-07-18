using TmsApi.Entities;

public class Enrollment
{
    public DateTime EnrolledAt { get; set; }=DateTime.UtcNow;

    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public decimal Grade { get; set; }


    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}