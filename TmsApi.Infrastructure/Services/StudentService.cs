using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;

    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    private DbSet<Student> Students => _context.Students;


    public async Task<IReadOnlyList<StudentResponseDto>> GetStudentsAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                GPA = (double)s.GPA,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);
    }


    public async Task<StudentResponseDto?> GetStudentByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                GPA = (double)s.GPA,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }


    public async Task<StudentResponseDto> CreateStudentAsync(
        StudentRequestDto request,
        CancellationToken ct)
    {
        var student = new Student
        {
            Name = request.Name!,
            Email = request.Email!,
            GPA = (decimal)request.GPA,
            IsActive = true
        };


        Students.Add(student);

        await _context.SaveChangesAsync(ct);


        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            GPA = (double)student.GPA,
            IsActive = student.IsActive
        };
    }


    public async Task<StudentResponseDto?> UpdateStudentAsync(
        int id,
        StudentRequestDto request,
        CancellationToken ct)
    {
        var student = await Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);


        if (student is null)
            return null;


        student.Name = request.Name!;
        student.Email = request.Email!;
        student.GPA = (decimal)request.GPA;


        await _context.SaveChangesAsync(ct);


        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            GPA = (double)student.GPA,
            IsActive = student.IsActive
        };
    }


    public async Task<bool> DeleteStudentAsync(
        int id,
        CancellationToken ct)
    {
        var student = await Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);


        if (student is null)
            return false;


        Students.Remove(student);

        await _context.SaveChangesAsync(ct);


        return true;
    }
}