using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EshopOnWebTaskFunctionAp.Models
{
    public class OrderDetails
    {
        public string id { get; set; }
        public string Type { get; set; }    
        public Address Address { get; set; }
        public List<Item> Items { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
