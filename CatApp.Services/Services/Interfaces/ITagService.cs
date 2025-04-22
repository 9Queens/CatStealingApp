using CatApp.Shared.Entities;

namespace CatApp.Services
{
    public interface ITagService
    {
        Task<bool> AddTagAsyn(Tag tag);
        Task<Tag?> GetTagAsync(string name);
        Task<Tag?> GetTagAsync(int id);
        Task UpdateTagAsync(Tag mew);
    }
}
