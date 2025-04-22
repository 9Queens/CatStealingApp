using CatApp.Shared;
using CatApp.Shared.APIs.Cats.v1.Responses;

using CatApp.Shared.Entities;

namespace CatApp.Services
{

    /// <summary>
    /// The Service that will fetch cats from the  https://thecatapi.com and stores them locally
    /// </summary>
    public interface IMewService
    {

        // ------------------------------------------------------------------------------------------------------   fetches locally stored cats 
        Task<CatDto?> GetLocalCatByIdAsync(int id);
        Task<List<CatDto>> GetLocalCatsPagedAsync(int page, int pageSize);
        Task<List<CatDto>> GetLocalCatsByTagAsync(List<string> tags, int page, int pageSize);
        // ------------------------------------------------------------------------------------------------------  



        // ------------------------------------------------------------------------------------------------------   Remote stored cats From the API 
        Task<CatDto?> GetRemoteCatByIdAsync(string id, bool includeImage = false);
        Task<List<CatDto>> GetRemoteCatsPagedAsync(int page, int pageSize, bool includeImage = false);
        Task<List<CatDto>> GetRemoteCatsByTagAsync(string tag, int page, int pageSize, bool includeImage = false);
        // ------------------------------------------------------------------------------------------------------  

        /// <summary>
        /// Gets a random cat from the API
        /// </summary>
        /// <returns></returns>
        Task<CatApiResponse?> FetchRandomCatAsync();

        /// <summary>
        /// Stores the cat in database  - ONLY - if the hash of the cat's images is not exists in db !!
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        Task<bool> StoreCatAsync(CatApiResponse cat, string imageHash, byte[] image);

        /// <summary>
        /// Schedule an individual job that will fetch and store a random cat from the API
        /// </summary>
        /// <returns></returns>
        Task<ScheduledJobResult> CaptureRandomCatAsync_Job();

        /// <summary>
        /// Serves the download batch progress to the caller
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<CatDownloadProgress?> GetBatchProgressAsync(Guid batchId);


        /// <summary>
        /// Serves the download batch progress to the caller
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<List<CatDownloadProgress>> GetActiveBatchesProgressAsync();

        /// <summary>
        /// Serves the download batch progress to the caller
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<List<CatDownloadProgress>> GetCompletedBatchesProgressAsync();

        /// <summary>
        /// Starts the download  - MEW - batch in parallel for the given amount of kittens.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task FetchAndStoreMultipleCatsAsync_Job(int count, Guid batchId);


        /// <summary>
        /// Starts the a schedule job for downloading a cat batch async from the Cats API.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<ScheduledJobResult> CaptureMultipleCats_Job(int count = 25);
    }

}
