namespace MovieService.Library.Domain
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual List<Genre> Genre { get; set; }
        public string Director { get; set; }
        public int ReleaseYear { get; set; }
        public string YouTubeLink { get; set; }
    }
}

