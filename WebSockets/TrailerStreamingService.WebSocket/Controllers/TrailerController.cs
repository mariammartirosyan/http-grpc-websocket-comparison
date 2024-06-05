using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrailerStreamingService.WebSocket.DTOs;

namespace TrailerStreamingService.WebSocket.Controllers
{
    [ApiController]
    public class TrailerController: ControllerBase
	{

        private readonly Library.Services.StreamingService _streamingService;
        private readonly ILogger<TrailerController> _logger;

        public TrailerController(Library.Services.StreamingService streamingService, ILogger<TrailerController> logger)
        {
            _streamingService = streamingService;
            _logger = logger;
        }

        [Route("/fetchTrailer")]
        public async Task FetchTrailer()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                FetchTrailerResponse fetchTrailerResponse = new FetchTrailerResponse() { StatusCode = DTOs.StatusCode.Success };

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[1024 * 4];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (received.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, received.Count);
                    var fetchTrailerRequest = JsonSerializer.Deserialize<FetchTrailerRequest>(message);

                    //authenticate
                    using (var client = new ClientWebSocket())
                    {
                        var accountServiceUrl = Environment.GetEnvironmentVariable("AccountServiceUrl") + "/login";
                        await client.ConnectAsync(new Uri(accountServiceUrl), CancellationToken.None);

                        string userJson = JsonSerializer.Serialize(fetchTrailerRequest.User);
                        byte[] byteArray = Encoding.UTF8.GetBytes(userJson);
                        await client.SendAsync(new ArraySegment<byte>(byteArray), WebSocketMessageType.Text, true, CancellationToken.None);

                        var responseBuffer = new byte[1024 * 4];
                        var loginResponse = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);
                        var loginResponseJson = Encoding.UTF8.GetString(responseBuffer, 0, loginResponse.Count);
                        var loginResponseDto = JsonSerializer.Deserialize<LoginResponse>(loginResponseJson);
                       

                        if ((int)loginResponseDto.StatusCode == 200)
                        {
                            try
                            {
                                using (var authClient = new ClientWebSocket())
                                {
                                  
                                    var token = loginResponseDto.Message;
                                    authClient.Options.SetRequestHeader("Authorization", $"Bearer {token}");

                                    //add new statistics entry
                                    var statisticsServiceUrl = Environment.GetEnvironmentVariable("StatisticsServiceUrl") + "/addStatisticsEntry";

                                    await authClient.ConnectAsync(new Uri(statisticsServiceUrl), CancellationToken.None);

                                    string addStatisticsJson = JsonSerializer.Serialize(new AddStatisticsRequest() { UserName = fetchTrailerRequest.User.UserName, MovieId = fetchTrailerRequest.MovieId });
                                    await authClient.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(addStatisticsJson)), WebSocketMessageType.Text, true, CancellationToken.None);

                                    var addStatisticsResponse = await authClient.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);
                                    await authClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

                                    var bytes = _streamingService.GetMovieTrailerBytes(fetchTrailerRequest.MovieId);
                                    if (bytes != null)
                                    {
                                        fetchTrailerResponse.Video = bytes;
                                        fetchTrailerResponse.Message = $"The movie trailer with movie ID = {fetchTrailerRequest.MovieId} is successfully fetched";
                                        _logger.LogInformation(fetchTrailerResponse.Message);
                                    }
                                    else
                                    {
                                        fetchTrailerResponse.StatusCode = DTOs.StatusCode.NotFound;
                                        fetchTrailerResponse.Message = $"The movie trailer with movie ID = {fetchTrailerRequest.MovieId} was not found";
                                        _logger.LogInformation(fetchTrailerResponse.Message);
                                    }

                                }
                            }
                            catch(Exception ex)
                            {
                                fetchTrailerResponse.StatusCode = DTOs.StatusCode.InternalServerError;
                                fetchTrailerResponse.Message = ex.ToString();
                                _logger.LogError(DateTime.Now + " - Get Movie Trailer - " + ex.ToString());
                            }
                        }
                        else
                        {
                            fetchTrailerResponse.StatusCode = loginResponseDto.StatusCode;
                            fetchTrailerResponse.Message = loginResponseDto.Message;
                        }
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                  
                }
               
                if ((int)fetchTrailerResponse.StatusCode==200)
                {
                   await webSocket.SendAsync(new ArraySegment<byte>(fetchTrailerResponse.Video), WebSocketMessageType.Binary, true, CancellationToken.None);
                   await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fetchTrailerResponse))), WebSocketMessageType.Text, true, CancellationToken.None);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
               
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}

