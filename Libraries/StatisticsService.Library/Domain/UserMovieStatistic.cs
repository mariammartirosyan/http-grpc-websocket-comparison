using System;
namespace StatisticsService.Library.Domain
{
	public class UserMovieStatistic
	{
        public int Id { get; set; }
        public string UserId { get; set; }
        public int MovieId { get; set; }
        public string Genre { get; set; }
        public DateTime DateTime { get; set; }
    }
}

