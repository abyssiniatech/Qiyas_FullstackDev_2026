using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{


    // Database Tables
    public DbSet<Student> Students { get; set; } = null!;

    public DbSet<Course> Courses { get; set; } = null!;

    public DbSet<Enrollment> Enrollments { get; set; } = null!;

    public DbSet<Assessment> Assessments { get; set; } = null!;

    public DbSet<Certificate> Certificates { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TmsDbContext).Assembly
        );
    }
}