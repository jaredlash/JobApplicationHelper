using JobApplicationHelper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplicationHelper.Infrastructure.Persistence.Configurations;

public sealed class ContextEntityConfiguration : IEntityTypeConfiguration<ContextEntity>
{
    public void Configure(EntityTypeBuilder<ContextEntity> builder)
    {
        builder.ToTable("Contexts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnType("citext")
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
