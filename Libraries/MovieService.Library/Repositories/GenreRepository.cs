using System;
using MovieService.Library.Domain;

namespace MovieService.Library.Repositories
{
	public class GenreRepository
	{
        private readonly MovieDbContext dbContext;

        public GenreRepository(MovieDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public Genre GetById(string name)
        {
            return dbContext.Genre.Find(name);
        }
        public IEnumerable<Genre> GetAll()
        {
            return dbContext.Genre.ToList();
        }
        public void Add(Genre genre)
        {
            dbContext.Genre.Add(genre);
            dbContext.SaveChanges();
        }
        public void Delete(Genre genre)
        {
            dbContext.Genre.Remove(genre);
            dbContext.SaveChanges();
        }
    }
}

