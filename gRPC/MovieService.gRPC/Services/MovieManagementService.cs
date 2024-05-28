using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using MovieService.gRPC;

namespace MovieService.gRPC.Services;

public class MovieManagementService : MovieManagement.MovieManagementBase
{
    private readonly ILogger<MovieManagementService> _logger;
    private readonly Library.Services.MovieService _movieService;

    public MovieManagementService(Library.Services.MovieService movieService, ILogger<MovieManagementService> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    [Authorize]
    public override Task<GetMovieReply> GetMovieById(GetMovieRequest request, ServerCallContext context)
    {
        var reply = new GetMovieReply() { Succeeded = true };
        try
        {
            var movie = _movieService.GetMovieById(request.Id);
            if (movie != null)
            {
                var movieReply = new Movie()
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Description = movie.Description,
                    Director = movie.Director,
                    ReleaseYear = movie.ReleaseYear,
                    YouTubeLink = movie.YouTubeLink
                };
                foreach (var genre in movie.Genre)
                {
                    movieReply.Genre.Add(new Genre()
                    {
                        Name = genre.Name
                    });

                }
                reply.Movie = movieReply;
            }
            else
            {
                reply.Message = $"Movie with Id-{request.Id} was not found";
                reply.Succeeded = false;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(DateTime.Now + $" - Get Movie with Id = {request.Id}\n" + ex.ToString());
            reply.Message = ex.ToString();
            reply.Succeeded = false;
        }

        return Task.FromResult(reply);
    }
}

