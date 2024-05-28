using System;
using AccountService.gRPC;
using TrailerStreamingService.gRPC;
using MovieService.gRPC;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using StatisticsService.gRPC;
using Grpc.Core;
using System.Collections;
using TrailerStreamingService.Library.Services;
using GrpcClient.DTOs;

namespace GrpcClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController: ControllerBase
	{
        #region Test
        private readonly StreamingService _streamingService;

        public HomeController(StreamingService streamingService)
        {
            _streamingService = streamingService;
        }
        [HttpGet("testLogin")]
        public async Task<IActionResult> TestLogin()
        {
            using var channel = GrpcChannel.ForAddress(" http://localhost:5033");
            var client = new AccountManagement.AccountManagementClient(channel);
            var reply = await client.LoginAsync(
                              new LoginRequest { UserName = "admin", Password = "pass" });
            return Ok(reply.Token);
        }
        [HttpGet("testStreaming")]
        public async Task<IActionResult> TestStreaming()
        {
            using var channel = GrpcChannel.ForAddress(" http://localhost:5265", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = int.MaxValue
            });
            var client = new Streaming.StreamingClient(channel);
            var reply = await client.GetMovieTrailerAsync(
                              new TrailerRequest { UserName = "admin", Password = "pass", MovieId = 3 });
            var bytes = reply.Video.ToByteArray();

             return File(bytes, "video/mp4");
        }
        [HttpGet("testMovie")]
        public async Task<IActionResult> TestMovie()
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5264");
            var client = new MovieManagement.MovieManagementClient(channel);
            var reply = await client.GetMovieByIdAsync(
                              new GetMovieRequest { Id =1 });
            return Ok(reply);
        }

        [HttpGet("testStatistics")]
        public async Task<IActionResult> TestStatistics()
        {
            using var channel = GrpcChannel.ForAddress(" http://localhost:5033");
            var client = new AccountManagement.AccountManagementClient(channel);
            var loginReply = await client.LoginAsync(
                              new LoginRequest { UserName = "admin", Password = "pass" });

            var headers = new Metadata();
            var token = loginReply.Message;
            headers.Add("Authorization", $"Bearer {token}");

            using var statisticsChannel = GrpcChannel.ForAddress("http://localhost:5164");
            var statisticsClient = new StatisticsManagement.StatisticsManagementClient(statisticsChannel);
            var statisticsReply =  await statisticsClient.AddStatisticsEntryAsync(
                              new AddStatisticsRequest { UserName ="admin", MovieId = 1 }, headers);
            //using var channel = GrpcChannel.ForAddress("http://localhost:5164");
            //var client = new StatisticsManagement.StatisticsManagementClient(channel);
            //var reply = client.AddStatisticsEntry(
            //                  new AddStatisticsRequest() { MovieId=1, UserName="admin"});
            return Ok();
        }
        [HttpGet("TestStreaming1")]
        public async Task<IActionResult> TestStreaming1()
        {
            var bytes = _streamingService.GetMovieTrailerBytes(1);
            return File(bytes, "video/mp4");
        }
        #endregion

        [HttpPost("fetchTrailer")]
        public async Task<IActionResult> FetchTrailer(TrailerDTO trailerDTO)
        {
            using var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("TrailerStreamingServiceUrl"), new GrpcChannelOptions
            {
                MaxReceiveMessageSize = int.MaxValue
            });
            var client = new Streaming.StreamingClient(channel);
            var reply = await client.GetMovieTrailerAsync(
                              new TrailerRequest { UserName = trailerDTO.User.UserName, Password = trailerDTO.User.Password, MovieId = trailerDTO.MovieId});
            var bytes = reply.Video.ToByteArray();

            return File(bytes, "video/mp4");
        }
    }

}

