using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;
using TmsApi.Models;


public class CourseConfiguration 
:IEntityTypeConfiguration<Course>
{


public void Configure(EntityTypeBuilder<Course> builder)
{


builder.HasKey(x=>x.Id);


builder.Property(x=>x.Title)
.IsRequired()
.HasMaxLength(150);


}


}