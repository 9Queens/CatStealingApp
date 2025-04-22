using CatApp.Shared;
using CatApp.Shared.Entities;

namespace CatApp.Services
{
    /// <summary>
    /// Base CRUD for locally stored cats in the database
    /// </summary>
    public interface ICatService
    {

        Task<bool> AddCatAsyn(Cat mew);

        Task UpdateCatAsync(Cat mew);

        Task<Cat?> GetCatAsync(int id);

        Task<bool> IsCatImageExist(string imageHash);

        Task<ICollection<CatDto>> GetCatsAsync(int pageNum, int pageSize = 5);

        Task<ICollection<CatDto>> GetCatsByTag(List<string> tagName, int pageNum, int pageSize = 5);

    }

}
