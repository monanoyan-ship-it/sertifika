using Sertifika.Entities;

namespace Sertifika.Factories.Categories;

public interface ICategoryCrudFactory
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
}
