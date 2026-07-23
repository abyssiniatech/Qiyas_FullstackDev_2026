// using Tms.Api.Dtos;
// using Tms.Dtos;


// namespace TmsApi.Infrastructure.Services;

// public class EnrollmentService : IEnrollmentService
// {
//     public Task<List<EnrollmentResponseDto>> GetByCourseAsync(
//         int courseId,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }


//     public Task<EnrollmentResponseDto?> GetByIdAsync(
//         int courseId,
//         int id,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }


//     public Task<EnrollmentResponseDto> CreateAsync(
//         int courseId,
//         CreateEnrollmentRequest request,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }


//     public Task<EnrollmentResponseDto> UpdateAsync(
//         int courseId,
//         int id,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }


//     public Task<EnrollmentResponseDto> PatchAsync(
//         int courseId,
//         int id,
//         PatchEnrollmentRequest request,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }


//     public Task DeleteAsync(
//         int courseId,
//         int id,
//         CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }

//     public Task<EnrollmentResponseDto> UpdateAsync(int courseId, int id, UpdateEnrollmentRequest request, CancellationToken ct)
//     {
//         throw new NotImplementedException();
//     }
// }


// {
// }public interface IEnrollmentService
// {
// }

// public class PatchEnrollmentRequest
// {
// }







using Tms.Api.Dtos;
using Tms.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    public Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    public Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        CreateEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    public Task<EnrollmentResponseDto> UpdateAsync(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    public Task<EnrollmentResponseDto> PatchAsync(
        int courseId,
        int id,
        PatchEnrollmentRequest request,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    public Task DeleteAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}