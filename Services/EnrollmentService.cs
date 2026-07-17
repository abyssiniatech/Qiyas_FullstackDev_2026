using TmsApi.Models;
using Scalar.AspNetCore;
namespace TmsApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly List<EnrollmentRecord> _enrollments = new();


    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<EnrollmentRecord>>(_enrollments);
    }


    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        var enrollment = _enrollments
            .FirstOrDefault(x => x.Id == id);

        return Task.FromResult(enrollment);
    }


    public Task<EnrollmentRecord> EnrollAsync(
        string studentId,
        string courseCode)
    {
        var enrollment = new EnrollmentRecord(
            Guid.NewGuid().ToString(),
            studentId,
            courseCode,
            System.DateTime.UtcNow);

        _enrollments.Add(enrollment);

        return Task.FromResult(enrollment);
    }


    public Task<bool> DeleteAsync(string id)
    {
        var enrollment = _enrollments
            .FirstOrDefault(x => x.Id == id);

        if (enrollment == null)
            return Task.FromResult(false);

        _enrollments.Remove(enrollment);

        return Task.FromResult(true);
    }

    public Task EnrollAsync(object studentId, object courseCode)
    {
        throw new NotImplementedException();
    }
}


public class TmsDatabaseException(string message) : Exception(message);