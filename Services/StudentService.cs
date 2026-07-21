// using Microsoft.EntityFrameworkCore;
// using Tms.Api.Dtos;
// using TmsApi.DTOs;
// using TmsApi.Persistence;

// namespace TmsApi.Services;

// public class StudentService : IStudentService
// {
//     private readonly AppDbContext context;


//     public StudentService(AppDbContext context)
//     {
//         this.context = context;
//     }



//     public async Task<IReadOnlyList<StudentDto>> GetStudentsAsync(
//         int page,
//         int pageSize,
//         CancellationToken ct)
//     {

//         return await context.Students
//             .OrderBy(s => s.Name)
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(s => new StudentDto
//             {
//                 Id = s.Id,
//                 Name = s.Name,
//                 Email = s.Email
//             })
//             .ToListAsync(ct);
//     }



//     public async Task<StudentDto?> GetStudentByIdAsync(
//         int id,
//         CancellationToken ct)
//     {

//         return await context.Students
//             .Where(s => s.Id == id)
//             .Select(s => new StudentDto
//             {
//                 Id = s.Id,
//                 Name = s.Name,
//                 Email = s.Email
//             })
//             .FirstOrDefaultAsync(ct);
//     }

// }




using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Controllers;
using TmsApi.DTOs;
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
        var entity = new Entities.Student
        {
            Name = request.Name,
            Email = (string)request.Email
        };

        var entityEntry = context.Students.Add(entity);
        int v = await context.SaveChangesAsync(ct);

        return new StudentResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email
        };
    }

    public async Task<StudentResponseDto?> UpdateStudentAsync(
        int id,
        StudentRequestDto request,
        CancellationToken ct)
    {
        var existing = await context.Students.FindAsync(new object[] { id }, ct);
        if (existing == null)
        {
            return null;
        }

        existing.Name = request.Name;
        existing.Email = (string)request.Email;

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto
        {
            Id = existing.Id,
            Name = existing.Name,
            Email = existing.Email
        };
    }

    public async Task<bool> DeleteStudentAsync(
        int id,
        CancellationToken ct)
    {
        var existing = await context.Students.FindAsync(new object[] { id }, ct);
        if (existing == null)
        {
            return false;
        }

        context.Students.Remove(existing);
        await context.SaveChangesAsync(ct);

        return true;
    }

    public Task UpdateStudentAsync(int id, Tms.Api.Dtos.StudentRequestDto request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<dynamic> CreateStudentAsync(StudentRequestDto request, object entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
