using System.Text.Json.Serialization;
using CatApp.Shared.Entities;

namespace CatApp.Shared
{

    public class CatDto
    {
        // public int Id { get; set; } // local db id

        public string ApiId { get; set; } //remote API id

        public int Width { get; set; }

        public int Height { get; set; }

        public string ImageUrl { get; set; }

        [JsonIgnore]
        public byte[]? Image { get; set; }

        public string ImageHash { get; set; }

        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

        public List<string> Tags { get; set; }

        public CatDto()
        {
            Tags = new List<string>(); // Initialize to avoid null reference
        }
        public CatDto(Cat cat)
        {
            ApiId = cat.ApiId;
            //Id = cat.Id;
            Width = cat.Width;
            Height = cat.Height;
            //Tags = cat.CatTags.Select(ct => ct.Tag).ToList();
            Tags = cat.CatTags.Select(ct => ct.Tag.Name).ToList();
            ImageUrl = cat.ImageUrl;
            //Image = cat.Image;
            ImageHash = cat.ImageHash;
            CreatedOn = cat.CreatedOn;
        }

        [JsonConstructor] // <--- this is for the deserialization that i use in tests !! IMPORTANT
        public CatDto(string apiId, int width, int height, string imageUrl, string imageHash, DateTimeOffset createdOn, List<string> tags)
        {
            ApiId = apiId;
            Width = width;
            Height = height;
            ImageUrl = imageUrl;
            ImageHash = imageHash;
            CreatedOn = createdOn;
            Tags = tags ?? new List<string>();
        }


    }
}
