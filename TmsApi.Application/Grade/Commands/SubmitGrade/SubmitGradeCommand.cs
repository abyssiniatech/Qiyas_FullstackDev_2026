using MediatR;

namespace TmsApi.Application.Grade.Commands.SubmitGrade;

public sealed class SubmitGradeCommandHandler
    : IRequestHandler<SubmitGradeCommand, GradeResult>
{
    public SubmitGradeCommandHandler()
    {
    }

    public async Task<GradeResult> Handle(SubmitGradeCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}