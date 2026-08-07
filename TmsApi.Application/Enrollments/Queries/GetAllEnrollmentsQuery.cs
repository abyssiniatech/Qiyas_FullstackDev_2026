using MediatR;


namespace TmsApi.Application.Enrollments.Queries;


public record GetAllEnrollmentsQuery 
    : IRequest<List<EnrollmentDto>>;