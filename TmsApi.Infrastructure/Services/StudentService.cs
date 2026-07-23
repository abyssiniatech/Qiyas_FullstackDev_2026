
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext context;

    public StudentService(AppDbContext context)
    {
        this.context = context;
    }


    private Microsoft.EntityFrameworkCore.DbSet<Student> Students => context.Students;


    public async Task<IReadOnlyList<StudentResponseDto>> GetStudentsAsync(
        CancellationToken ct)
    {
        return await Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
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



    public async Task<StudentResponseDto?> GetByIdAsync(
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



    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        if (request.Name is null)
            throw new ArgumentNullException(nameof(request.Name));

        if (request.Email is null)
            throw new ArgumentNullException(nameof(request.Email));

        var student = new Student
        {
            Name = request.Name,
            Email = request.Email,
            GPA = (decimal)request.GPA,
            IsActive = true
        };


        context.Students.Add(student);

        await context.SaveChangesAsync(ct);


        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            GPA = (double)student.GPA,
            IsActive = student.IsActive
        };
    }



    public async Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct)
    {
        var student = await Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);


        if (student is null)
        {
            return null;
        }

        if (request.Name is null)
            throw new ArgumentNullException(nameof(request.Name));

        if (request.Email is null)
            throw new ArgumentNullException(nameof(request.Email));

        student.Name = request.Name;
        student.Email = request.Email;
        student.GPA = (decimal)request.GPA;
        student.IsActive = request.IsActive;

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            GPA = (double)student.GPA,
            IsActive = student.IsActive
        };
    }



    public async Task<StudentResponseDto?> PatchAsync(
        int id,
        PatchStudentRequest request,
        CancellationToken ct)
    {
        var student = await Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);


        if (student is null)
        {
            return null;
        }


        if (request.Name != null)
            student.Name = request.Name;


        if (request.Email != null)
            student.Email = request.Email;


        if (request.GPA.HasValue)
            student.GPA = (decimal)request.GPA.Value;


        if (request.IsActive.HasValue)
            student.IsActive = request.IsActive.Value;


        await context.SaveChangesAsync(ct);


        return new StudentResponseDto
        {
            Id = student.Id,
            Name = student.Name,
            Email = student.Email,
            GPA = (double)student.GPA,
            IsActive = student.IsActive
        };
    }



    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        var student = await Students
            .FirstOrDefaultAsync(
                s => s.Id == id,
                ct);


        if (student is null)
        {
            return false;
        }


        Students.Remove(student);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public Task<IReadOnlyList<StudentResponseDto>> GetStudentsAsync(int page, int pageSize, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<StudentResponseDto?> GetStudentByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<StudentResponseDto> CreateStudentAsync(StudentRequestDto request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<StudentResponseDto?> UpdateStudentAsync(int id, StudentRequestDto request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteStudentAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

}

public interface IStudentService
{
}

// Fallback DTO definitions to satisfy compilation when application DTOs
// are not available. These are minimal and mirror the properties used
// by this service. If the real DTOs exist in TmsApi.Application.DTOs,
// these will not be used by that namespace.

public class StudentResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public double GPA { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStudentRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public double GPA { get; set; }
}

public class UpdateStudentRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public double GPA { get; set; }
    public bool IsActive { get; set; }
}

public class PatchStudentRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public double? GPA { get; set; }
    public bool? IsActive { get; set; }
}

public class StudentRequestDto : CreateStudentRequest { }

internal class DbSet<T>
{
}