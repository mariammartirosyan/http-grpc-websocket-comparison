using System;
namespace TrailerStreamingService.WebSocket.DTOs
{
    public class FetchTrailerRequest
    {
        public UserDTO User { get; set; }
        public int MovieId { get; set; }
    }
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

