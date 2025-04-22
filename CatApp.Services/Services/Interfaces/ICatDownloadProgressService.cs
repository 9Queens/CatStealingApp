using CatApp.Shared.Entities;

namespace CatApp.Services
{
    public interface ICatDownloadProgressService
    {
        /// <summary>
        /// Starts a new hangfire task that will try to capture the given amount of cats async. 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<Guid> StartNewBatchAsync(int count);

        /// <summary>
        /// Do what it says...
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task IncrementProgressAsync(Guid batchId);

        /// <summary>
        /// Do what it says...
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task IncrementBatchFailureAsync(Guid batchId);

        /// <summary>
        /// Do what it says...
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task IncremenFailuresAsync(Guid batchId);

        /// <summary>
        /// Do what it says...
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task IncremenDublicatesAsync(Guid batchId);

        /// <summary>
        /// Also Do what it says....
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task CompleteBatchAsync(Guid batchId);

        /// <summary>
        /// Also Do what it says....
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task AppendInfoMessage(Guid batchId, string infoMessage);


        /// <summary>
        /// Live monitor tracking for the current given batch ID (Guid)
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<CatDownloadProgress?> TrackJobProgressAsync(Guid batchId);


        /// <summary>
        /// Live monitor tracking of all active batched jobs
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<List<CatDownloadProgress>> TrackActiveJobsProgressAsync();

        /// <summary>
        /// Get all completed downloaded batched jobs
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<List<CatDownloadProgress>> TrackCompletedJobsProgressAsync();
    }
}
