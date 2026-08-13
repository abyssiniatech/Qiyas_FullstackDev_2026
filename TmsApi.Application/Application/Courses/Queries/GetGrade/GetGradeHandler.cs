using MediatR;
using TmsApi.Application.Grade.Commands.SubmitGrade;

namespace TmsApi.Application.Grade.Queries.GetGrade;

public sealed class GetGradeHandler
    : IRequestHandler<GetGradeQuery, GradeResult>
{
    public async Task<GradeResult> Handle(
        GetGradeQuery request,
        CancellationToken cancellationToken)
    {
        // Get grade from database

        throw new NotImplementedException();
    }
}