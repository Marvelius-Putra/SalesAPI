using SalesAPI.Model;

namespace SalesAPI.Interfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task DeleteAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync(int threshold);
        Task UpdateAsync(Product product);
    }
}