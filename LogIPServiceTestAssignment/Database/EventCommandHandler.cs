using Microsoft.EntityFrameworkCore;

namespace LogIPServiceTestAssignment.Database;

public class EventCommandHandler(IDbContextFactory<EventDbContext> dbContextFactory, ILogger<EventCommandHandler> logger)
{
    public async Task StoreEvent(long userId, string ip, DateTime timestamp, CancellationToken cancellationToken = default) {
        var eventObj = new LogEntry() {
            OccurrenceDate = timestamp,
            UserId = userId,
            Ip = ip
        };
        logger.LogDebug("storing event to db");
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        context.Events.Add(eventObj);
        await context.SaveChangesAsync(cancellationToken);
    }
}