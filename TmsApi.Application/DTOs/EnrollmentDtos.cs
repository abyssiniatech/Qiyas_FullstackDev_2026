// namespace Tms.Api.Dtos;




// public class EnrollStudentRequest
// {
//     public int StudentId { get; set; }
// }


public class UpdateEnrollmentRequest
{
    public DateOnly EnrollmentDate { get; set; }

    public decimal? Grade { get; set; }

    public bool IsActive { get; set; }
}