using LogIPServiceTestAssignment.Database;
using LogIPServiceTestAssignment.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddGrpc();
builder.Services.AddDbContextFactory<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LogIPDB")));
builder.Services.AddSingleton<EventCommandHandler>();
builder.Services.AddSingleton<EventQueryHandler>();

var host = builder.Build();
host.MapGrpcService<LogService>();
host.Run();

public partial class Program { } 