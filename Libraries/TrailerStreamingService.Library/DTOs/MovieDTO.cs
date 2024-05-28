using System;
namespace TrailerStreamingService.Library.DTOs
{
	public class MovieDTO
	{
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<GenreDTO> Genre { get; set; }
        public string Director { get; set; }
        public int ReleaseYear { get; set; }
        public string YouTubeLink { get; set; }
    }
}

