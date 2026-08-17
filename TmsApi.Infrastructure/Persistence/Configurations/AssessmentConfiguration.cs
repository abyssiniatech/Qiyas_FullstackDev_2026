using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class AssessmentConfiguration 
    : IEntityTypeConfiguration<Assessment>
{
    public void Configure(
        EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
    }
}


