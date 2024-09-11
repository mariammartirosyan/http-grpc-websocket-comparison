using System;
namespace StatisticsService.API.Helpers
{
	public class Response
	{
		public bool IsSuccessful { get; set; }
        public string Message { get; set; }

		public Response(bool isSuccessful, string message)
		{
			IsSuccessful = isSuccessful;
			Message = message;

        }
    }
}

