using Microsoft.EntityFrameworkCore;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options) { }

    public DbSet<Claim> Claims { get; set; }
}
