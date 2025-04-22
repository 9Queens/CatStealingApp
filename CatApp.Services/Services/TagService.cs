using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatApp.Services.Services
{
    public class TagService : ITagService
    {
        //private readonly DataContext _context;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;

        public TagService(IDbContextFactory<DataContext> dbContextFactory/* DataContext context*/)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public async Task<bool> AddTagAsyn(Tag tag)
        {
            using var context = _dbContextFactory.CreateDbContext();

            await context.AddAsync(tag);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<Tag?> GetTagAsync(string name)
        {
            using var context = _dbContextFactory.CreateDbContext();

            return await context.Tags.Where(x => x.Name.Equals(name)).FirstOrDefaultAsync();

        }

        public async Task<Tag?> GetTagAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();

            return await context.Tags.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateTagAsync(Tag mew)
        {

            using var context = _dbContextFactory.CreateDbContext();
            context.Tags.Update(mew);
            await context.SaveChangesAsync();

        }
    }
}
