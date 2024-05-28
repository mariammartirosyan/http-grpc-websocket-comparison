using System;
using StatisticsService.Library;
using StatisticsService.Library.Domain;
using StatisticsService.Library.Repositories;

namespace StatisticsService.Library.Services
{
	public class StatisticsService
	{
        private readonly StatisticsRepository _statisticsRepository;

        public StatisticsService(StatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }
        public void Add(UserMovieStatistic userMovieStatistic)
        {
            _statisticsRepository.Add(userMovieStatistic);
        }
    }
}

