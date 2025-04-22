namespace CatApp.Shared.Options
{
    public class MewServiceOptions
    {

        //max retry attempts (per unloved cat)
        public int RetryAttempts { get; set; } = 2;

        //time needit for cat to feel loved ( cat should be unloved )
        public int IntervalBetweenFailedAttempts = 1000;

        //sleep intervals between each seperete  ( just for the demonstration so to have enough time to see the status times )
        public int DownloadInteval { get; set; } = 1000; // in ms

        public int MinBatchSize { get; set; }

        public int MaxBatchSize { get; set; }

        public int DefaultBatchSize { get; set; }

        public int PageSize { get; set; } = 10;

        public int MaxPageSize { get; set; } = 30;

    }

}
