using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<User> Users { get; set; } = null!;
    }
}
