using Microsoft.EntityFrameworkCore;
using TmsApi.Persistence;

namespace TmsApi.Services;

public class NPlusOneDemoService
{
    private readonly AppDbContext db;

    public NPlusOneDemoService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task RunNPlusOneProblem(
        CancellationToken cancellationToken = default)
    {
        var students = await db.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            // This creates N additional SQL queries (N+1 problem)
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(
                    e => e.StudentId == s.Id,
                    cancellationToken);

            Console.WriteLine($"{s.Name}: {count} enrollments");
        }
    }
}