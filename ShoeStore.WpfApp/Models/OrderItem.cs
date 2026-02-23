using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public Article Article { get; set; } = null!;
        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public long Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
