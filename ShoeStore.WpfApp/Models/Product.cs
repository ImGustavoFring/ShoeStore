using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Product
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public Article Article { get; set; } = null!;
        public string Name { get; set; } = null!;
        public long UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
        public decimal Price { get; set; }
        public long SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        public long ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public long CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public decimal Discount { get; set; }
        public long QuantityInStock { get; set; }
        public string Description { get; set; } = null!;
        public string? PhotoPath { get; set; } = null;
    }
}
