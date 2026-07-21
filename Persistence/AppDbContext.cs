using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses { get; set; } = null!;

    public DbSet<Student> Students { get; set; } = null!;

    public DbSet<Enrollment> Enrollments { get; set; } = null!;
}