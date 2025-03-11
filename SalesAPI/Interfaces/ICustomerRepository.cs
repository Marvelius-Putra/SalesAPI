using SalesAPI.Model;

namespace SalesAPI.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(int id);
        Task UpdateAsync(Customer customer);
    }
}