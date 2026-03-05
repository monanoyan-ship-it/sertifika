using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ICategoryEntityService
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetByIdAsync(int id);
    void Add(Category category);
    void Update(Category category);
}
