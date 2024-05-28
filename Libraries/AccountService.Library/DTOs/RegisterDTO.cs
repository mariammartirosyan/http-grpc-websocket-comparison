using System;
namespace AccountService.Library.DTOs
{
	public class RegisterDTO
	{
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

