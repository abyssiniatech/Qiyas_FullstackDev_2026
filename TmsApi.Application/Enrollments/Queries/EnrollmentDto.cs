namespace TmsApi.Application.Enrollments.Queries;


public class EnrollmentDto
{
    public string Id { get; set; } = "";

    public int StudentId { get; set; }

    public string StudentName { get; set; } = "";

    public int CourseId { get; set; }

    public string CourseName { get; set; } = "";

    public string Status { get; set; } = "";

    public string EnrolledAt { get; set; } = "";
}
