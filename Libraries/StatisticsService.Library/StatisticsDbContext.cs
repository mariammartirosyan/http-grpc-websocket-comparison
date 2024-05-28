using System;
using Microsoft.EntityFrameworkCore;
using StatisticsService.Library.Domain;

namespace StatisticsService.Library
{
	public class StatisticsDbContext : DbContext
    {
        public DbSet<UserMovieStatistic> UserMovieStatistic { get; set; }

        public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options)
            : base(options)
        {
        }
    }
}

