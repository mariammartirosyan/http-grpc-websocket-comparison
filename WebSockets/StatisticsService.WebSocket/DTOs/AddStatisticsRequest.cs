using System;
namespace StatisticsService.WebSocket.DTOs
{
	public class AddStatisticsRequest
	{
        public string UserName { get; set; }
        public int MovieId { get; set; }
    }

}

