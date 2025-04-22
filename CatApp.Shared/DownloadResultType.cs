namespace CatApp.Shared
{
    /// <summary>
    /// Just an enum to help us switch the download result of each cat item individually more clearly
    /// </summary>
    public enum DownloadResultType
    {
        Success = 0,
        Error = 1,
        Dublicate = 2
    }
}
