namespace CatApp.Shared.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Gets the image as <see cref="byte[]"/> from location.
        /// <para>
        /// Returns :
        /// </para>
        /// <para>- Either the actual image in byte []</para>
        /// <para>- Either an empty byte[] array (under exception) </para> 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<byte[]> DownloadAsByteArrayAsync(this HttpClient httpClient, string url)
        {

            try
            {

                using (var imageStream = await httpClient.GetStreamAsync(url))
                {
                    using (var memStream = new MemoryStream())
                    {
                        await imageStream.CopyToAsync(memStream);
                        await memStream.FlushAsync();
                        return memStream.Length == 0 ? Array.Empty<byte>() : memStream.ToArray();
                    }
                }

            }
            catch
            {
                Console.WriteLine("MEW!!! :(  ,  cat image lost in transfer or not present !!! ");
                return Array.Empty<byte>();
            }
        }
    }
}
