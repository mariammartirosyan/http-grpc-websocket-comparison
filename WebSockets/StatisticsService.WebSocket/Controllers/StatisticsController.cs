using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatisticsService.WebSocket.DTOs;

namespace StatisticsService.WebSocket.Controllers
{
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly Library.Services.StatisticsService _statisticService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(Library.Services.StatisticsService statisticService, ILogger<StatisticsController> logger)
        {
            _statisticService = statisticService;
            _logger = logger;
        }
        [Authorize]
        [Route("/addStatisticsEntry")]
        public async Task AddStatisticsEntry()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                AddStatisticsResponse addStatisticsResponse = new AddStatisticsResponse() { Succeeded = true };

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var buffer = new byte[1024 * 4];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (received.MessageType == WebSocketMessageType.Text)
                {
                    try
                    {
                        var jsonMessage = Encoding.UTF8.GetString(buffer, 0, received.Count);
                        var addStatisticsRequest = JsonSerializer.Deserialize<AddStatisticsRequest>(jsonMessage);
                        HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader);

                        GetMovieResponse getMovieResponse = null;
                        GetUserDetailsResponse getUserDetailsResponse = null;

                        //getAccountDetails
                        var accountServiceTask = Task.Run(async () =>
                        {
                            using (var authClient = new ClientWebSocket())
                            {
                                authClient.Options.SetRequestHeader("Authorization", authorizationHeader);
                                var accountServiceUrl = Environment.GetEnvironmentVariable("AccountServiceUrl") + "/getUserDetails";
                                await authClient.ConnectAsync(new Uri(accountServiceUrl), CancellationToken.None);

                                string getUserDetailsRequestJson = JsonSerializer.Serialize(new GetUserDetailsRequest() { UserName = addStatisticsRequest.UserName });
                                await authClient.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(getUserDetailsRequestJson)), WebSocketMessageType.Text, true, CancellationToken.None);

                                var responseBuffer = new byte[1024 * 4];
                                var getUserDetailsServerResponse = await authClient.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);
                                var getUserDetailsResponseJson = Encoding.UTF8.GetString(responseBuffer, 0, getUserDetailsServerResponse.Count);
                                getUserDetailsResponse = JsonSerializer.Deserialize<GetUserDetailsResponse>(getUserDetailsResponseJson);
                                await authClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            }
                        });

                        //getMovieById
                        var movieServiceTask = Task.Run(async () =>
                        {
                            using (var authClient = new ClientWebSocket())
                            {
                                authClient.Options.SetRequestHeader("Authorization", authorizationHeader);
                                var movieServiceUrl = Environment.GetEnvironmentVariable("MovieServiceUrl") + "/getMovieById";
                                await authClient.ConnectAsync(new Uri(movieServiceUrl), CancellationToken.None);

                                string getMovieRequestJson = JsonSerializer.Serialize(new GetMovieRequest() { MovieId = addStatisticsRequest.MovieId });
                                await authClient.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(getMovieRequestJson)), WebSocketMessageType.Text, true, CancellationToken.None);

                                var responseBuffer = new byte[1024 * 4];
                                var getMovieServerResponse = await authClient.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);
                                var getMovieResponseJson = Encoding.UTF8.GetString(responseBuffer, 0, getMovieServerResponse.Count);
                                getMovieResponse = JsonSerializer.Deserialize<GetMovieResponse>(getMovieResponseJson);
                                await authClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            }

                        });

                        await Task.WhenAll(accountServiceTask, movieServiceTask);

                        if (getUserDetailsResponse != null && getUserDetailsResponse.Succeeded &&
                                getMovieResponse != null && getMovieResponse.Succeeded)
                        {
                            var movie = getMovieResponse.Movie;
                            foreach (var genre in movie.Genre)
                            {
                                _statisticService.Add(new Library.Domain.UserMovieStatistic()
                                {
                                    UserId = getUserDetailsResponse.User.Id,
                                    MovieId = movie.Id,
                                    Genre = genre.Name,
                                    DateTime = DateTime.UtcNow
                                });
                            }
                            addStatisticsResponse.Message = "Statistics entry was added";
                            _logger.LogInformation(DateTime.Now + " - " + addStatisticsResponse.Message);
                        }
                        else
                        {
                            addStatisticsResponse.Succeeded = false;
                            addStatisticsResponse.Message = getUserDetailsResponse?.Message + "\n" + getMovieResponse?.Message;
                        }

                    }
                    catch (Exception ex)
                    {
                        addStatisticsResponse.Message = ex.ToString();
                        addStatisticsResponse.Succeeded = false;
                        _logger.LogError(DateTime.Now + " - Add Statistics - " + ex.ToString());
                    }
                    finally
                    {
                        var responseBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(addStatisticsResponse));
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        if (addStatisticsResponse.Succeeded)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        }
                        else
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Closing", CancellationToken.None);
                        }
                    }
                }


            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    }
}

