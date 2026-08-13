using MediatR;
using TmsApi.Application.Grade.Queries.GetGrades;
namespace TmsApi.Application.Grade.Queries.GetGradeById;

public sealed record GetGradeByIdQuery(
    int Id
) : IRequest<GradeResponse?>;