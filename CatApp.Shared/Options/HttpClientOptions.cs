namespace CatApp.Shared.Options
{
    public class HttpClientOptions
    {

        public string ServerUrl { get; set; } = string.Empty;
        public List<HeaderValue> HeadersAndValues { get; set; } = new List<HeaderValue>();

    }

    public class HeaderValue
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}
