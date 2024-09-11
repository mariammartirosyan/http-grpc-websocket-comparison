using System;
using System.Text.Json.Serialization;

namespace StatisticsService.API.DTOs
{
	public class UserDTO
	{
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; }
    }
}

