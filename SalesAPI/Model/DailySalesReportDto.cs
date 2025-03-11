using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPI.Model
{
    public class DailySalesReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalProductQty { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }
}
