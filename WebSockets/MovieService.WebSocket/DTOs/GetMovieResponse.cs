using System;
using MovieService.Library.Domain;

namespace MovieService.WebSocket.DTOs
{
	public class GetMovieResponse
	{
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public Movie Movie { get; set; }
    }
}

