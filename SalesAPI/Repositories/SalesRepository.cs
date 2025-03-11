using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SalesAPI.Interfaces;
using SalesAPI.Model;

namespace SalesAPI.Repositories
{
    public class SalesRepository : BaseRepository, ISalesRepository
    {
        public SalesRepository(IConfiguration configuration, IOptions<AppSettings> appSettings)
            : base(configuration, appSettings)
        {
        }

        public async Task<IEnumerable<Sales>> GetAllAsync()
        {
            using var connection = CreateConnection();
            string sql = @"
                SELECT 
                    sales_id AS SalesId, 
                    customer_id AS CustomerId, 
                    product_id AS ProductId, 
                    product_qty AS ProductQty, 
                    sales_date AS SalesDate
                FROM sales";

            var salesList = await connection.QueryAsync<Sales>(sql);
            return salesList.ToList();
        }

        public async Task<Sales> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "SELECT sales_id AS SalesId, customer_id AS CustomerId, product_id AS ProductId, product_qty AS ProductQty, sales_date AS SalesDate FROM sales WHERE sales_id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Sales>(sql, new { Id = id });
        }

        public async Task<Sales> AddSaleAsync(CreateSaleDto saleDto)
        {
            using var connection = CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Periksa stok produk
                string checkStockSql = "SELECT product_stock FROM product WHERE product_id = @ProductId";
                int currentStock = await connection.ExecuteScalarAsync<int>(checkStockSql, new { saleDto.ProductId }, transaction);

                if (currentStock < saleDto.ProductQty)
                {
                    throw new Exception("Not enough stock available");
                }

                // 2️⃣ Insert transaksi penjualan
                string insertSaleSql = @"
                    INSERT INTO sales (customer_id, product_id, product_qty, sales_date) 
                    VALUES (@CustomerId, @ProductId, @ProductQty, CURRENT_TIMESTAMP);
                    SELECT last_insert_rowid();";

                int saleId = await connection.ExecuteScalarAsync<int>(insertSaleSql, saleDto, transaction);

                // 3️⃣ Update stok produk
                string updateStockSql = "UPDATE product SET product_stock = product_stock - @ProductQty WHERE product_id = @ProductId";
                await connection.ExecuteAsync(updateStockSql, saleDto, transaction);

                // 4️⃣ Ambil data yang baru saja ditambahkan
                string getSaleSql = @"
                    SELECT 
                        sales_id AS SalesId,
                        customer_id AS CustomerId,
                        product_id AS ProductId,
                        product_qty AS ProductQty,
                        sales_date AS SalesDate
                    FROM sales
                    WHERE sales_id = @SaleId";

                var sale = await connection.QuerySingleOrDefaultAsync<Sales>(getSaleSql, new { SaleId = saleId }, transaction);

                transaction.Commit();
                return sale;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            string sql = "DELETE FROM sales WHERE sales_id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<DailySalesReportDto>> GetDailySalesReportAsync(DateTime date)
        {
            using var connection = CreateConnection();
            string sql = @"
                SELECT 
                    c.customer_id AS CustomerId, 
                    c.customer_name AS CustomerName, 
                    SUM(s.product_qty) AS TotalProductQty,
                    SUM(s.product_qty * p.product_price) AS TotalSalesAmount
                FROM sales s
                JOIN customer c ON s.customer_id = c.customer_id
                JOIN product p ON s.product_id = p.product_id
                WHERE DATE(s.sales_date) = DATE(@Date)
                GROUP BY c.customer_id, c.customer_name";

            return await connection.QueryAsync<DailySalesReportDto>(sql, new { Date = date });
        }




    }
}
