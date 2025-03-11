using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SalesAPI.Interfaces;
using SalesAPI.Model;

namespace SalesAPI.Repositories
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(IConfiguration configuration, IOptions<AppSettings> appSettings)
            : base(configuration, appSettings)
        {
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = CreateConnection();
            string sql = "SELECT product_id AS ProductId, product_name AS ProductName, product_price AS ProductPrice, product_stock AS ProductStock, supplier_id AS SupplierId FROM product";
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "SELECT product_id AS ProductId, product_name AS ProductName, product_price AS ProductPrice, product_stock AS ProductStock, supplier_id AS SupplierId FROM product WHERE product_id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }

        public async Task AddAsync(Product product)
        {
            using var connection = CreateConnection();
            string sql = "INSERT INTO product (product_name, product_price, product_stock, supplier_id) VALUES (@ProductName, @ProductPrice, @ProductStock, @SupplierId)";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task UpdateAsync(Product product)
        {
            using var connection = CreateConnection();
            string sql = "UPDATE product SET product_name = @ProductName, product_price = @ProductPrice, product_stock = @ProductStock, supplier_id = @SupplierId WHERE product_id = @ProductId";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "DELETE FROM product WHERE product_id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync(int threshold)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT p.product_id AS ProductId, 
                       p.product_name AS ProductName, 
                       p.product_stock AS ProductStock, 
                       s.supplier_name AS SupplierName
                FROM product p
                JOIN supplier s ON p.supplier_id = s.supplier_id
                WHERE p.product_stock < @Threshold";

            return await connection.QueryAsync<LowStockProductDto>(query, new { Threshold = threshold });
        }


    }
}
