


using Microsoft.Extensions.Logging;
using TmsApi.Models;

namespace TmsApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly List<EnrollmentRecord> _enrollments = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    // Get all enrollments
    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        _logger.LogInformation(
            "Retrieving all enrollments. Count: {Count}",
            _enrollments.Count);

        return Task.FromResult<IReadOnlyList<EnrollmentRecord>>(_enrollments);
    }

    // Get enrollment by ID
    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment is null)
        {
            _logger.LogWarning(
                "Enrollment not found. EnrollmentId: {EnrollmentId}",
                id);
        }
        else
        {
            _logger.LogInformation(
                "Enrollment retrieved successfully. EnrollmentId: {EnrollmentId}",
                id);
        }

        return Task.FromResult(enrollment);
    }

    // Create a new enrollment
    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        var enrollment = new EnrollmentRecord(
            Guid.NewGuid().ToString(),
            studentId,
            courseCode,
            DateTime.UtcNow);

        _enrollments.Add(enrollment);

        _logger.LogInformation(
            "Student enrolled successfully. EnrollmentId: {EnrollmentId}, StudentId: {StudentId}, CourseCode: {CourseCode}",
            enrollment.Id,
            studentId,
            courseCode);

        return Task.FromResult(enrollment);
    }

    public Task<EnrollmentRecord> EnrollAsync(object studentId, object courseCode)
    {
        if (studentId is null)
        {
            throw new ArgumentNullException(nameof(studentId));
        }

        if (courseCode is null)
        {
            throw new ArgumentNullException(nameof(courseCode));
        }

        return EnrollAsync(studentId.ToString()!, courseCode.ToString()!);
    }

    // Delete an enrollment
    public Task<bool> DeleteAsync(string id)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment is null)
        {
            _logger.LogWarning(
                "Delete failed. Enrollment not found. EnrollmentId: {EnrollmentId}",
                id);

            return Task.FromResult(false);
        }

        _enrollments.Remove(enrollment);

        _logger.LogInformation(
            "Enrollment deleted successfully. EnrollmentId: {EnrollmentId}",
            id);

        return Task.FromResult(true);
    }
}

// Custom exception
public class TmsDatabaseException : Exception
{
    public TmsDatabaseException(string message)
        : base(message)
    {
    }
}