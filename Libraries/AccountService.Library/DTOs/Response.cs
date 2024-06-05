using System;
namespace AccountService.Library.DTOs
{
	public class Response
	{
		public StatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
	public enum StatusCode
	{
		Success = 200,
		Unauthorized = 401,
        NotFound = 404,
        Conflict = 409,
		InternalServerError = 500
	}
}

