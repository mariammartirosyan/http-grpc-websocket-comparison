using System;
namespace TrailerStreamingService.WebSocket.DTOs
{
	public class LoginResponse
	{
        public StatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}

