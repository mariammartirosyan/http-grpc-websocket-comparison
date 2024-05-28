using System;
using CsvHelper.Configuration;
using MovieService.Library.Domain;

namespace MovieService.Library.CsvHelpers
{
	public class MovieGenreMapping: ClassMap<MovieGenre>
    {
		public MovieGenreMapping()
		{
            Map(p => p.MovieId).Index(0);
            Map(p => p.GenreName).Index(1);
        }
	}
}

