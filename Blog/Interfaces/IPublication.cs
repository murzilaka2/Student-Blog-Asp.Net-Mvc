using Blog.Models;
using Blog.Models.Pages;

namespace Blog.Interfaces
{
    public interface IPublication
    {
        Task<IEnumerable<Publication>> GetAllPublicationsAsync();
        PagedList<Publication> GetAllPublicationsWithCategories(QueryOptions options);
        Task<PagedList<Publication>> GetAllPublicationsByCategoryWithCategories(QueryOptions options, string id);
        Task<Publication> GetPublicationAsync(string id);
        Task<Publication> GetPublicationWithCategoriesAsync(string id);
        Task<IEnumerable<Publication>> GetFourRandomPublicationsAsync(string id);

        Task UpdateViewsAsync(int id);

        Task AddPublicationAsync(Publication publication);
        Task UpdatePublicationAsync(Publication publication);
        Task DeletePublicationAsync(Publication publication);
    }
}
