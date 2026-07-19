using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data
{
    public class TmsDbContext : DbContext
    {
        public TmsDbContext(DbContextOptions<TmsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
}