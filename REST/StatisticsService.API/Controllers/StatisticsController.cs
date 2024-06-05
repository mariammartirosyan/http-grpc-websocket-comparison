using System;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatisticsService.API.DTOs;
using StatisticsService.API.Helpers;
using StatisticsService.Library.Services;

namespace StatisticsService.API.Controllers
{
    [Authorize]
    [ApiController]
    //[Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly Library.Services.StatisticsService statisticService;
        private readonly ILogger<StatisticsController> logger;

        public StatisticsController(Library.Services.StatisticsService statisticService, ILogger<StatisticsController> logger)
		{
            this.statisticService = statisticService;
            this.logger = logger;
        }
        [HttpPost("addStatisticsEntry")]
        public async Task<IActionResult> Add(InsertStatisticsDTO statisticsDTO)
        {
            try
            {
                string accountServiceUrl = Environment.GetEnvironmentVariable("AccountServiceUrl");
                string movieServiceUrl = Environment.GetEnvironmentVariable("MovieServiceUrl");
                Response accountServiceResponse = null;
                Response movieServiceResponse = null;

                var headers = HttpContext.Request.Headers;
                var accountServiceTask = Task.Run(async () =>
                {
                    accountServiceResponse = await HttpRequestHelper.SendGetRequest($"{accountServiceUrl}/getUserDetails?userName={statisticsDTO.UserName}", headers);
                });

                var movieServiceTask = Task.Run(async () =>
                {
                    movieServiceResponse = await HttpRequestHelper.SendGetRequest($"{movieServiceUrl}/movie/{statisticsDTO.MovieId}", headers);

                });

                await Task.WhenAll(accountServiceTask, movieServiceTask);

                if (accountServiceResponse != null && accountServiceResponse.IsSuccessful &&
                    movieServiceResponse != null && movieServiceResponse.IsSuccessful)
                {
                    var dateTime = DateTime.UtcNow;
                    var user = JsonSerializer.Deserialize<UserDTO>(accountServiceResponse.Message);
                    var movie = JsonSerializer.Deserialize<MovieDTO>(movieServiceResponse.Message);

                    foreach (var genre in movie.Genre)
                    {
                        statisticService.Add(new Library.Domain.UserMovieStatistic()
                        {
                            UserId = user.Id,
                            MovieId = movie.Id,
                            Genre = genre.Name,
                            DateTime = dateTime
                        });
                    }
                }
                logger.LogInformation(DateTime.Now + " - Statistics entry was added");
                return Ok();
            }
            catch(Exception ex)
            {
                logger.LogError(DateTime.Now + " - StatisticsService - " + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
            
        }
	}
}

