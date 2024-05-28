using System.Text.Json;
using AccountService.gRPC;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using MovieService.gRPC;
using StatisticsService.gRPC;
using StatisticsService.Library.Services;

namespace StatisticsService.gRPC.Services;

public class StatisticsManagementService : StatisticsManagement.StatisticsManagementBase
{
    private readonly Library.Services.StatisticsService _statisticsService;
    private readonly ILogger<StatisticsManagementService> _logger;

    public StatisticsManagementService(Library.Services.StatisticsService statisticsService, ILogger<StatisticsManagementService> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    [Authorize]
    public override async Task<AddStatisticsReply> AddStatisticsEntry(AddStatisticsRequest request, ServerCallContext context)
    {
        string? token = context.RequestHeaders.FirstOrDefault(x => x.Key == "authorization")?.Value.Substring(7);
        
        AddStatisticsReply reply = new AddStatisticsReply() { Succeeded = true };
        try
        {
            string accountServiceUrl = Environment.GetEnvironmentVariable("AccountServiceUrl");
            string movieServiceUrl = Environment.GetEnvironmentVariable("MovieServiceUrl");

            UserDetailsReply userDetailsReply = null;
            GetMovieReply movieDetailsReply = null;

            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            var accountServiceTask = Task.Run(async () =>
            {
                using var channel = GrpcChannel.ForAddress(accountServiceUrl);
                var client = new AccountManagement.AccountManagementClient(channel);
                userDetailsReply = await client.GetUserDetailsAsync(new UserDetailsRequest { UserName = request.UserName}, headers);
            });

            var movieServiceTask = Task.Run(async () =>
            {
                using var channel = GrpcChannel.ForAddress(movieServiceUrl);
                var client = new MovieManagement.MovieManagementClient(channel);
                movieDetailsReply = await client.GetMovieByIdAsync(new GetMovieRequest { Id = request.MovieId }, headers);
            });

            await Task.WhenAll(accountServiceTask, movieServiceTask);

            if (userDetailsReply != null && userDetailsReply.Succeeded &&
                movieDetailsReply != null && movieDetailsReply.Succeeded)
            {
                var dateTime = DateTime.UtcNow;
                var user = userDetailsReply.User;
                var movie = movieDetailsReply.Movie;

                foreach (var genre in movie.Genre)
                {
                    _statisticsService.Add(new Library.Domain.UserMovieStatistic()
                    {
                        UserId = user.Id,
                        MovieId = movie.Id,
                        Genre = genre.Name,
                        DateTime = dateTime
                    });
                }
            }

            reply.Message = "Statistics entry was added";
            _logger.LogInformation(DateTime.Now + " - " + reply.Message);
        }
        catch (Exception ex)
        {
            reply.Message = ex.ToString();
            reply.Succeeded = false;
            _logger.LogError(DateTime.Now + " - Add Statistics - " + ex.ToString());
        }

        return await Task.FromResult(reply);
    }
}