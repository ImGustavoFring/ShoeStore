using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class PickUpPoint
    {
        public long Id { get; set; }
        public long PostCode { get; set; }
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public long House {  get; set; }
        public ICollection<Order> Orders { get; set; } = null!;
    }
}
