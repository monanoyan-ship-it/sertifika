using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Holders;

public class HolderCrudFactory : IHolderCrudFactory
{
    private readonly IHolderEntityService _holderService;
    private readonly IUnitOfWork _unitOfWork;

    public HolderCrudFactory(IHolderEntityService holderService, IUnitOfWork unitOfWork)
    {
        _holderService = holderService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Holder>> GetHoldersAsync()
    {
        return await _holderService.GetActiveHoldersAsync();
    }

    public async Task<Holder?> GetHolderAsync(int id)
    {
        return await _holderService.GetByIdAsync(id);
    }

    public async Task<Holder> CreateHolderAsync(Holder holder)
    {
        _holderService.Add(holder);
        await _unitOfWork.SaveChangesAsync();
        return holder;
    }

    public async Task UpdateHolderAsync(Holder holder)
    {
        holder.UpdatedAt = DateTime.UtcNow;
        _holderService.Update(holder);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteHolderAsync(int id)
    {
        var holder = await _holderService.GetByIdAsync(id);
        if (holder == null) return false;

        holder.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
