using Grpc.Net.Client;
using GrpcClient.DTOs;
using Microsoft.AspNetCore.Mvc;
using TrailerStreamingService.gRPC;

namespace GrpcClient.Controllers
{
    [ApiController]
    public class HomeController: ControllerBase
	{
        [HttpPost("fetchTrailer")]
        public async Task<IActionResult> FetchTrailer(TrailerDTO trailerDTO)
        {
            using var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("TrailerStreamingServiceUrl"), new GrpcChannelOptions
            {
                MaxReceiveMessageSize = int.MaxValue
            });
            var client = new Streaming.StreamingClient(channel);
            var reply = await client.GetMovieTrailerAsync(
                              new TrailerRequest { User = new User() { UserName = trailerDTO.User.UserName, Password = trailerDTO.User.Password }, MovieId = trailerDTO.MovieId});
            if (reply.Succeeded)
            {
                return File(reply.Video.ToByteArray(), "video/mp4");
            }
            return StatusCode(400, reply.Message);

        }
    }

}

