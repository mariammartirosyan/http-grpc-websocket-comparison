using System;
using StatisticsService.Library.Domain;

namespace StatisticsService.Library.Repositories
{
	public class StatisticsRepository
	{
        private readonly StatisticsDbContext dbContext;

        public StatisticsRepository(StatisticsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Add(UserMovieStatistic userMovieStatistic)
        {
            dbContext.UserMovieStatistic.Add(userMovieStatistic);
            dbContext.SaveChanges();
        }
        public IEnumerable<UserMovieStatistic> GetAll()
        {
            return dbContext.UserMovieStatistic.ToList();
        }
    }
}

