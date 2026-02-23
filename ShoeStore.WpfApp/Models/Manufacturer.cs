using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Manufacturer
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public IEnumerable<Product> Products { get; set; } = null!;
    }
}
