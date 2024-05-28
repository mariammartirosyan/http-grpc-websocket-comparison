using System;
using Microsoft.EntityFrameworkCore;
using MovieService.Library.Domain;

namespace MovieService.Library.Repositories
{
	public class MovieRepository
	{
        private readonly MovieDbContext dbContext;

        public MovieRepository(MovieDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public Movie GetById(int id)
        {
            return dbContext.Movie.Find(id);
        }
        public IEnumerable<Movie> GetAll()
        {
            return dbContext.Movie.ToList();
        }
        public void Add(Movie movie)
        {
            dbContext.Movie.Add(movie);
            dbContext.SaveChanges();
        }
        public void Update(Movie movie)
        {
            dbContext.Movie.Update(movie);
            dbContext.SaveChanges();
        }
        public void Delete(Movie movie)
        {
            dbContext.Movie.Remove(movie);
            dbContext.SaveChanges();
        }
    }
}

