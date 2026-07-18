using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class AssessmentConfiguration 
    : IEntityTypeConfiguration<Assessment>
{
    public void Configure(
        EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
    }
}