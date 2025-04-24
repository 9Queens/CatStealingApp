using System.Net;
using System.Text.Json;
using CatApp.Shared.APIs.Local.v1.Responses;
using CatApp.Shared;
using FluentAssertions;
using CatApp.Intergration.Tests.Configuration;
using System.Diagnostics;

namespace CatApp.Intergration.Tests
{
    //this means that a one time compose will take place for all the test that exist in the collection
    [CollectionDefinition("compose")]
    public class ComposeCollection : ICollectionFixture<ComposeCliFixture> { }



    //and here we make use of the collection
    [Collection("compose")]
    public class CatApiDockerTests
    {
        //we do not set any apikey here in the client... 
        //as the http client with the X API KEY for cats is been set inside the main app
        //so a simple http client will be used
        private readonly HttpClient _client;

        //and the settings for the fixture
        private readonly DockerSettings _dockerSettings;

        public CatApiDockerTests(ComposeCliFixture fx)
        {
            // Re‑use the HttpClient & strongly‑typed settings supplied by the fixture
            _client = new HttpClient { BaseAddress = new(fx.DockerSettings.ApiBaseUrl) };
            _dockerSettings = fx.DockerSettings;
        }

        // ---------------------------------------------------------
        // GET /api/Cats/1  – happy path
        // ---------------------------------------------------------
        [Fact]
        public async Task GetCatById_1_FromContainer_ReturnsExpectedPayload()
        {
            //------------------------------------------------------------
            // Placing a cat stealing operation request  (size = 3)
            //------------------------------------------------------------
            var fetchResponse = await _client.PostAsync("/api/Cats/fetch?size=3", content: null);
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var batchJson = await fetchResponse.Content.ReadAsStringAsync();
            var scheduledJob = JsonSerializer.Deserialize<ApiResponse<ScheduledJobResult?>>(
                                batchJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            scheduledJob.Should().NotBeNull();
            scheduledJob!.Success.Should().BeTrue();
            scheduledJob.Payload.Should().NotBeNull();
            scheduledJob.Payload!.JobId.Should().NotBe(Guid.Empty);

            var jobId = scheduledJob.Payload.JobId;
            Console.WriteLine($"TEST — Cat stealing batch scheduled with JobId = {jobId}");

            //------------------------------------------------------------
            // Manually give the background job a moment
            // to complete ,not the best way... but will do for now
            //------------------------------------------------------------
            await Task.Delay(TimeSpan.FromSeconds(10)); 

     

            //Crating a pooling strategy here 
            var timeout = TimeSpan.FromSeconds(30);
            var sw = Stopwatch.StartNew();
            ApiResponse<CatDto?>? apiCat = null;

            Console.WriteLine("Pooling to fetch cat with id 1 ......");

            while (sw.Elapsed < timeout)
            {

                //------------------------------------------------------------
                //Retrieve the single cat with internal id = 1
                //------------------------------------------------------------

                var rsp = await _client.GetAsync("/api/Cats/1");
                if (rsp.IsSuccessStatusCode)
                {
                    var catJson = await rsp.Content.ReadAsStringAsync();
                    apiCat = JsonSerializer.Deserialize<ApiResponse<CatDto?>>(
                                 catJson,
                                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiCat?.Payload is not null)
                        break; // <--- will break on success                   
                }

                Console.WriteLine("Pooling fro extra  0.5 seconds ......");
                await Task.Delay(500);  //will wait some time and will retry
            }
 
            //------------------------------------------------------------
            // Final asserts 
            //------------------------------------------------------------
            apiCat.Should().NotBeNull();
            apiCat!.Success.Should().BeTrue();
            apiCat.Payload.Should().NotBeNull();

            // I don’t expose the DB primary‑key, only the third‑party ApiId,
            // so simply verify that a non‑null cat came back.
            apiCat.Payload!.ApiId.Should().NotBeNullOrWhiteSpace();
        }
    }
}
