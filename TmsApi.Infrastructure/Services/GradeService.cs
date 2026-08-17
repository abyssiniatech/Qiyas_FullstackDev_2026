using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public sealed class GradeService : IGradeService
{
    private readonly AppDbContext _context;

    public GradeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GradeReportDto> CreateAsync(
        int studentId,
        int courseId,
        string assessmentType,
        decimal score,
        CancellationToken cancellationToken = default)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Score must be between 0 and 100.");
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == studentId,
                cancellationToken);

        if (student is null)
        {
            throw new KeyNotFoundException(
                $"Student with ID {studentId} was not found.");
        }

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == courseId,
                cancellationToken);

        if (course is null)
        {
            throw new KeyNotFoundException(
                $"Course with ID {courseId} was not found.");
        }

        var grade = new Grade
        {
            StudentId = studentId,
            CourseId = courseId,
            AssessmentType = assessmentType,
            Score = score
        };

        _context.Grades.Add(grade);

        await _context.SaveChangesAsync(cancellationToken);

        return new GradeReportDto(
            grade.GradeId,
            studentId,
            student.Name,
            courseId,
            course.Title,
            assessmentType,
            score);
    }

    public async Task<IReadOnlyList<GradeReportDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .AsNoTracking()
            .Select(grade => new GradeReportDto(
                grade.GradeId,
                grade.StudentId,
                grade.Student.Name,
                grade.CourseId,
                grade.Course.Title,
                grade.AssessmentType,
                grade.Score))
            .ToListAsync(cancellationToken);
    }

    public async Task<GradeReportDto?> GetByIdAsync(
        int gradeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .AsNoTracking()
            .Where(grade => grade.GradeId == gradeId)
            .Select(grade => new GradeReportDto(
                grade.GradeId,
                grade.StudentId,
                grade.Student.Name,
                grade.CourseId,
                grade.Course.Title,
                grade.AssessmentType,
                grade.Score))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int gradeId,
        CancellationToken cancellationToken = default)
    {
        var grade = await _context.Grades
            .FirstOrDefaultAsync(
                x => x.GradeId == gradeId,
                cancellationToken);

        if (grade is null)
        {
            return false;
        }

        _context.Grades.Remove(grade);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}