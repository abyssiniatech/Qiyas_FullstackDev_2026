using MediatR;
using TmsApi.Application.Grade.Commands.SubmitGrade;

namespace TmsApi.Application.Grade.Queries.GetGrade;

public sealed record GetGradeQuery(
    int Id
) : IRequest<GradeResult>;