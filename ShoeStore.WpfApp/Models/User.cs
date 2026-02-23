using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class User
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = null!;
    }
}
