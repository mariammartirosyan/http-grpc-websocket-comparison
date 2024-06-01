using System;
namespace StatisticsService.WebSocket.DTOs
{
	public class GetUserDetailsResponse
	{
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public UserDTO User { get; set; }
    }

    public class UserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}

