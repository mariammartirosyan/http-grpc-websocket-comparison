using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TestApp.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }


    [HttpGet("test")]
    public async Task<IActionResult> Test(string version, int numberOfRequests)
    {
        if (numberOfRequests <= 0)
        {
            return BadRequest();
        }
        switch (version.ToLower())
        {
            case "http":
                var httpUrl = Environment.GetEnvironmentVariable("HttpEntryPointUrl");
                if (!string.IsNullOrEmpty(httpUrl))
                {
                    await SendRequests(httpUrl, numberOfRequests);
                }
                else
                {
                    return BadRequest("The entry point url for HTTP version is not specified.");
                }
                break;
            case "grpc":
                var grpcUrl = Environment.GetEnvironmentVariable("GrpcEntryPointUrl");
                if (!string.IsNullOrEmpty(grpcUrl))
                {
                    await SendRequests(grpcUrl, numberOfRequests);
                }
                else
                {
                    return BadRequest("The entry point url for gRPC version is not specified.");
                }
                break;
            case "websockets":
                var webSocketsUrl = Environment.GetEnvironmentVariable("WebSocketsEntryPointUrl");
                if (!string.IsNullOrEmpty(webSocketsUrl))
                {
                    await SendRequests(webSocketsUrl, numberOfRequests);
                }
                else
                {
                    return BadRequest("The entry point url for WebSockets version is not specified.");
                }
                break;
            default:
                return BadRequest("Invalid version specified. Valid versions are: http, grpc, websockets.");
        }

        return Ok("Requests completed.");
    }

    private async Task SendRequests(string url, int numberOfRequests)
    {
        HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(3000);
        var tasks = new List<Task>();

        for (int i = 0; i < numberOfRequests; i++)
        {
            int movieId = (i % 20) + 1;
            var payload = new
            {
                User = new { UserName = "admin", Password = "pass" },
                MovieId = movieId
            };

            
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(url))
            {
                Version = HttpVersion.Version10,
                Content = content
            };

            tasks.Add(Task.Run(async () =>
            {
                var response = await client.SendAsync(httpRequestMessage);
                response.EnsureSuccessStatusCode();
            }));
        }

        await Task.WhenAll(tasks);
    }
}
