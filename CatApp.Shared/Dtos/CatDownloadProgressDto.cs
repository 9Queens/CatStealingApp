namespace CatApp.Shared.Entities
{
    public class CatDownloadProgressDto
    {
        public Guid Id { get; set; }
        public int BatchSize { get; set; }

        public int Downloaded { get; set; }

        public int DoublicatesOccured { get; set; }

        public int ErrorsOccured { get; set; }

        public DateTimeOffset StartedOn { get; set; } = DateTime.UtcNow;

        public DateTimeOffset? CompletedOn { get; set; }

        public bool IsCompleted { get; set; }
        public Shared.Status? Status { get; set; }

        public int? BatchFailedAttemptes { get; set; }
        public string Information { get; set; }

        public CatDownloadProgressDto(CatDownloadProgress downloadProgress)
        {
            Id = downloadProgress.Id;
            BatchSize = downloadProgress.TotalCats;
            Downloaded = downloadProgress.CatsDownloaded;
            StartedOn = downloadProgress.StartedOn;
            CompletedOn = downloadProgress.CompletedOn ?? null;
            DoublicatesOccured = downloadProgress.DoublicatesOccured;
            ErrorsOccured = downloadProgress.ErrorsOccured;

            Status = downloadProgress.Status ?? null;
            IsCompleted = downloadProgress.CatsDownloaded >= downloadProgress.TotalCats;

            Status = downloadProgress.Status ?? null;
            BatchFailedAttemptes = downloadProgress.BatchFailures;
            Information = downloadProgress.Messages ?? null;

        }
    }
}
