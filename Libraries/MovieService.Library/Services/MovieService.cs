using MovieService.Library.Domain;
using MovieService.Library.DTOs;
using MovieService.Library.Repositories;

namespace MovieService.Library.Services
{
    public class MovieService
	{
        private readonly MovieRepository movieRepository;
        private readonly GenreRepository genreRepository;

        public MovieService(MovieRepository movieRepository, GenreRepository genreRepository)
		{
            this.movieRepository = movieRepository;
            this.genreRepository = genreRepository;
        }
        public Movie GetMovieById(int id)
        {
            return movieRepository.GetById(id);
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return movieRepository.GetAll();
        }

		public void InsertMovie(MovieDTO movieDTO)
		{
            Movie movie = new Movie()
            {
                Title = movieDTO.Title,
                Description = movieDTO.Description,
                Director = movieDTO.Director,
                ReleaseYear = movieDTO.ReleaseYear,
                YouTubeLink = movieDTO.YouTubeLink
            };

            movie.Genre = new List<Genre>();
            foreach (var genreDTO in movieDTO.Genre)
            {
                var genre = genreRepository.GetById(genreDTO.Name);
                if (genre == null)
                {
                    genre = new Genre() { Name = genreDTO.Name };
                    genreRepository.Add(genre);
                }

                movie.Genre.Add(genre);
            }

            movieRepository.Add(movie);
        }

        public Movie UpdateMovie(MovieDTO movieDTO)
        {
            var movie = movieRepository.GetById(movieDTO.Id);
            if (movie != null)
            {
                movie.Title = movieDTO.Title;
                movie.Description = movieDTO.Description;
                movie.Director = movieDTO.Director;
                movie.ReleaseYear = movieDTO.ReleaseYear;
                movie.YouTubeLink = movieDTO.YouTubeLink;

                movie.Genre.RemoveAll(x => true);

                foreach (var genreDTO in movieDTO.Genre)
                {
                    var genre = genreRepository.GetById(genreDTO.Name);
                    if (genre == null)
                    {
                        genre = new Genre() { Name = genreDTO.Name };
                        genreRepository.Add(genre);
                    }

                    movie.Genre.Add(genre);
                }

                movieRepository.Update(movie);
               
            }
            return movie;
        }
        public void DeleteMovie(int id)
        {
            var movie = movieRepository.GetById(id);
            if (movie != null)
            {
                movieRepository.Delete(movie);
            }
        }
	}
}

