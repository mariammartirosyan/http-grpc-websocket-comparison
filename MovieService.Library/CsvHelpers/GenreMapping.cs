using System;
using CsvHelper.Configuration;
using MovieService.Library.Domain;

namespace MovieService.Library.CsvHelpers
{
	public class GenreMapping: ClassMap<Genre>
    {
		public GenreMapping()
		{
            Map(x => x.Name).Index(0);
        }
	}
}

