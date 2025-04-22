using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatApp.Services;
using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Moq;

namespace CatApp.Intergration.Tests
{

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {

        public CustomWebApplicationFactory()
        {
            // Seed data once during factory initialization
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Database.EnsureCreated();
                Add_Get_Cat_ByID_TestData_Async(db).GetAwaiter().GetResult();
            }

        }


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContextFactory<DataContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
            });
        }

        public async Task SeedTestDataAsync()
        {

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            db.Database.EnsureCreated();
            await Add_Get_Cat_ByID_TestData_Async(db);
        }

        private async Task Add_Get_Cat_ByID_TestData_Async(DataContext db)
        {
            try
            {
                // Create tags
                var tag1 = new Tag { Name = "lazy", Created = DateTimeOffset.UtcNow, Id = 1 };
                var tag2 = new Tag { Name = "Like alot", Created = DateTimeOffset.UtcNow, Id = 2 };

                db.Tags.Add(tag1);
                db.Tags.Add(tag2);

                // Create cat
                var cat = new Cat
                {
                    Id = 1,
                    ApiId = "ujxcyomIQ",
                    Width = 750,
                    Height = 561,
                    Image = new byte[1000],
                    ImageUrl = "https://cdn2.thecatapi.com/images/ujxcyomIQ.jpg",
                    ImageHash = "928eef44723d114eff427500f02aa378ea31aa079eede84eba6215fa7a572684",
                    CreatedOn = DateTime.UtcNow
                };

                // Create cat tags
                var catTag = new CatTag { Cat = cat, Tag = tag1 };
                var catTag2 = new CatTag { Cat = cat, Tag = tag2 };

                var catTags = new List<CatTag> { catTag, catTag2 };
                cat.CatTags = catTags;

                // Save cat
                db.Cats.Add(cat);
                await db.SaveChangesAsync();

                Console.WriteLine(" [ Test data for - [ api/cats/{id:int} ] placed -- use id = 1 to retrieve it.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception when setting ---> Test data for testing api/cats/{{id:int}} with message:\n{ex.Message}");
                throw; // Rethrow to ensure test failure
            }
        }
    }
}
