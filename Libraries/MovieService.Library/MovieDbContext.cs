using System;
using System.Data;
using System.Net;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using MovieService.Library.CsvHelpers;
using MovieService.Library.Domain;

namespace MovieService.Library
{
    public class MovieDbContext : DbContext
    {
        public DbSet<Movie> Movie { get; set; }
        public DbSet<Genre> Genre { get; set; }

        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseLazyLoadingProxies();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var genresPath = Path.Combine(baseDirectory, "Seed Data", "Genres.csv");
            var moviesPath = Path.Combine(baseDirectory, "Seed Data", "Movies.csv");
            var movieGenresPath = Path.Combine(baseDirectory, "Seed Data", "MovieGenre.csv");

            modelBuilder.Entity<Genre>().HasData(CsvDataReader<Genre, GenreMapping>.GetData(genresPath));

            var moviesSeedData = CsvDataReader<Movie, MovieMapping>.GetData(moviesPath);
            modelBuilder.Entity<Movie>().HasData(moviesSeedData);
            modelBuilder.Entity<Movie>().HasMany(x => x.Genre).WithMany(x => x.Movie).UsingEntity(j => j
                    .HasData(CsvDataReader<MovieGenre, MovieGenreMapping>.GetData(movieGenresPath)));

            //modelBuilder.Entity<Genre>().HasData(CsvDataReader<Genre, GenreMapping>.GetData("Seed Data/Genres.csv"));

            //var moviesSeedData = CsvDataReader<Movie, MovieMapping>.GetData("Seed Data/Movies.csv");
            //modelBuilder.Entity<Movie>().HasData(moviesSeedData);
            //modelBuilder.Entity<Movie>().HasMany(x => x.Genre).WithMany(x => x.Movie).UsingEntity(j => j
            //        .HasData(CsvDataReader<MovieGenre, MovieGenreMapping>.GetData("Seed Data/MovieGenre.csv")));
        }
    }
}

