using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrailerStreamingService.API.DTOs;
using TrailerStreamingService.Library.Services;

namespace TrailerStreamingService.API.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    public class TrailerController : ControllerBase
    {
        private readonly StreamingService _streamingService;
        private readonly ILogger<TrailerController> _logger;


        public TrailerController(StreamingService streamingService, ILogger<TrailerController> logger)
        {
            _streamingService = streamingService;
            _logger = logger;
        }

        [HttpPost("fetchTrailer")]
        public async Task<IActionResult> GetMovieTrailer(TrailerDTO trailerDTO)
        {
            try
            {
                HttpClient client = new HttpClient();
                var content = JsonSerializer.Serialize(trailerDTO.User);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                //var response = await client.PostAsync(Environment.GetEnvironmentVariable("AccountServiceUrl")+"/Account/login", httpContent);
                var response = await client.PostAsync(Environment.GetEnvironmentVariable("AccountServiceUrl") + "/login", httpContent);
                var responseMsg = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var token = responseMsg;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    content = JsonSerializer.Serialize(new { UserName = trailerDTO.User.UserName, MovieId = trailerDTO.MovieId });
                    httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                    //response = await client.PostAsync(Environment.GetEnvironmentVariable("StatisticsServiceUrl")+ "/Statistics", httpContent);
                    response = await client.PostAsync(Environment.GetEnvironmentVariable("StatisticsServiceUrl") + "/addStatisticsEntry", httpContent); 

                    _logger.LogInformation($"Request for movie ID = {trailerDTO.MovieId} is being processed");
                    var fileStream = _streamingService.GetMovieTrailer(trailerDTO.MovieId);
                    if (fileStream != null)
                    {
                        _logger.LogInformation($"The movie trailer with movie ID = {trailerDTO.MovieId} is successfully fetched");


                        return new FileStreamResult(fileStream, "video/mp4");
                    }
                    _logger.LogInformation($"The movie trailer with movie ID = {trailerDTO.MovieId} was not found");
                    return NotFound();
                }

                return StatusCode((int)response.StatusCode, responseMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error - {ex.ToString()}");
                return StatusCode(500, ex.ToString());
            }
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}

