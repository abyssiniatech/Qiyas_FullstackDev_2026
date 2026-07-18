using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Services;

public class NPlusOneDemoService
{
    private readonly TmsDbContext db;

    public NPlusOneDemoService(TmsDbContext db)
    {
        this.db = db;
    }


    public async Task RunNPlusOneProblem(
        CancellationToken cancellationToken)
    {
        var students = await db.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);


        foreach (var s in students)
        {
            // This creates N extra SQL queries
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(
                    e => e.StudentId == s.Id,
                    cancellationToken);


            Console.WriteLine(
                $"{s.Name}: {count} enrollments");
        }
    }
}