using Grpc.Net.Client;
using LogIPServiceTestAssignment;
using LogIPServiceTestAssignment.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LogIPServiceTestAssignment;
using LogIPServiceTestAssignment.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace LogIPServiceTests;

public class LogServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly GrpcChannel _channel;
    private readonly WebApplicationFactory<Program> _factory;

    public LogServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<EventDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContextFactory<EventDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                });
            });
        }).CreateClient();
        
        _channel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = client
        });
    }

    [Fact]
    public async Task LogEventShouldSaveLogEntry()
    {
        // Arrange
        var client = new LogCollectionService.LogCollectionServiceClient(_channel);
        var request = new LogRequest { UserId = 12345, Ip = "192.168.1.1" };

        // Act
        var response = await client.LogEventAsync(request);

        // Assert
        Assert.True(response.Success);
    }

    [Theory]
    [InlineData(12345, "192.168.1.1", "192", true)]
    [InlineData(12345, "192.168.1.1", "10.0", false)]
    [InlineData(12345, "192.168.1.1", "1.1", true)]
    [InlineData(0, "invalid.ip", "192", false)]
    [InlineData(-1, "10.0.0.1", "192", false)]
    public async Task FindUsersByContentShouldReturnMatchingUsers(int userId, string ip, string content, bool expectedSuccess)
    {
        // Arrange
        var client = new LogCollectionService.LogCollectionServiceClient(_channel);
        var logRequest = new LogRequest { UserId = userId, Ip = ip };
        await client.LogEventAsync(logRequest);

        var request = new FindUsersByIpRequest() { Content = content };

        // Act
        var response = await client.FindUsersByIpAsync(request);

        // Assert
        if (expectedSuccess)
        {
            Assert.Single(response.UserIds);
            Assert.Equal(12345, response.UserIds[0]);
        }
        else
        {
            Assert.Empty(response.UserIds);
        }
    }
    
    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}