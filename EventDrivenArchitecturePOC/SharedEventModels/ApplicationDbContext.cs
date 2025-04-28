using Microsoft.EntityFrameworkCore;


namespace SharedEventModels
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<StoredEvent> Events { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
    }
}
