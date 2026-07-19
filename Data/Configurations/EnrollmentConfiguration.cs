using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;


namespace TmsApi.Data.Configurations;

public class EnrollmentConfiguration
    : IEntityTypeConfiguration<Enrollment>
{

    public void Configure(
        EntityTypeBuilder<Enrollment> builder)
    {

        // Primary key
        builder.HasKey(e => e.Id);



        // Student relationship
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);



        // Course relationship
        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);



        // Optional: prevent duplicate enrollment
        builder.HasIndex(e => new
        {
            e.StudentId,
            e.CourseId

        })
        .IsUnique();

    }
}