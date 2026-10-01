
using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class BackgroundJobEntityConfiguration : IEntityTypeConfiguration<BackgroundJobEntity>
{
    public void Configure(EntityTypeBuilder<BackgroundJobEntity> builder)
    {
        builder.ToTable("BackgroundJobs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Error)
            .HasMaxLength(4000);

        builder.HasIndex(x => x.Status);

        builder.HasOne<JobApplicationEntity>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobApplicationId);
    }
}