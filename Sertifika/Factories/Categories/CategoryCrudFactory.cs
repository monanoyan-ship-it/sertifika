using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Categories;

public class CategoryCrudFactory : ICategoryCrudFactory
{
    private readonly ICategoryEntityService _categoryService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryCrudFactory(ICategoryEntityService categoryService, IUnitOfWork unitOfWork)
    {
        _categoryService = categoryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _categoryService.GetActiveCategoriesAsync();
    }

    public async Task<Category?> GetCategoryAsync(int id)
    {
        return await _categoryService.GetByIdAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _categoryService.Add(category);
        await _unitOfWork.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _categoryService.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return false;

        category.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
