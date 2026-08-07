using MediatR;


namespace TmsApi.Application.Enrollments.Queries;


public class GetAllEnrollmentsQueryHandler
    : IRequestHandler<GetAllEnrollmentsQuery, List<EnrollmentDto>>
{

    public Task<List<EnrollmentDto>> Handle(
        GetAllEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {

        var data = new List<EnrollmentDto>
        {
            new EnrollmentDto
            {
                Id = "1",
                StudentId = 1001,
                StudentName = "Liya",
                CourseId = 1,
                CourseName = "Angular",
                Status = "Pending",
                EnrolledAt = DateTime.UtcNow.ToString()
            }
        };


        return Task.FromResult(data);
    }
}