namespace TmsApi.Application.Grade.Queries.GetGrades;

public sealed record GradeResponse(
    int Id,
    int StudentId,
    int CourseId,
    string AssessmentType,
    decimal Score
);