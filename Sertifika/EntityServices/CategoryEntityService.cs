using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class CategoryEntityService : ICategoryEntityService
{
    private readonly AppDbContext _context;

    public CategoryEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        => await _context.Categories.Where(c => c.IsActive).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories.FindAsync(id);

    public void Add(Category category) => _context.Categories.Add(category);

    public void Update(Category category) => _context.Entry(category).State = EntityState.Modified;
}
