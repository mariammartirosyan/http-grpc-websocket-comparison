using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.WebSocket.DTOs;

namespace MovieService.WebSocket.Controllers
{
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly Library.Services.MovieService _movieService;

        public MovieController(
            Library.Services.MovieService movieService,
            ILogger<MovieController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        [Authorize]
        [Route("/getMovieById")]
        public async Task GetMovieById()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[1024 * 4];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                var getMovieResponse = new GetMovieResponse() { Succeeded = true };

                if (received.MessageType == WebSocketMessageType.Text)
                {
                    try
                    {
                        var jsonMessage = Encoding.UTF8.GetString(buffer, 0, received.Count);
                        var getMovieRequest = JsonSerializer.Deserialize<GetMovieRequest>(jsonMessage);

                        var movie = _movieService.GetMovieById(getMovieRequest.MovieId);
                        if (movie != null)
                        {
                            getMovieResponse.Movie = movie;
                        }
                        else
                        {
                            getMovieResponse.Message = $"Movie with Id-{getMovieRequest.MovieId} was not found";
                            getMovieResponse.Succeeded = false;
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(DateTime.Now + $" - Get Movie\n" + ex.ToString());
                        getMovieResponse.Message = ex.ToString();
                        getMovieResponse.Succeeded = false;
                    }

                    var responseBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(getMovieResponse));
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
         
    }
}

