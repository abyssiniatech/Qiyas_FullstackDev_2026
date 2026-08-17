

namespace TmsApi.Application.Dtos;

public sealed record GradeReportDto(
    int GradeId,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string AssessmentType,
    decimal Score
);


