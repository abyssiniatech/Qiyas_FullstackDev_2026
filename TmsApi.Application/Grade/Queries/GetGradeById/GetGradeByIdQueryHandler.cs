using MediatR;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Grade.Queries.GetGrades;

namespace TmsApi.Application.Grade.Queries.GetGradeById;

public sealed class GetGradeByIdQueryHandler
    : IRequestHandler<GetGradeByIdQuery, GradeResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetGradeByIdQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GradeResponse?> Handle(
        GetGradeByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(
            _context.Grades
                .Where(grade => grade.Id == request.Id)
                .Select(grade => new GradeResponse(
                    grade.Id,
                    grade.StudentId,
                    grade.CourseId,
                    grade.AssessmentType,
                    grade.Score
                ))
                .FirstOrDefault());
    }
}