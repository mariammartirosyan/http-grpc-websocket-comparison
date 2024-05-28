using System;
using CsvHelper.Configuration;
using MovieService.Library.Domain;

namespace MovieService.Library.CsvHelpers
{
	public class MovieMapping: ClassMap<Movie>
	{
		public MovieMapping()
		{
            Map(p => p.Id).Index(0);
            Map(p => p.Title).Index(1);
            Map(p => p.Description).Index(2);
            Map(p => p.Director).Index(3);
            Map(p => p.ReleaseYear).Index(4);
            Map(p => p.YouTubeLink).Index(5);
        }
	}
}

