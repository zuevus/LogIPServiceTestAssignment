using Microsoft.EntityFrameworkCore;

namespace LogIPServiceTestAssignment.Database;

public class EventQueryHandler(IDbContextFactory<EventDbContext> dbContextFactory, ILogger<EventQueryHandler> logger)
{
    public async Task<string[]> GetEventsByUser(long userId, CancellationToken cancellationToken = default) {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Events.Where(e => e.UserId == userId).Select(e => e.Ip).ToArrayAsync(cancellationToken);
    }

    public async Task<long[]> GetEventsByIp(string content, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userIds = await context.Events
            .Where(e => e.Ip.Contains(content))
            .Select(e => e.UserId)
            .ToArrayAsync(cancellationToken);
        return userIds;
    }
    
    public async Task<LogEntry?> GetLastEvent(long userId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var lastConnection = await context.Events
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.OccurrenceDate)
            .FirstOrDefaultAsync(cancellationToken);
        return lastConnection;
    }

}