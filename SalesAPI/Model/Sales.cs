using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPI.Model
{
    public class Sales
    {
        public int SalesId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int ProductQty { get; set; }
        public DateTime SalesDate { get; set; }
    }


}
