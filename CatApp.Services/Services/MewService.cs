using System.Text.Json;
using CatApp.Shared;
using CatApp.Shared.APIs.Cats.v1.Responses;
using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using CatApp.Shared.Extensions;
using CatApp.Shared.Helpers;
using CatApp.Shared.Options;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CatApp.Services
{
    public class MewService : IMewService
    {

        private readonly HttpClient _http = new HttpClient();
        // private readonly DataContext _context;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;
        private readonly MewServiceOptions _options;
        public readonly ICatService _catService;
        public readonly ITagService _tagService;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ICatDownloadProgressService _catDownloadProgress;
        private readonly ILogger<MewService> _logger;

        //to notify when a fetch batch or item downloaded with success!!!
        public event Action OnBatchDownloadComplete;
        public event Action OnBatchItemDownloadCompleted;

        public MewService(HttpClient httpClient, IDbContextFactory<DataContext> dbContextFactory, /* DataContext context,*/ IOptions<MewServiceOptions> options, IBackgroundJobClient jobClient, ICatDownloadProgressService catDownloadProgress, ICatService catService, ITagService tagService, ILogger<MewService> logger)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
            // _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _catService = catService ?? throw new ArgumentNullException(nameof(catService));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _catDownloadProgress = catDownloadProgress ?? throw new ArgumentNullException(nameof(_catDownloadProgress));

        }


        public async Task<ScheduledJobResult> CaptureMultipleCats_Job(int count = 25)
        {
            //setting up our mew batch here....
            var batchId = await _catDownloadProgress.StartNewBatchAsync(count);

            //the entire mew batch starts here...
            _jobClient.Enqueue(() => FetchAndStoreMultipleCatsAsync_Job(count, batchId));


            return new ScheduledJobResult
            {
                Success = true,
                JobId =batchId,
                Message = $"Download of {count} cats scheduled. Track progress using batch ID: {batchId}"
            };
        }



        public async Task<CatDownloadProgress?> GetBatchProgressAsync(Guid batchId)
        {
            var batch = await _catDownloadProgress.TrackJobProgressAsync(batchId);

            if (batch != null)
            {
                return new CatDownloadProgress()
                {
                    CatsDownloaded = batch.CatsDownloaded,
                    StartedOn = batch.StartedOn,
                    TotalCats = batch.TotalCats,
                    Id = batchId,
                    CompletedOn = batch.CompletedOn,
                    BatchFailures = batch.BatchFailures,
                    Status = batch.Status,
                    Messages = batch.Messages
                    // IsCompleted = batch.CatsDownloaded >= batch.TotalCats
                };
            }
            else
            {
                return null;
            }

        }


        public async Task<List<CatDownloadProgress>> GetActiveBatchesProgressAsync()
        {
            var batch = await _catDownloadProgress.TrackActiveJobsProgressAsync();

            if (batch != null)
            {
                var progressList = batch.Select(item => new CatDownloadProgress()
                {
                    CatsDownloaded = item.CatsDownloaded,
                    StartedOn = item.StartedOn,
                    TotalCats = item.TotalCats,
                    CompletedOn = item.CompletedOn,
                    // IsCompleted = item.CatsDownloaded >= item.TotalCats // Uncomment if needed
                    BatchFailures = item.BatchFailures,
                    Status = item.Status,
                    Messages = item.Messages

                }).ToList();

                return progressList;
            }
            else
            {
                return new List<CatDownloadProgress>();
            }
        }



        public async Task<List<CatDownloadProgress>> GetCompletedBatchesProgressAsync()
        {
            var batch = await _catDownloadProgress.TrackCompletedJobsProgressAsync();

            if (batch != null)
            {
                var progressList = batch.Select(item => new CatDownloadProgress()
                {
                    CatsDownloaded = item.CatsDownloaded,
                    StartedOn = item.StartedOn,
                    TotalCats = item.TotalCats,
                    CompletedOn = item.CompletedOn,
                    // IsCompleted = item.CatsDownloaded >= item.TotalCats // Uncomment if needed
                    BatchFailures = item.BatchFailures,
                    Status = item.Status,
                    Messages = item.Messages
                }).ToList();

                return progressList;
            }
            else
            {
                return new List<CatDownloadProgress>();
            }
        }



        /// <summary>
        /// Starts whatever can starts in paralle - l if possible -  Light speed fast but - NOT Accurate most of the times -
        /// needs more work ... but --- >  ITS FAST <----
        /// </summary>
        /// <param name="count"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        /// public async Task FetchAndStoreMultipleCatsParallelAsync_Job(int count, Guid batchId)
        /// {
        /// 
        ///     var tasks = Enumerable.Range(0, count).Select(async _ =>
        ///     {
        ///         try
        ///         {
        /// 
        ///             var success = await FetchAndStoreRandomCatAsync_Job();
        ///             Thread.Sleep(240);
        ///             switch (success)
        ///             {
        ///                 case DownloadResultType.Success:
        ///                     await _catDownloadProgress.IncrementProgressAsync(batchId);
        ///                     _logger.LogInformation("-------- Cat successfully downloaded !!");
        ///                     break;
        /// 
        ///                 case DownloadResultType.Dublicate:
        ///                     await _catDownloadProgress.IncremenDublicatesAsync(batchId);
        ///                     _logger.LogWarning("-------- Cat Dublication occured !!");
        ///                     break;
        /// 
        ///                 case DownloadResultType.Error:
        ///                     await _catDownloadProgress.IncremenFailuresAsync(batchId);
        ///                     _logger.LogWarning("-------- Error occured while downloading Cat !!");
        ///                     break;
        ///             }
        /// 
        ///             Thread.Sleep(120);
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             //if you read this....  i saw concurrency issue with hangfire task and i have registered dbcontext
        ///             // using dbcontextFactory .... etc.
        ///             _logger.LogError(ex, "error in FetchAndStoreMultipleCatsParallelAsync_Job - when scheduling a batch fetchi");
        ///             _logger.LogWarning(ex, "A cat failed to fetch");
        ///         }
        ///     });
        /// 
        /// 
        ///     await Task.WhenAll(tasks);
        /// 
        ///     await _catDownloadProgress.CompleteBatchAsync(batchId);
        /// 
        /// }


        /// <summary>
        /// Starts whatever can starts - NOT fast approach but STABLE and ACCURATE
        /// Feel free to spam from the controller (fetch) as much as you like -AT THE SAME TIME - .... can handle the stress at ease.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task FetchAndStoreMultipleCatsAsync_Job(int count, Guid batchId)
        {

            var successCount = 0;
            var retryAttempts = _options.RetryAttempts * count; //adding some retry policy from IOptions

            int attempts = 0;

            while (successCount < count && attempts < count + retryAttempts)
            {
                attempts++;

                try
                {
                    var result = await FetchAndStoreRandomCatAsync_Job();

                    // --- To successfully test the generated logs under failure just uncomment this exception 
                    //throw new Exception("something very MEW happernd when collecting item !!!");

                    switch (result)
                    {

                        case DownloadResultType.Success:
                            successCount++;
                            await _catDownloadProgress.IncrementProgressAsync(batchId);
                            _logger.LogInformation(" Cat successfully downloaded.");
                            break;

                        case DownloadResultType.Dublicate:
                            await _catDownloadProgress.IncremenDublicatesAsync(batchId);
                            _logger.LogWarning("Duplicate cat detected.");
                            break;

                        case DownloadResultType.Error:
                            await _catDownloadProgress.IncremenFailuresAsync(batchId);
                            _logger.LogWarning(" Error while downloading cat.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occured while fetching a single cat  --  (0.o)  MEW !!!");
                    await _catDownloadProgress.IncremenFailuresAsync(batchId);
                }
            }

            await _catDownloadProgress.CompleteBatchAsync(batchId);

            var failedToReachCount = count - successCount;

            if (failedToReachCount > 0)
            {
                await _catDownloadProgress.AppendInfoMessage(batchId, "failed to reach IOptions count for items ");
                _logger.LogWarning($"Batch finished but {failedToReachCount} cats failed to download successfully after retries.");
            }
        }
 
       


        /// <summary>
        /// Scheduled hangfire job that captures and stores one random - UNIQUE cat in the database
        /// </summary>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public Task<ScheduledJobResult> CaptureRandomCatAsync_Job()
        {
            // here i create background job to fetch and store the cat.
            _jobClient.Enqueue(() => FetchAndStoreRandomCatAsync_Job());

            // here i return as requested a dummyy result since the actual cat is added in background.
            return Task.FromResult(new ScheduledJobResult { Success = true, Message = "An unloved cat capture from the api  will start shortly - job schedulred with success" });
        }


        /// <summary>
        /// Scheduled hangfire job that fetch and stores a unique cat
        /// First check if the just retrieved cat image in it's hashed form exists or not in the database.
        /// If not exists , proceeds with the actual storage of the new cat entry, else returns.
        /// </summary>
        /// <returns> A <see cref="DownloadResultType"/> depending on the case (if image exists or not - or error) </returns>
        public async Task<DownloadResultType> FetchAndStoreRandomCatAsync_Job()
        {
            try
            {
                var cat = await FetchRandomCatAsync();

                if (cat == null || cat.Breeds == null || cat.Breeds.Count == 0)
                    return DownloadResultType.Error;


                var catImage = await _http.DownloadAsByteArrayAsync(cat.Url);

                if (catImage == null)
                    return DownloadResultType.Error;

                var catImageHash = ImageHelpers.ComputeSHA256(catImage);

                //check for cat uniqueness in database
                if (await _catService.IsCatImageExist(catImageHash) == false)
                {

                    var result = await StoreCatAsync(cat, catImageHash, catImage);

                    OnBatchItemDownloadCompleted?.Invoke();

                    return DownloadResultType.Success;
                }
                else
                {
                    return DownloadResultType.Dublicate;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occured when trying to download a random cat with messag : {ex.Message}\nStak Trace : {ex.ToString()}");
                return DownloadResultType.Error;
            }

        }


        /// <summary>
        /// Fetches a random cat from the API
        /// </summary>
        /// <returns></returns>
        public async Task<CatApiResponse?> FetchRandomCatAsync()
        {

            //this will fetch an image -- WITH BREED --- info details
            //--- BUT THE RESPONSE WILL NOT CARRY ANY BREED info
            var response = await _http.GetAsync("https://api.thecatapi.com/v1/images/search?has_breeds=1");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            var catList = JsonSerializer.Deserialize<List<CatApiResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //ensuring that the serialized result will not be null so we can collect our mew !!
            if (catList == null)
                return null;

            var cat = catList.FirstOrDefault();

            if (cat == null)
                return null;

            //TO GET the breed info we have to make a call againt  to ---- > "https://api.thecatapi.com/v1/images/{ID_FROM_FIRST_RESPONSE_HERE}"
            var catWithBreed = await _http.GetAsync($"https://api.thecatapi.com/v1/images/{cat.Id}");
            var breedContent = await catWithBreed.Content.ReadAsStringAsync();


            var fullCatWithBreedDetails = JsonSerializer.Deserialize<CatApiResponse>(breedContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return fullCatWithBreedDetails;
        }

        public async Task<bool> StoreCatAsync(CatApiResponse cat, string imageHash, byte[] image)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            if (cat == null || cat.Breeds == null || cat.Breeds.Count == 0)
                return false;

            if (image == null || string.IsNullOrEmpty(imageHash))
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!await _catService.IsCatImageExist(imageHash))
                {
                    var catEntity = new Cat
                    {
                        ApiId = cat.Id,
                        Width = cat.Width,
                        Height = cat.Height,
                        ImageUrl = cat.Url,
                        Image = image,
                        ImageHash = imageHash,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CatTags = new List<CatTag>()
                    };

                    _context.Cats.Add(catEntity);

                    var tagNames = cat.Breeds
                        .Where(b => !string.IsNullOrWhiteSpace(b.Temperament))
                        .SelectMany(b => b.Temperament.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(t => t.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var tagName in tagNames)
                    {
                        // Inlined version of GetOrCreateTagAsync
                        var tagEntity = await _context.Tags
                            .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                        if (tagEntity == null)
                        {
                            tagEntity = new Tag
                            {
                                Name = tagName,
                                Created = DateTimeOffset.UtcNow
                            };

                            _context.Tags.Add(tagEntity);
                            await _context.SaveChangesAsync();
                        }

                        var catTag = new CatTag
                        {
                            Cat = catEntity,
                            Tag = tagEntity
                        };

                        _context.CatTags.Add(catTag);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Rolling back transaction for newly retrieved cat  :  [ {cat?.Url} ]");
                _logger.LogError($"Exception during storing cat: {ex.Message}\nStackTrace: {ex}");

                return false;
            }
        }

        public async Task<CatDto?> GetLocalCatByIdAsync(int id)
        {
            var cat = await _catService.GetCatAsync(id);

            if (cat != null)
                return new CatDto(cat);

            else
                return null;
        }
        public async Task<List<CatDto>> GetLocalCatsPagedAsync(int page, int pageSize)
        {

            var cats = await _catService.GetCatsAsync(page, pageSize);

            return cats.ToList();

        }
        public async Task<List<CatDto>> GetLocalCatsByTagAsync(List<string> tags, int page, int pageSize)
        {

            var cats = await _catService.GetCatsByTag(tags, page, pageSize);

            return cats.ToList();
        }


        // -------------------- TODO THESE 3 later 
        public async Task<CatDto?> GetRemoteCatByIdAsync(string id, bool includeImage = false)
        {

            throw new NotImplementedException();

        }

        public Task<List<CatDto>> GetRemoteCatsPagedAsync(int page, int pageSize, bool includeImage = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<CatDto>> GetRemoteCatsByTagAsync(string tag, int page, int pageSize, bool includeImage = false)
        {
            throw new NotImplementedException();
        }


        // -------------------- TODO THESE 3 later 

    }
}
