using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MovieService.Library.Domain
{
    public class Genre
	{
        [Key]
        public string Name { get; set; }
        [JsonIgnore]
        public virtual List<Movie> Movie { get; } = new List<Movie>();
    }
}

