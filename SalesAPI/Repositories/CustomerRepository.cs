using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SalesAPI.Interfaces;
using SalesAPI.Model;

namespace SalesAPI.Repositories
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {

        public CustomerRepository(IConfiguration configuration, IOptions<AppSettings> appSettings) : base(configuration, appSettings)
        {
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using var connection = CreateConnection();
            string sql = "SELECT customer_id AS CustomerId, customer_name AS CustomerName FROM customer";
            return await connection.QueryAsync<Customer>(sql);
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "SELECT customer_id AS CustomerId, customer_name AS CustomerName FROM customer WHERE customer_id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
        }

        public async Task AddAsync(Customer customer)
        {
            using var connection = CreateConnection();
            string sql = "INSERT INTO customer (customer_name) VALUES (@CustomerName)";
            await connection.ExecuteAsync(sql, customer);
        }

        public async Task UpdateAsync(Customer customer)
        {
            using var connection = CreateConnection();
            string sql = "UPDATE customer SET customer_name = @CustomerName WHERE customer_id = @CustomerId";
            await connection.ExecuteAsync(sql, customer);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "DELETE FROM customer WHERE customer_id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
