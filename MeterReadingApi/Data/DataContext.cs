using MeterReadingApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Principal;

namespace MeterReadingApi.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<Account> Accounts { get; set; }

    }
}
