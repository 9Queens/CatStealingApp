using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CatApp.Shared.Entities
{
    public class Cat
    {
        [Key]
        public int Id { get; set; } // local db id

        [Required]
        public required string ApiId { get; set; }  // id from https://thecatapi.com

        [Required]
        public required int Width { get; set; }
        [Required]
        public required int Height { get; set; }

        [Required]
        public required string ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "varbinary(max)")]
        public byte[]? Image { get; set; }

        [Required]
        [MaxLength(64)]
        public required string ImageHash { get; set; }

        [Required]
        public required DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [JsonIgnore]
        public ICollection<CatTag> CatTags { get; set; } = new List<CatTag>();   // Navigation property

    }
}
