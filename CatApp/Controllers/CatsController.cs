using CatApp.Services;
using CatApp.Shared;
using CatApp.Shared.APIs.Local.v1.Responses;
using CatApp.Shared.Entities;
using CatApp.Shared.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly IMewService _mewService;
        private readonly MewServiceOptions _options;
        private readonly ILogger<CatsController> _logger;

        public CatsController(IMewService mewService, IOptions<MewServiceOptions> options, ILogger<CatsController> logger)
        {
            _mewService = mewService ?? throw new ArgumentNullException(nameof(mewService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a single cat by its ID.
        /// GET /api/cat/{id}
        /// </summary>
        [ProducesResponseType(typeof(ApiResponse<CatDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatDto>), StatusCodes.Status500InternalServerError)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CatDto?>>> GetCatById(int id)
        {
            try
            {
                var serviceResponse = new ApiResponse<string>();

                var cat = await _mewService.GetLocalCatByIdAsync(id);
                if (cat == null)
                {
                    return ApiResponse<CatDto?>.NotFound(null, "Mew, no one loved that cat :(");
                }

                return ApiResponse<CatDto?>.Ok(cat, "Mew! (>^-^)>  a cat is loved! ");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving  GetCatById({id}) with message : \n{ex.Message} ");
                return ApiResponse<CatDto?>.Fail("Mew :( -  An unexpected error occurred");

            }

        }


        /// <summary>
        /// Get paginated list of cats.
        /// GET /api/cat/all?page=1&pageSize=10
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse<List<CatDto?>>> GetCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {

            // Validate inputs (optional)
            if (page <= 0 || pageSize <= 0)
            {
                return ApiResponse<List<CatDto?>>.Fail("Mew!  >.<   Page and pageSize must be greater than 0.");
            }

            if (pageSize > 20)
                pageSize = _options.MaxPageSize;


            try
            {
                var cats = await _mewService.GetLocalCatsPagedAsync(page, pageSize);

                if (cats == null || cats.Count == 0)
                    return ApiResponse<List<CatDto?>>.NotFound(null!, "Mew, no loved cats found :(");
                else
                    return ApiResponse<List<CatDto?>>.Ok(cats!, $"Mew!  (>^-^)>  a bounch of cats are feeling loved !  (max page size set to  {_options.MaxPageSize} - according to settings )  ");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving  GetCats() with message : \n{ex.Message}");
                return ApiResponse<List<CatDto?>>.Fail("Mew :( -  An unexpected error occurred");
            }
        }


        /// <summary>
        /// Get cats filtered by tags (comma-separated).
        /// GET /api/cats?page=1&pageSize=10    --- No filter selected
        /// GET /api/cat/by-tags?tag=cute,fluffy&page=1&pageSize=10   --- With filter selected
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDto?>>), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse<List<CatDto?>>> GetCatsByTag([FromQuery] string? tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {

                // Validate inputs (optional)
                if (page <= 0 || pageSize <= 0)
                {
                    return ApiResponse<List<CatDto?>>.Fail("Mew!  >.<   Page and pageSize must be greater than 0.");
                }

                if (pageSize > 20)
                    pageSize = _options.MaxPageSize;


                var cats = new List<CatDto>();

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var tags = tag.Split(',').ToList();

                    cats = await _mewService.GetLocalCatsByTagAsync(tags, page, pageSize);
                }
                else
                {
                    cats = await _mewService.GetLocalCatsPagedAsync(page, pageSize);
                }

                if (cats != null && cats.Count > 0)
                    return ApiResponse<List<CatDto?>>.Ok(cats!, $"Mew! <(^O^<)  a bounch of cats with awesome temperments are loved! ( max page size set to {_options.MaxPageSize} - according to settings ) ");


                return ApiResponse<List<CatDto?>>.NotFound(new List<CatDto?>(), "Mew!  :(    No loved cats found.... ");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving   GetCatsByTag() with message : \n{ex.Message}");
                return ApiResponse<List<CatDto?>>.Fail("Mew :( -  An unexpected error occurred");
            }
        }

        /// <summary>
        /// Start downloading a batch of cats from TheCatAPI.
        /// POST /api/cat/fetch?size=0  ->  size will take default as specified by IOption settings
        /// POST /api/cat/fetch?size=50 ->  size of 50 will be used
        /// </summary>
        [HttpPost("fetch")]
        [ProducesResponseType(typeof(ApiResponse<ScheduledJobResult?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ScheduledJobResult?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ScheduledJobResult?>), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse<ScheduledJobResult?>> FetchCats([FromQuery] int size = 0)
        {
            try
            {
                if (size == 0)
                    size = _options.DefaultBatchSize; //default selected if not supplied at all

                if (size <= _options.MinBatchSize || size > _options.MaxBatchSize)
                    return ApiResponse<ScheduledJobResult?>.Fail($"Mew >.< - Size must be between {_options.MinBatchSize} and {_options.MaxBatchSize}. ( According to MewOptions provided )");


                var result = await _mewService.CaptureMultipleCats_Job(size);
                return ApiResponse<ScheduledJobResult?>.Ok(result, "Mew job - calling cat-stealing ninja to do the job  (>-<)  !!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving   FetchCats() with message : \n{ex.Message}");
                return ApiResponse<ScheduledJobResult?>.Fail("Mew :( -  An unexpected error occurred");
            }
        }


        /// <summary>
        /// Get live  progress  informatio and error statistics for the given job id (batch).
        /// GET /api/cat/jobs/{batchId}
        /// </summary
        [ProducesResponseType(typeof(ApiResponse<CatDownloadProgressDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatDownloadProgressDto?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatDownloadProgressDto?>), StatusCodes.Status500InternalServerError)]
        [HttpGet("jobs/{batchId:guid}")]
        public async Task<ApiResponse<CatDownloadProgressDto?>> GetBatchProgress(Guid batchId)
        {
            try
            {
                var batch = await _mewService.GetBatchProgressAsync(batchId);

                if (batch == null)
                    return ApiResponse<CatDownloadProgressDto?>.NotFound(null, $"Mew :( - No cat job exists for this Id");

                var downloadProgDto = new CatDownloadProgressDto(batch);

                return ApiResponse<CatDownloadProgressDto?>.Ok(downloadProgDto, "Mew job - Cat stealing ninja is on the move ....  !!");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving   FetchCats() with message : \n{ex.Message}");
                return ApiResponse<CatDownloadProgressDto?>.Fail("Mew :( -  An unexpected error occurred");
            }
        }


        /// <summary>
        /// Get live  progress  information and error statistics for all active and running stealing tasks (returns only downloading jobs).
        /// GET /api/cat/jobs/active
        /// </summary
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status500InternalServerError)]
        [HttpGet("jobs/active")]
        public async Task<ApiResponse<List<CatDownloadProgressDto?>>> GetActiveBatchesProgress(  /* Mew Mew Mew */  )
        {
            try
            {
                var activeJobs = await _mewService.GetActiveBatchesProgressAsync();

                if (activeJobs == null || activeJobs.Count == 0)
                    return ApiResponse<List<CatDownloadProgressDto?>>.NotFound(new List<CatDownloadProgressDto?>(), $"Mew :( - No cat stealing actions on the move at this moment");

                var downloadProgDto = activeJobs.Select(job => new CatDownloadProgressDto(job)).ToList();

                return ApiResponse<List<CatDownloadProgressDto?>>.Ok(downloadProgDto!, "Mew job - Cat stealing ninja is on the move ....  !!");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving   GetActiveBatchesProgress() with message : \n{ex.Message}");
                return ApiResponse<List<CatDownloadProgressDto?>>.Fail("Mew :( -  An unexpected error occurred");
            }
        }


        /// <summary>
        /// Get live  progress  information and error statistics for all active and running stealing tasks (returns only completed jobs).
        /// GET /api/cat/jobs/completed
        /// </summary
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<CatDownloadProgressDto?>>), StatusCodes.Status500InternalServerError)]
        [HttpGet("jobs/completed")]
        public async Task<ApiResponse<List<CatDownloadProgressDto?>>> GetCompletedBatchesProgress(  /* Mew Mew Mew */  )
        {
            try
            {
                var activeJobs = await _mewService.GetCompletedBatchesProgressAsync();

                if (activeJobs == null || activeJobs.Count == 0)
                    return ApiResponse<List<CatDownloadProgressDto?>>.NotFound(new List<CatDownloadProgressDto?>(), $"Mew :( -  cat stealing not completed yet or not available at all");

                var downloadProgDto = activeJobs.Select(job => new CatDownloadProgressDto(job)).ToList();

                return ApiResponse<List<CatDownloadProgressDto?>>.Ok(downloadProgDto!, "Mew job - A bounch of cat stealing operations have been completed with success !!");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went really MEW when serving   GetActiveBatchesProgress() with message : \n{ex.Message}");
                return ApiResponse<List<CatDownloadProgressDto?>>.Fail("Mew :( -  An unexpected error occurred");
            }
        }


    }
}
