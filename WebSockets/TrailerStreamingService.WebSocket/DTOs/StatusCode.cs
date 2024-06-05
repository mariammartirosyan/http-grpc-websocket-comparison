using System;
namespace TrailerStreamingService.WebSocket.DTOs
{
    public enum StatusCode
    {
        Success = 200,
        Unauthorized = 401,
        NotFound = 404,
        Conflict = 409,
        InternalServerError = 500
    }
}

