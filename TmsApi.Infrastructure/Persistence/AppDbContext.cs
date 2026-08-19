
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Persistence;

public sealed class AppDbContext :IdentityDbContext<TmsUser> 
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================================
        // Student
        // =========================================================

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired();

            entity.Property(s => s.Email)
                .IsRequired();

            entity.Property(s => s.GPA)
                .HasPrecision(5, 2);

            entity.Property(s => s.IsActive)
                .HasDefaultValue(true);
        });

        // =========================================================
        // Course
        // =========================================================

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");

            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.Code)
                .IsUnique();

            entity.Property(c => c.Code)
                .IsRequired();

            entity.Property(c => c.Title)
                .IsRequired();

            entity.Property(c => c.Description)
                .IsRequired();

            entity.Property(c => c.MaxCapacity)
                .IsRequired();
        });

        // =========================================================
        // Enrollment
        // =========================================================

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.EnrolledAt)
                .IsRequired();

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new
            {
                e.StudentId,
                e.CourseId
            })
            .IsUnique();
        });

        // =========================================================
        // Grade
        // =========================================================
        // Grade is configured by GradeConfiguration.cs.
        // ApplyConfigurationsFromAssembly automatically discovers it.
        // =========================================================

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}