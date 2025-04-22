using System.Net;
using System.Text.Json;
using CatApp.Controllers;
using CatApp.Services;
using CatApp.Shared;
using CatApp.Shared.APIs.Local.v1.Responses;
using CatApp.Shared.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CatApp.Intergration.Tests
{
    //-----------------------------------------------------------------------------------------------
    //  I just test ONLY one endpoint as this thing will take ages to complete
    //
    //   ONLY ------------>  /api/Cats/{id}  ( happy paths )
    //
    //-----------------------------------------------------------------------------------------------

    public class CatControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        private readonly Mock<IMewService> _mewServiceMock;
        private readonly Mock<IOptions<MewServiceOptions>> _optionsMock;
        private readonly Mock<ILogger<CatsController>> _loggerMock;
        private readonly CatApp.Controllers.CatsController _controller;

        public CatControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _mewServiceMock = new Mock<IMewService>();
            _optionsMock = new Mock<IOptions<MewServiceOptions>>();
            _loggerMock = new Mock<ILogger<CatsController>>();

            // Setup IOptions to return a MewServiceOptions instance
            _optionsMock.Setup(o => o.Value).Returns(new MewServiceOptions());

            _controller = new CatsController(_mewServiceMock.Object, _optionsMock.Object, _loggerMock.Object);

            _factory = factory;
            _client = factory.CreateClient();
        }


        //-----------------------------------------------------------------------------------------------
        //  Test data is already generated inside  CustomWebApplicationFactory.cs ( CatApp.Intergration.Tests )
        //-----------------------------------------------------------------------------------------------

        //some simple intergration tests regarding   /api/Cats/{id}  ( happy paths )

        [Fact]
        public async Task GetCatById_1_ShouldReturnOk_WhenCatExists()
        {
            // Act
            var response = await _client.GetAsync("/api/Cats/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ApiResponse<CatDto?>>(content, options);

            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Status.Should().Be(HttpStatusCode.OK);
            result.Message.Should().Be("Mew! (>^-^)>  a cat is loved! ");

            result.Payload.Should().NotBeNull();
            result.Payload!.ApiId.Should().Be("ujxcyomIQ");
            result.Payload.Width.Should().Be(750);
            result.Payload.Height.Should().Be(561);
            result.Payload.ImageUrl.Should().Be("https://cdn2.thecatapi.com/images/ujxcyomIQ.jpg");
            result.Payload.ImageHash.Should().Be("928eef44723d114eff427500f02aa378ea31aa079eede84eba6215fa7a572684");

            result.Payload.Tags.Should().BeEquivalentTo(new[] { "lazy", "Like alot" });
        }


        [Fact]
        public async Task GetCatById_50000_ShouldReturnNotFound_WhenCat_NOT_Exists()
        {
            // Arrange 
            //await _factory.SeedTestDataAsync(); //  -- Already generated in CustomWebApplicationFactory.cs

            // Act
            var response = await _client.GetAsync("/api/Cats/50000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ApiResponse<CatDto?>>(content, options);

            result.Should().NotBeNull();
            result!.Success.Should().BeTrue(); // <------------- YES TRUE --> business wise it run and fetched nothing ( as expected)  !!
            result.Status.Should().Be(HttpStatusCode.NotFound);
            result.Message.Should().Be("Mew, no one loved that cat :(");


        }


        [Fact]
        public async Task GetCatById_WhenServiceThrowsException_Returns500InternalServerError()
        {
            // Arrange
            int catId = 1;
            var exceptionMessage = "Database connection failed";

            // Create a new factory with a mocked IMewService
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing IMewService registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMewService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Register mocked IMewService
                    var mewServiceMock = new Mock<IMewService>();
                    mewServiceMock
                        .Setup(service => service.GetLocalCatByIdAsync(catId))
                        .ThrowsAsync(new Exception(exceptionMessage));
                    services.AddScoped<IMewService>(_ => mewServiceMock.Object);
                });
            });


            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Cats/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK); // API returns 200 with error payload

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ApiResponse<CatDto?>>(content, options);

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Status.Should().Be(HttpStatusCode.InternalServerError);
            result.Message.Should().Be("Mew :( -  An unexpected error occurred");


        }
    }
}
