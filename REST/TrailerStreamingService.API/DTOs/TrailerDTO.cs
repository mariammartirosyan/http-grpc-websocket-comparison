using System;
namespace TrailerStreamingService.API.DTOs
{
    public class TrailerDTO
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

