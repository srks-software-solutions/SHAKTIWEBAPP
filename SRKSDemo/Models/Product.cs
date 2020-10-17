using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo
{
    public class Product
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public List<Product> plist { get; set; } // List of Products
    }
}