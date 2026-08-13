using MediatR;

namespace TmsApi.Application.Grade.Queries.GetGrades;

public sealed record GetGradesQuery(
    int? StudentId = null,
    int? CourseId = null
) : IRequest<IReadOnlyList<GradeResponse>>;