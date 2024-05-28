using System;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using AccountService.gRPC;
using StatisticsService.gRPC;
using System.Reflection.PortableExecutable;

namespace TrailerStreamingService.gRPC.Services
{
	public class StreamingService : Streaming.StreamingBase
	{
        private readonly Library.Services.StreamingService _streamingService;
        private readonly ILogger<StreamingService> _logger;

        public StreamingService(Library.Services.StreamingService streamingService, ILogger<StreamingService> logger)
		{
            _streamingService = streamingService;
            _logger = logger;
        }

        public override async Task<TrailerReply> GetMovieTrailer(TrailerRequest request, ServerCallContext context)
        {
            TrailerReply reply = new TrailerReply() { Succeeded = true };
            try
            {

                using var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("AccountServiceUrl"));
                var client = new AccountManagement.AccountManagementClient(channel);
                var loginReply = await client.LoginAsync(
                                  new LoginRequest { UserName = request.UserName, Password = request.Password });

                if (loginReply.Succeeded)
                {
                    var headers = new Metadata();
                    var token = loginReply.Message;
                    headers.Add("Authorization", $"Bearer {token}");

                    using var statisticsChannel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("StatisticsServiceUrl"));
                    var statisticsClient = new StatisticsManagement.StatisticsManagementClient(statisticsChannel);
                    var statisticsReply = await statisticsClient.AddStatisticsEntryAsync(
                                      new AddStatisticsRequest { UserName = request.UserName, MovieId = request.MovieId }, headers);

                    var bytes = _streamingService.GetMovieTrailerBytes(request.MovieId);
                    if (bytes != null)
                    {
                        reply.Video = ByteString.CopyFrom(bytes);
                        reply.Message = $"The movie trailer with movie ID = {request.MovieId} is successfully fetched";
                        _logger.LogInformation(reply.Message);
                    }
                    else
                    {
                        reply.Succeeded = false;
                        reply.Message = $"The movie trailer with movie ID = {request.MovieId} was not found";
                        _logger.LogInformation(reply.Message);
                    }
                }
                else
                {
                    reply.Succeeded = false;
                    reply.Message = loginReply.Message;
                }

            }
            catch(Exception ex)
            {
                reply.Succeeded = false;
                reply.Message = ex.ToString();
                _logger.LogError(DateTime.Now + " - Get Movie Trailer - " + ex.ToString());
            }
            return await Task.FromResult(reply);
        }
    }
}

