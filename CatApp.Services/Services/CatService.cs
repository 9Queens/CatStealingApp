using CatApp.Shared;
using CatApp.Shared.Data;
using CatApp.Shared.Entities;
using CatApp.Shared.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace CatApp.Services
{
    public class CatService : ICatService
    {
        //private readonly DataContext _context;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;
        private readonly CatServiceOptions _options;

        public CatService(IDbContextFactory<DataContext> dbContextFactory /*DataContext context*/, IOptions<CatServiceOptions> options)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<bool> AddCatAsyn(Cat mew)
        {

            using var _context = _dbContextFactory.CreateDbContext();

            await _context.AddAsync(mew);

            return await _context.SaveChangesAsync() > 0;

        }

        public async Task UpdateCatAsync(Cat mew)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            _context.Update(mew);
            _context.Entry(mew).State = EntityState.Modified;

            await _context.SaveChangesAsync();

        }

        public async Task<Cat?> GetCatAsync(int id)
        {
            using var _context = _dbContextFactory.CreateDbContext();
            return await _context.Cats
                    .AsNoTracking()
                    .Where(x => x.Id == id)
                    .Include(c => c.CatTags)
                    .ThenInclude(ct => ct.Tag)
                    .FirstOrDefaultAsync();
        }

        public async Task<ICollection<CatDto>> GetCatsAsync(int pageNum, int pageSize = 5)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var skip = (pageNum - 1) * pageSize;

            var cats = await _context.Cats
             .AsNoTracking()
             .Where(c => c.Image != null) // Optional: add filter if needed
             .Include(c => c.CatTags)
                 .ThenInclude(ct => ct.Tag)
             .OrderByDescending(c => c.CreatedOn)
             .Skip(skip)
             .Take(pageSize)
             .Select(c => new Cat
             {
                 //Id = c.Id,  <--- also this
                 ApiId = c.ApiId,
                 Width = c.Width,
                 Height = c.Height,
                 //Image = c.Image, < -- removing this for speed and for the demo
                 ImageUrl = c.ImageUrl,
                 ImageHash = c.ImageHash,
                 CreatedOn = c.CreatedOn,
                 CatTags = c.CatTags.Select(ct => new CatTag
                 {
                     Tag = new Tag
                     {
                         Id = ct.Tag.Id,
                         Name = ct.Tag.Name,
                         Created = ct.Tag.Created
                     }
                 }).ToList()
             })
             .ToListAsync();

            // Now project into DTOs
            var catDtos = cats.Select(c => new CatDto(c)).ToList();

            return catDtos ?? new List<CatDto>();
        }

        public async Task<ICollection<CatDto>> GetCatsByTag(List<string> tagNames, int pageNum, int pageSize = 5)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            var skip = (pageNum - 1) * pageSize;

            var cats = await _context.Cats
                          .AsNoTracking()
                          .Where(c => c.CatTags.Any(ct => tagNames.Contains(ct.Tag.Name)))
                          .OrderByDescending(c => c.CreatedOn)
                          .Skip(skip)
                          .Take(pageSize)
                          .Select(c => new Cat
                          {
                              //Id = c.Id,     <---- removing also this
                              ApiId = c.ApiId,
                              Width = c.Width,
                              Height = c.Height,
                              ImageUrl = c.ImageUrl,
                              ImageHash = c.ImageHash,
                              CreatedOn = c.CreatedOn,
                              //Image = c.Image, < -- removing this for speed and for the demo
                              CatTags = c.CatTags
                                  .Where(ct => ct.Tag != null)
                                  .Select(ct => new CatTag
                                  {
                                      Tag = new Tag
                                      {
                                          Id = ct.Tag.Id,
                                          Name = ct.Tag.Name,
                                          Created = ct.Tag.Created
                                      }
                                  }).ToList()
                          })
                          .ToListAsync();

            var catsDto = cats.Select(c => new CatDto(c)).ToList();

            return catsDto ?? new List<CatDto>();
        }


        public async Task<bool> IsCatImageExist(string imageHash)
        {
            using var _context = _dbContextFactory.CreateDbContext();

            return await _context.Cats.AnyAsync(x => x.ImageHash.Equals(imageHash));
        }
    }
}
