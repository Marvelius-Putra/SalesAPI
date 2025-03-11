using SalesAPI.Model;

namespace SalesAPI.Interfaces
{
    public interface ISalesRepository
    {
        Task<Sales> AddSaleAsync(CreateSaleDto saleDto);
        Task DeleteAsync(int id);
        Task<IEnumerable<Sales>> GetAllAsync();
        Task<Sales> GetByIdAsync(int id);
        Task<IEnumerable<DailySalesReportDto>> GetDailySalesReportAsync(DateTime date);
    }
}