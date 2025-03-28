using Grpc.Core;
using LogIPServiceTestAssignment.Database;
using Microsoft.EntityFrameworkCore;

namespace LogIPServiceTestAssignment.Services;

public class LogService(EventQueryHandler queryHandler, EventCommandHandler commandHandler, ILogger<LogService> logger) : LogCollectionService.LogCollectionServiceBase
{
    // Existing LogEvent method
    public override async Task<LogResponse> LogEvent(LogRequest request, ServerCallContext context)
    {
        var logResponce = new LogResponse();
        try
        {
            commandHandler.StoreEvent(request.UserId, request.Ip, DateTime.UtcNow, context.CancellationToken);
            logResponce.Success = true;
        }
        catch (Exception ex)
        {
            logResponce.Exception = ex.Message;
        }
        return logResponce;
    }

    // Find users whose content partially matches the input string
    public override async Task<FindUsersByIpResponse> FindUsersByIp(FindUsersByIpRequest request, ServerCallContext context)
    {
        var userIds = await queryHandler.GetEventsByIp(request.Content, context.CancellationToken);
        return new FindUsersByIpResponse { UserIds = { userIds } };
    }

    // Find all IP addresses associated with a specific user
    public override async Task<FindUserIPsResponse> FindUserIPs(FindUserIPsRequest request, ServerCallContext context)
    {
        var ips = await queryHandler.GetEventsByUser(request.UserId, context.CancellationToken);
        return new FindUserIPsResponse { Ips = { ips } };
    }

    // Find the last connection time and IP address for a specific user
    public override async Task<FindLastUserConnectionResponse> FindLastUserConnection(FindLastUserConnectionRequest request, ServerCallContext context)
    {
        var lastConnection =  await queryHandler.GetLastEvent(request.UserId, context.CancellationToken);
        if (lastConnection is null) 
            return new FindLastUserConnectionResponse(); // Return empty response if no records found
        
        return new FindLastUserConnectionResponse
            {
                LastIp = lastConnection.Ip,
                LastConnectionTime = lastConnection.OccurrenceDate.ToString("o") // ISO 8601 format
            };
    }
}


