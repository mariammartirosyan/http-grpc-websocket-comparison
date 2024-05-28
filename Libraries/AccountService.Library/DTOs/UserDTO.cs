using System;
using System.Text.Json.Serialization;

namespace AccountService.Library.DTOs
{
	public class UserDTO
	{
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        [JsonIgnore]
        public string Role { get; set; }

    }
}

