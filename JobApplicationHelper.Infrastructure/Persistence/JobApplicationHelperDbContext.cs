using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Infrastructure.Persistence;

public sealed class JobApplicationHelperDbContext(DbContextOptions<JobApplicationHelperDbContext> options)
    : DbContext(options)
{
}