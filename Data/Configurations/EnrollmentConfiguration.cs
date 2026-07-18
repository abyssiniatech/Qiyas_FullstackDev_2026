using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Models;


public class EnrollmentConfiguration 
:IEntityTypeConfiguration<Enrollment>
{


public void Configure(EntityTypeBuilder<Enrollment> builder)
{


builder.HasKey(x=>x.Id);



builder.HasOne(e=>e.Student)

.WithMany(s=>s.Enrollments)

.HasForeignKey(e=>e.StudentId)

.OnDelete(DeleteBehavior.Restrict);



// Prevent deleting courses that still have enrollments
builder.HasOne(e=>e.Course)

.WithMany(c=>c.Enrollments)

.HasForeignKey(e=>e.CourseId)

.OnDelete(DeleteBehavior.Restrict);


}


}