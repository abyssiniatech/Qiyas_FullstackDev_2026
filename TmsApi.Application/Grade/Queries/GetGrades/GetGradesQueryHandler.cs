
using MediatR;
using TmsApi.Application.Common.Interfaces;

namespace TmsApi.Application.Grade.Queries.GetGrades;

public sealed class GetGradesQueryHandler
    : IRequestHandler<GetGradesQuery, IReadOnlyList<GradeResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetGradesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradeResponse>> Handle(
        GetGradesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Grades
            .AsQueryable();

        if (request.StudentId.HasValue)
        {
            query = query.Where(
                grade => grade.StudentId == request.StudentId.Value);
        }

        if (request.CourseId.HasValue)
        {
            query = query.Where(
                grade => grade.CourseId == request.CourseId.Value);
        }

        return await Task.FromResult(
            query
                .OrderByDescending(grade => grade.Id)
                .Select(grade => new GradeResponse(
                    grade.Id,
                    grade.StudentId,
                    grade.CourseId,
                    grade.AssessmentType,
                    grade.Score
                ))
                .ToList() as IReadOnlyList<GradeResponse>);
    }
}