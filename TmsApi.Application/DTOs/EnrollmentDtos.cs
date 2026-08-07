

public class UpdateEnrollmentRequest
{
    public DateOnly EnrollmentDate { get; set; }

    public decimal? Grade { get; set; }

    public bool IsActive { get; set; }
}