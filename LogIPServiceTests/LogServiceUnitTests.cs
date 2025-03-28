using LogIPServiceTestAssignment.Database;
using LogIPServiceTestAssignment.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Grpc.Core;
using LogIPServiceTestAssignment;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace LogIPServiceTests;

public class LogServiceUnitTests
{
    private readonly IDbContextFactory<EventDbContext> _dbContextFactory;
    private readonly LogService _logService;
    private readonly Mock<ILogger<LogService>> _mockLogger;
    
    

    public LogServiceUnitTests()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
       .UseInMemoryDatabase(databaseName: "TestDatabase")
       .Options;
       
        _dbContextFactory = new EventDbContextFactory(options);
        
        _mockLogger = new Mock<ILogger<LogService>>();
        var mockCommandHandlerLogger = new Mock<ILogger<EventCommandHandler>>();
        var mockQueryHandlerLogger = new Mock<ILogger<EventQueryHandler>>();

        var commandHandler = new EventCommandHandler(_dbContextFactory, mockCommandHandlerLogger.Object);
        var queryHandler = new EventQueryHandler(_dbContextFactory, mockQueryHandlerLogger.Object);

        _logService = new LogService(queryHandler, commandHandler, _mockLogger.Object);
    }
    
    [Theory]
    [AllureFeature("Unit Test")]
    [AllureSeverity(SeverityLevel.critical)]
    [AllureStory("Log Event")]
    [InlineData(12345, "192.168.1.1")]
    public async Task LogEventShouldSaveLogEntryGrpc(int userId, string ip)
    {
        // Arrange
        var request = new LogRequest { UserId = userId, Ip = ip };

        // Act
        var response = await _logService.LogEvent(request, new Mock<ServerCallContext>().Object);

        // Assert
        Assert.True(response.Success);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var logEntry = await dbContext.Events.FirstOrDefaultAsync();
        Assert.NotNull(logEntry);
        Assert.Equal(userId, logEntry.UserId);
        Assert.Equal(ip, logEntry.Ip);
    }
}

