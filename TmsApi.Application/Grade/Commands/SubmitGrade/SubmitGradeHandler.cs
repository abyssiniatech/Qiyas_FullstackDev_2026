using MediatR;

namespace TmsApi.Application.Grade.Commands.SubmitGrade;

public sealed record SubmitGradeCommand(
    int StudentId,
    int CourseId,
    string AssessmentType,
    decimal Score
) : IRequest<GradeResult>;