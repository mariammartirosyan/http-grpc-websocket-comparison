using System;
using AccountService.Library.DTOs;

namespace AccountService.WebSocket.DTOs
{
	public class UserDetailsReply
	{
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public UserDTO User { get; set; }
    }
}

