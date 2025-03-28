using Microsoft.EntityFrameworkCore;

namespace LogIPServiceTestAssignment.Database;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<LogEntry> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEntry>().HasKey(e => e.Id);
    }
}