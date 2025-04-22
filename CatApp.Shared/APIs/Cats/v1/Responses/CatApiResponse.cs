namespace CatApp.Shared.APIs.Cats.v1.Responses
{
    /// <summary>
    /// Response for the Remote Cat api not the local of the demo !!!
    /// </summary>
    public class CatApiResponse
    {
        public required string Id { get; set; }
        public required string Url { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }

        public List<CatBreedInfo> Breeds { get; set; } = new();
    }
}
