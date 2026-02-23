using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Status
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public IEnumerable<Order> Orders { get; set; } = null!;
    }
}
