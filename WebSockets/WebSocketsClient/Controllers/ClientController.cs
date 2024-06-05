using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebSocketsClient.DTOs;

namespace WebSocketsClient.Controllers
{
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }

        [HttpPost("fetchTrailer")]
        public async Task<IActionResult> FetchTrailer(TrailerDTO trailerDTO)
        {
            using (var client = new ClientWebSocket())
            {
                var trailerStreamingServiceUrl = Environment.GetEnvironmentVariable("TrailerStreamingServiceUrl") + "/fetchTrailer";
                await client.ConnectAsync(new Uri(trailerStreamingServiceUrl), CancellationToken.None);

                Console.WriteLine("Connected to the server.");

                string jsonString = JsonSerializer.Serialize(trailerDTO);

                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                await client.SendAsync(new ArraySegment<byte>(byteArray), WebSocketMessageType.Text, true, CancellationToken.None);
                var responseBuffer = new byte[8192 * 2000];
                var response = await client.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);
                if (response.MessageType == WebSocketMessageType.Binary)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    return File(responseBuffer, "video/mp4");
                }

                var responseJson = Encoding.UTF8.GetString(responseBuffer, 0, response.Count);
                _logger.LogInformation(responseJson);
                var fetchTrailerResponse = JsonSerializer.Deserialize<FetchTrailerResponse>(responseJson);

                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                

                return StatusCode((int)fetchTrailerResponse.StatusCode, fetchTrailerResponse.Message);
            }
        }
        
    }
}

