using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;
using TmsApi.Models;
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(x => x.Id);


        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);


        // Shadow audit property
        builder.Property<DateTime>("LastUpdated")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}