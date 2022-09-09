using Microsoft.EntityFrameworkCore;
namespace Aisistant.Data;

public class AIAgentDBContext : DbContext
{
    public AIAgentDBContext(DbContextOptions<AIAgentDBContext> options) : base(options)
    {


    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.SessionTranscript>()
            .HasKey(c => new { c.sessionID, c.timestamp });
    }
    public DbSet<Models.SessionTranscript> SessionTranscript { get; set; }
}