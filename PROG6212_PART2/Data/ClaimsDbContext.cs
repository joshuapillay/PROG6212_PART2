using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ClaimsDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Lecturer> Lecturers { get; set; }//saving lecturer info
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }
}
