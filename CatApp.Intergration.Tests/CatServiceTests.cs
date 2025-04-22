using System.Net;
using System.Text.Json;
using CatApp.Controllers;
using CatApp.Services;
using CatApp.Shared;
using CatApp.Shared.APIs.Cats.v1.Responses;
using CatApp.Shared.APIs.Local.v1.Responses;
using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using CatApp.Shared.Options;
using FluentAssertions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CatApp.Intergration.Tests
{

    //-----------------------------------------------------------------------------------------------
    //  I just test ONLY  ICatService as this thing will take ALSO ages to complete
    //
    //   ONLY ------------>  CatApp.Services.ICatService.cs
    //                          <--- and only what the interface provides  NOT All 
    //                                 All methods inside the concrete implementation of CatService (if any)
    //
    //-----------------------------------------------------------------------------------------------

    public class CatServiceTests
    {
        private readonly Mock<IOptions<CatServiceOptions>> _optionsMock;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;

        public CatServiceTests()
        {
            _optionsMock = new Mock<IOptions<CatServiceOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(new CatServiceOptions());

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContextFactory = new InMemoryDbContextFactory(options);
        }

        private class InMemoryDbContextFactory : IDbContextFactory<DataContext>
        {
            private readonly DbContextOptions<DataContext> _options;

            public InMemoryDbContextFactory(DbContextOptions<DataContext> options)
            {
                _options = options;
            }

            public DataContext CreateDbContext()
            {
                return new DataContext(_options);
            }
        }

        private Cat CreateTestCat(int id = 1, string hash = "testhash123")
        {
            return new Cat
            {
                Id = id,
                ApiId = "api" + id,
                Width = 100,
                Height = 100,
                ImageUrl = "http://cat.jpg",
                ImageHash = hash,
                Image = new byte[2000],
                CreatedOn = DateTimeOffset.UtcNow,
                CatTags = new List<CatTag>
                {
                    new CatTag { Tag = new Tag { Id = 1, Name = "cute", Created = DateTimeOffset.UtcNow } }
                }
            };
        }

        [Fact]
        public async Task AddCatAsync_ShouldAddCatAndReturnTrue()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);
            var cat = CreateTestCat();

            // Act
            var result = await service.AddCatAsyn(cat);

            // Assert
            result.Should().BeTrue();
            using var context = _dbContextFactory.CreateDbContext();
            var savedCat = await context.Cats.FindAsync(1);
            savedCat.Should().NotBeNull();
            savedCat.ApiId.Should().Be("api1");
        }

        [Fact]
        public async Task AddCatAsync_ShouldThrow_WhenCatIsNull()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddCatAsyn(null));
        }

        [Fact]
        public async Task UpdateCatAsync_ShouldUpdateCat()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var cat = CreateTestCat(1,"asdasdasdasd");
            await context.Cats.AddAsync(cat);
            await context.SaveChangesAsync();

            var service = new CatService(_dbContextFactory, _optionsMock.Object);
            //var updatedCat = CreateTestCat();
            var saved = await service.GetCatAsync(1);
            saved.Width = 500;

            // Act
            await service.UpdateCatAsync(saved);

            // Assert
            var result = await service.GetCatAsync(1);
            result.Width.Should().Be(500);
        }

        [Fact]
        public async Task UpdateCatAsync_ShouldThrow_WhenCatIsNull()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateCatAsync(null));
        }

        [Fact]
        public async Task GetCatAsync_ShouldReturnCat_WhenExists()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var cat = CreateTestCat();
            await context.Cats.AddAsync(cat);
            await context.SaveChangesAsync();

            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var result = await service.GetCatAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.ApiId.Should().Be("api1");
            result.CatTags.Should().HaveCount(1);
            result.CatTags.First().Tag.Name.Should().Be("cute");
        }

        [Fact]
        public async Task GetCatAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var result = await service.GetCatAsync(9000);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCatsAsync_ShouldReturnPagedList()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            
            var tag = new Tag { Id = 1, Name = "cute", Created = DateTimeOffset.UtcNow };
            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            for (int i = 1; i <= 10; i++)
            {
                var cat = new Cat
                {
                    Id = i,
                    ApiId = "api" + i,
                    Width = 100,
                    Height = 100,
                    ImageUrl = "http://cat.jpg",
                    ImageHash = "hash" + i,
                    Image = new byte[2000],
                    CreatedOn = DateTimeOffset.UtcNow,
                    CatTags = new List<CatTag>
            {
                new CatTag { Tag = tag } // Reuse the same Tag
            }
                };
                await context.Cats.AddAsync(cat);
                await context.SaveChangesAsync();
            }

            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var cats = await service.GetCatsAsync(pageNum: 1, pageSize: 5);

            // Assert
            cats.Should().HaveCount(5);
            cats.Should().AllSatisfy(c => c.ImageUrl.Should().NotBeNull());
            cats.Should().AllSatisfy(c => c.Tags.Should().NotBeEmpty());
        }

        [Fact]
        public async Task GetCatsAsync_ShouldReturnEmpty_WhenNoCats()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var cats = await service.GetCatsAsync(pageNum: 1, pageSize: 5);

            // Assert
            cats.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCatsByTag_ShouldReturnFilteredCats()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var cat = CreateTestCat();
            await context.Cats.AddAsync(cat);
            await context.SaveChangesAsync();

            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var cats = await service.GetCatsByTag(new List<string> { "cute" }, pageNum: 1, pageSize: 5);

            // Assert
            cats.Should().HaveCount(1);
            cats.First().ApiId.Should().Be("api1");
            cats.First().Tags.Should().Contain("cute");
        }

        [Fact]
        public async Task GetCatsByTag_ShouldReturnEmpty_WhenTagNotFound()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var cat = CreateTestCat();
            await context.Cats.AddAsync(cat);
            await context.SaveChangesAsync();

            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var cats = await service.GetCatsByTag(new List<string> { "notfound" }, pageNum: 1, pageSize: 5);

            // Assert
            cats.Should().BeEmpty();
        }

        [Fact]
        public async Task IsCatImageExist_ShouldReturnTrue_WhenHashExists()
        {
            // Arrange
            using var context = _dbContextFactory.CreateDbContext();
            var cat = CreateTestCat();
            await context.Cats.AddAsync(cat);
            await context.SaveChangesAsync();

            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var result = await service.IsCatImageExist("testhash123");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsCatImageExist_ShouldReturnFalse_WhenHashDoesNotExist()
        {
            // Arrange
            var service = new CatService(_dbContextFactory, _optionsMock.Object);

            // Act
            var result = await service.IsCatImageExist("nonexistent");

            // Assert
            result.Should().BeFalse();
        }
    }

}