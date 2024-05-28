namespace TrailerStreamingService.Library.Services
{
    public class StreamingService
	{
		public FileStream GetMovieTrailer(int id)
		{
            var trailerPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Trailers/{id}.mp4";
            if (File.Exists(trailerPath))
            {
                return new FileStream(trailerPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            return null;
        }
        public byte[] GetMovieTrailerBytes(int id)
        {
            var trailerPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Trailers/{id}.mp4";
            if (File.Exists(trailerPath))
            {
               return File.ReadAllBytes(trailerPath);
            }
            return null;
        }
    }
}

