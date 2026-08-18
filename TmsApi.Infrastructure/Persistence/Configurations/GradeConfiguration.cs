using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");

        // Primary Key
        builder.HasKey(g => g.GradeId);

        builder.Property(g => g.GradeId)
            .ValueGeneratedOnAdd();

        // Student
        builder.Property(g => g.StudentId)
            .IsRequired();

        // Course
        builder.Property(g => g.CourseId)
            .IsRequired();

        // Assessment Type
        builder.Property(g => g.AssessmentType)
            .IsRequired()
            .HasMaxLength(50);

        // Score
        builder.Property(g => g.Score)
            .HasPrecision(5, 2)
            .IsRequired();

        // Created At
        builder.Property(g => g.CreatedAt)
            .IsRequired();

        // Student -> Grades
        builder.HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Grade -> Course
        // Course has no Grades collection navigation.
        builder.HasOne(g => g.Course)
            .WithMany()
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}