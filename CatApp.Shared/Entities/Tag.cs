using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CatApp.Shared.Entities
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public ICollection<CatTag> CatTags { get; set; } = new List<CatTag>();
    }
}
