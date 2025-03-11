using SalesAPI.Model;

namespace SalesAPI.Interfaces
{
    public interface ISupplierRepository
    {
        Task AddAsync(Supplier supplier);
        Task DeleteAsync(int id);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier> GetByIdAsync(int id);
        Task UpdateAsync(Supplier supplier);
    }
}