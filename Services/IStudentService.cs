using System.Collections.Generic;
using System.Threading;
using TmsApi.DTOs;

namespace TmsApi.Services
{
    public interface IStudentService
    {
        Task<List<StudentEnrollmentReportDto>> GetEnrollmentReportAsync(
            CancellationToken cancellationToken);
    }
}