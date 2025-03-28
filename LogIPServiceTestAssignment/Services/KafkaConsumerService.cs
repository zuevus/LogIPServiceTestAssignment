using System.Text.Json;
using Confluent.Kafka;
using LogIPServiceTestAssignment.Database;
using LogIPServiceTestAssignment.Share;
using Microsoft.EntityFrameworkCore;

namespace LogIPServiceTestAssignment.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly EventCommandHandler _commandHandler;
    private readonly ILogger<KafkaConsumerService> _logger; 
    private readonly string _kafkaTopic;

    public KafkaConsumerService(EventCommandHandler commandHandler, IConfiguration configuration, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger; 
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers"),
            GroupId = configuration.GetValue<string>("Kafka:GroupId"),
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Ensure manual commit for guaranteed delivery
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _kafkaTopic = configuration.GetValue<string>("Kafka:UserIPTopic");
       // _consumer.Subscribe(configuration.GetValue<string>("Kafka:UserIPTopic"));
        _logger.LogInformation($"Kafka consumer service started: \n - BootstrapServers {config.BootstrapServers}"
                               +$" \n - GroupId {config.GroupId} \n - Subscribes: {string.Join(", ", _consumer.Subscription)} ");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (false == _consumer.Subscription.Contains(_kafkaTopic))
            {
                _consumer.Subscribe(_kafkaTopic);
                _logger.LogInformation($"Kafka subscribes: {string.Join(", ", _consumer.Subscription)} ");
            }
          
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var message =JsonSerializer.Deserialize<UserIpMessage>(consumeResult.Message.Value);
                if (message == null)
                    throw new InvalidOperationException("Failed to deserialize Kafka message.");
                
                _commandHandler.StoreEvent(message.UserId, message.Ip, consumeResult.Message.Timestamp.UtcDateTime, stoppingToken);
                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or retry)
                Console.WriteLine($"Error processing Kafka message: {ex.Message}");
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}