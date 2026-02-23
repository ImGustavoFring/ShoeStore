using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Article
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = null!;
    }
}
