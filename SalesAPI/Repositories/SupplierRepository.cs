using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SalesAPI.Interfaces;
using SalesAPI.Model;

namespace SalesAPI.Repositories
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        public SupplierRepository(IConfiguration configuration, IOptions<AppSettings> appSettings)
            : base(configuration, appSettings)
        {
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            using var connection = CreateConnection();
            string sql = "SELECT supplier_id AS SupplierId, supplier_name AS SupplierName FROM supplier";
            return await connection.QueryAsync<Supplier>(sql);
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "SELECT supplier_id AS SupplierId, supplier_name AS SupplierName FROM supplier WHERE supplier_id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { Id = id });
        }

        public async Task AddAsync(Supplier supplier)
        {
            using var connection = CreateConnection();
            string sql = "INSERT INTO supplier (supplier_name) VALUES (@SupplierName)";
            await connection.ExecuteAsync(sql, supplier);
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            using var connection = CreateConnection();
            string sql = "UPDATE supplier SET supplier_name = @SupplierName WHERE supplier_id = @SupplierId";
            await connection.ExecuteAsync(sql, supplier);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "DELETE FROM supplier WHERE supplier_id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
