using System;
namespace TrailerStreamingService.WebSocket.DTOs
{
    public class FetchTrailerResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public byte[] Video { get; set; }
    }

}

