using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Domain.Entities;
using GradeEntity = TmsApi.Domain.Entities.Grade;

namespace TmsApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Course> Courses { get; }
    IQueryable<Student> Students { get; }
    IQueryable<Enrollment> Enrollments { get; }
    IQueryable<GradeEntity> Grades { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}