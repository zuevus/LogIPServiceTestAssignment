using LogIPServiceTestAssignment.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogIPServiceTests;

public class EventDbContextFactory : IDbContextFactory<EventDbContext>
{
    private readonly DbContextOptions<EventDbContext> _options;

    public EventDbContextFactory(DbContextOptions<EventDbContext> options)
    {
        _options = options;
    }

    public EventDbContext CreateDbContext()
    {
        return new EventDbContext(_options);
    }
}
