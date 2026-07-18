using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;
using TmsApi.Models;

public class StudentService
{
    private readonly TmsDbContext _context;


    public StudentService(TmsDbContext context)
    {
        _context = context;
    }


    public async Task<List<Student>> GetStudentsPagedAsync(
        int pageNumber)
    {
        int pageSize = 20;

        return await _context.Students
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }


    // Module 5 Exercise 7: N+1 Fix with Projection
    public async Task<List<StudentEnrollmentReportDto>> GetEnrollmentReportAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Students
            .AsNoTracking()
            .Select(s => new StudentEnrollmentReportDto
            {
                Name = s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);
    }

    
}