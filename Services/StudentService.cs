using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Persistence;

namespace TmsApi.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext context;

    public StudentService(AppDbContext context)
    {
        this.context = context;
    }


    public async Task<IReadOnlyList<StudentResponseDto>> GetStudentsAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email
            })
            .ToListAsync(ct);
    }


    public async Task<StudentResponseDto?> GetStudentByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Students
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email
            })
            .FirstOrDefaultAsync(ct);
    }


    public async Task<StudentResponseDto> CreateStudentAsync(
        StudentRequestDto request,
        CancellationToken ct)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        var student = new Entities.Student
        {
            Name = request.Name,
            Email = request.Email
        };
#pragma warning restore CS8601 // Possible null reference assignment.

        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email
        };
    }


    public async Task<StudentResponseDto?> UpdateStudentAsync(
        int id,
        StudentRequestDto request,
        CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);

        if (student is null)
        {
            return null;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        student.Name = request.Name;
#pragma warning restore CS8601 // Possible null reference assignment.
        student.Email = request.Email;

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email
        };
    }


    public async Task<bool> DeleteStudentAsync(
        int id,
        CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);

        if (student is null)
        {
            return false;
        }

        context.Students.Remove(student);

        await context.SaveChangesAsync(ct);

        return true;
    }
}