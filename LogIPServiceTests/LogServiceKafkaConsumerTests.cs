using Confluent.Kafka;
using LogIPServiceTestAssignment.Database;
using LogIPServiceTestAssignment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Allure.Net.Commons;
using Moq;
using Allure.Xunit;
using Allure.Xunit.Attributes;

namespace LogIPServiceTests
{
    public class LogServiceKafkaConsumerTests
    {
        private const string KafkaBootstrapServers = "localhost:9092";
        //private const string KafkaBootstrapServers = "kafka:9092";
        
        private readonly IDbContextFactory<EventDbContext> _dbContextFactory;

        public LogServiceKafkaConsumerTests()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContextFactory = new EventDbContextFactory(options);
        }

        [Theory]
        [AllureFeature("Kafka Consumer")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureStory("Event Store")]
        [InlineData(123453, "198.168.1.1", "user-ip-topic")]
        public async Task ExecuteAsyncShouldProcessMessages(int userId, string ip, string topic)
        {
            // Arrange
            var mockConsumer =  new Mock<IConsumer<string, string>>();
            var loggerCommandHandler = new Mock<ILogger<EventCommandHandler>>();
            var loggerKafkaConsumerService = new Mock<ILogger<KafkaConsumerService>>();

            var eventCommandHandler = new EventCommandHandler(_dbContextFactory, loggerCommandHandler.Object);

            var inMemorySettings = new Dictionary<string, string> {
                {"Kafka", "Kafka"},
                {"Kafka:BootstrapServers", KafkaBootstrapServers},
                {"Kafka:GroupId", "worker-service-group"},
                {"Kafka:UserIPTopic", "user-ip-topic"}

            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            
        
            var kafkaConsumerService = new KafkaConsumerService(eventCommandHandler, configuration, loggerKafkaConsumerService.Object );

            // Act
            await PublishKafkaMessage(userId, ip, topic);
            await Task.Delay(1000);
            await kafkaConsumerService.StartAsync(CancellationToken.None);

            // Assert
            mockConsumer.Verify(c => c.Consume(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var logEntry = await dbContext.Events.FirstOrDefaultAsync();
            Assert.NotNull(logEntry);
            Assert.Equal(userId, logEntry.UserId);
            Assert.Equal(ip, logEntry.Ip);
        }
        
                        
        private async Task PublishKafkaMessage(int userId, string ip, string topic)
        {
            var config = new ProducerConfig { BootstrapServers = KafkaBootstrapServers };
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            var kafkaMessage = $"{{\"UserId\": {userId}, \"Ip\": \"{ip}\"}}";
            var deliveryResult = await producer.ProduceAsync(topic, new Message<Null, string> { Value = kafkaMessage });
            Assert.NotNull(deliveryResult);
        }
    }
}