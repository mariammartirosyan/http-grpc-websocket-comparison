using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Library.DTOs;

namespace MovieService.API.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly Library.Services.MovieService movieService;
        private readonly ILogger<MovieController> logger;

        public MovieController(Library.Services.MovieService movieService, ILogger<MovieController> logger)
        {
            this.movieService = movieService;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var movies = movieService.GetAllMovies();

                if (movies != null)
                {
                    return Content(JsonSerializer.Serialize(movies), "application/json");
                }
                return StatusCode(404, "No movies were found");
            }
            catch (Exception ex)
            {
                logger.LogError("MovieService: " + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        { 
            try
            {
                var movie = movieService.GetMovieById(id);

                if (movie != null)
                {
                    return Content(JsonSerializer.Serialize(movie), "application/json");
                }
                return StatusCode(404, $"Movie with Id-{id} was not found");
            }
            catch (Exception ex)
            {
                logger.LogError("MovieService: " + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Insert([FromBody] MovieDTO movieDTO)
        {
            try
            {
                movieService.InsertMovie(movieDTO);
                return StatusCode(200, "Movie is added");
            }
            catch(Exception ex)
            {
                logger.LogError("MovieService: " + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public IActionResult Update([FromBody] MovieDTO movieDTO)
        {
            try
            {
                var movie = movieService.UpdateMovie(movieDTO);
                if (movie != null)
                {
                    return StatusCode(200, $"Movie with Id-{movieDTO.Id} is updated");
                }
                return StatusCode(404, $"Movie with Id-{movieDTO.Id} was not found");
            }
            catch (Exception ex)
            {
                logger.LogError("MovieService: " + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                movieService.DeleteMovie(id);
                return StatusCode(200, $"Movie with Id-{id} is deleted");
            }
            catch (Exception ex)
            {
                logger.LogError("MovieService: " + ex.ToString());
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

