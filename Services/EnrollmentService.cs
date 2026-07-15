using Microsoft.Extensions.Logging;
using TmsApi.Models;

namespace TmsApi.Services;


public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();

    private readonly ILogger<EnrollmentService> _logger;


    public EnrollmentService(
    ILogger<EnrollmentService> logger)
{
    _logger = logger;


    // Seed sample data
    var enrollment1 = new EnrollmentRecord(
        "1001",
        "ST001",
        "CS101",
        DateTime.UtcNow);


    var enrollment2 = new EnrollmentRecord(
        "1002",
        "ST002",
        "CS102",
        DateTime.UtcNow);


    var enrollment3 = new EnrollmentRecord(
        "1003",
        "ST003",
        "CS103",
        DateTime.UtcNow);



    _store[enrollment1.Id] = enrollment1;
    _store[enrollment2.Id] = enrollment2;
    _store[enrollment3.Id] = enrollment3;
}



    public Task<EnrollmentRecord> EnrollAsync(
        string studentId,
        string courseCode)
    {

        var id = Guid.NewGuid()
            .ToString("N")[..8];


        var record = new EnrollmentRecord(
            id,
            studentId,
            courseCode,
            DateTime.UtcNow);


        _store[id] = record;


        _logger.LogInformation(
            "Student {StudentId} enrolled in {CourseCode} with ID {EnrollmentId}",
            studentId,
            courseCode,
            id);


        return Task.FromResult(record);
    }



    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);

        return Task.FromResult(record);
    }



    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> result =
            _store.Values.ToList();


        return Task.FromResult(result);
    }



    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);

        return Task.FromResult(removed);
    }

    public Task<object?> EnrollAsync(object studentId, object courseCode)
    {
        throw new NotImplementedException();
    }
}