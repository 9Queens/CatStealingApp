using System.Text;
using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatApp.Services.Services
{
    /// <summary>
    /// Just another concrete service implementation with the ability to collect <see cref="Hangfire"/> 
    /// failed and success jobs for tracking.
    /// 
    /// I use calculations for failed error or dublicate download attempts by hand - BUT  - i use Hangfire's automated filters 
    /// To hook the events for the batches   ---  So i can mark an entire download act as sucessful or failed.
    /// 
    /// </summary>
    public class CatDownloadProgressService : ICatDownloadProgressService, IApplyStateFilter, IElectStateFilter
    {
        //private readonly DataContext _context;
        // private readonly CatServiceOptions _options;
        private readonly ILogger<CatDownloadProgressService> _logger;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;


        public CatDownloadProgressService(/*DataContext context, */IDbContextFactory<DataContext> dbContextFactory, ILogger<CatDownloadProgressService> logger)
        {
            //_context = context ?? throw new ArgumentNullException(nameof(context));
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //var methodArgs = context.BackgroundJob.Job.Args;

            //if (methodArgs.Count > 0 && methodArgs[0] is Guid progressId)
            //{
            //    try
            //    {
            //        using var db = _dbContextFactory.CreateDbContext();

            //        // Fetch the progress entry based on the progressId
            //        var progress = db.CatDownloadProgresses.FirstOrDefault(p => p.Id == progressId);
            //        if (progress != null)
            //        {
            //            if (context.NewState is FailedState failedState)
            //            {
            //                // Handling failure state
            //                if (progress.BatchFailures == null)
            //                    progress.BatchFailures = 0;

            //                progress.BatchFailures++;
            //                progress.Messages += $"\n[Failure] for batch {progress.Id} {DateTimeOffset.UtcNow}: {failedState.Exception?.Message}\n";

            //                // Set status to 'Failed' after 3 failures
            //                if (progress.BatchFailures >= 3)
            //                    progress.Status = Shared.Status.Falied;
            //            }
            //            else if (context.NewState is SucceededState)
            //            {
            //                // Handling success state
            //               // progress.CatsDownloaded++;
            //                progress.Messages += $"\n[Success] Job completed successfully for batch {progress.Id} at {DateTimeOffset.UtcNow}\n";

            //                // Optionally set the status to 'Completed' after success
            //                progress.Status = Shared.Status.Success;
            //            }

            //            // Save changes to the database
            //            await db.SaveChangesAsync();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error updating CatDownloadProgress after job state change.");
            //    }
            //}
        }


        /// <summary>
        /// A further improvement for this would be to pass IOptions, set the failure attribute high -- BUT -- have a hangfire job
        /// termnated here after the failed attempts directly from the IOptions.... for now due to speed i do it hardcode it.
        /// 
        /// 
        /// To test this --->  just turn of the internet and set perform a fetch operation through the Api.
        /// You will see the Messages of the  CatDownloadProgress  entries to be adding gradually untill the maximum attempts that i have set
        /// is finally reached ---> inside this function im killing hangfire task by hand.
        /// 
        /// </summary>
        /// <param name="context"></param>
        public async void OnStateElection(ElectStateContext context)
        {
            var methodArgs = context.BackgroundJob.Job.Args;

            if (methodArgs.Count > 0 && methodArgs[1] is Guid progressId)
            {
                try
                {
                    using var db = _dbContextFactory.CreateDbContext();

                    // Fetch the progress entry based on the progressId
                    var progress = db.CatDownloadProgresses.FirstOrDefault(p => p.Id == progressId);
                    if (progress != null)
                    {
                        if (context.CandidateState is FailedState failedState)
                        {
                            // Handling failure state
                            if (progress.BatchFailures == null)
                                progress.BatchFailures = 0;

                            progress.BatchFailures++;
                            progress.Messages += $"\n[Failure] {progress.BatchFailures} / 3 for batch {progress.Id} {DateTimeOffset.UtcNow}: {failedState.Exception?.Message}";

                            _logger.LogWarning($" >>>>>>> Scheduled job {nameof(CatDownloadProgress)} with Id : {progress.Id} ---- failed : {progress.BatchFailures} ");
                            // Set status to 'Failed' after 3 failures
                            if (progress.BatchFailures >= 3)
                            {
                                progress.Status = Shared.Status.Falied;


                                //not the best way to delete a scheduled job but will do the trick for now...
                                context.CandidateState = new DeletedState
                                {
                                    Reason = $"Exceeded failure threshold (3 failures)"
                                };

                                _logger.LogWarning($" >>>>>>>  Manually deleting {nameof(CatDownloadProgress)} with Id : {progress.Id} after failling 3 times ");
                            }
                        }
                        else if (context.CandidateState is SucceededState)
                        {

                            // here i do manually calculate the condition  cause if not it will be saved as successful !!!
                            // our success condition is   ---> asked cats  to be the same in count as downloaded cats <----- 
                            if (progress.CatsDownloaded == progress.TotalCats)
                            {
                                // Handling success state
                                // LAST error in batch will be always from here !!!
                                progress.Messages += $"\n[Success] {progress.CatsDownloaded} / {progress.TotalCats} cats aquired!!  Job completed successfully for batch {progress.Id} at {DateTimeOffset.UtcNow}\n";
                                progress.Status = Shared.Status.Success;
                            }
                            else
                            {
                                // Handling failed state
                                // LAST error in batch will be always from here !!!
                                progress.Messages += $"\n[Completed with Errors] {progress.CatsDownloaded} / {progress.TotalCats}  cats  !! Errors for batch {progress.Id} at {DateTimeOffset.UtcNow}\n";
                                progress.Status = Shared.Status.Falied;
                            }

                        }

                        // Save changes to the database
                        await db.SaveChangesAsync();

                        _logger.LogInformation($" ------  Mew    (>^-^)>       Cat stealing operation with id {progress.Id} completed with success !!!  Marking the Batch as successful !!! ");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating CatDownloadProgress after job state election.");
                }
            }
        }

        public async Task<Guid> StartNewBatchAsync(int count)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = new CatDownloadProgress
            {
                TotalCats = count
            };

            _context.CatDownloadProgresses.Add(batch);
            await _context.SaveChangesAsync();

            return batch.Id;
        }

        public async Task IncrementProgressAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
                batch.CatsDownloaded = batch.CatsDownloaded + 1;
                await _context.SaveChangesAsync();
            }
        }


        public async Task IncrementBatchFaiureAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
                batch.CatsDownloaded = batch.CatsDownloaded + 1;
                await _context.SaveChangesAsync();
            }
        }


        public async Task IncremenFailuresAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
                batch.ErrorsOccured = batch.ErrorsOccured + 1;
                await _context.SaveChangesAsync();
            }
        }


        public async Task IncremenDublicatesAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
                batch.DoublicatesOccured = batch.DoublicatesOccured + 1;
                await _context.SaveChangesAsync();
            }
        }


        public async Task CompleteBatchAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
                batch.CompletedOn = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CatDownloadProgress?> TrackJobProgressAsync(Guid batchId)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            return await _context.CatDownloadProgresses.Where(x => x.Id == batchId).FirstOrDefaultAsync();
        }

        public async Task<List<CatDownloadProgress>> TrackActiveJobsProgressAsync()
        {
            using var _context = _dbContextFactory.CreateDbContext();

            return await _context.CatDownloadProgresses.Where(x => x.CompletedOn == null).ToListAsync();
        }


        public async Task<List<CatDownloadProgress>> TrackCompletedJobsProgressAsync()
        {
            using var _context = _dbContextFactory.CreateDbContext();

            return await _context.CatDownloadProgresses.Where(x => x.CompletedOn != null).ToListAsync();
        }


        public async Task AppendInfoMessage(Guid batchId, string infoMessage)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var batch = await _context.CatDownloadProgresses.FindAsync(batchId);
            if (batch != null)
            {
             
                //Tgis check is important cause i was loosing my logs  
                if (!string.IsNullOrEmpty(batch.Messages))
                    batch.Messages += batch.Messages + $"\n{infoMessage}";
                else
                    batch.Messages = infoMessage;

                _context.Update(batch);
                await _context.SaveChangesAsync();
            }
        }

        public Task IncrementBatchFailureAsync(Guid batchId)
        {
            throw new NotImplementedException();
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //nothing for now
            ;
        }

    }
}
