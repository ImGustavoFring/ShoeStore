using System;
using System.Collections.Generic;
using System.Text;

namespace ShoeStore.WpfApp.Models
{
    public class Order
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = null!;
        public DateOnly OrderDate { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public long PickUpPointId {  get; set; }
        public PickUpPoint PickUpPoint { get; set; } = null!;
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        public long ReceiptCode { get; set; }
        public long StatusId { get; set; }
        public Status Status { get; set; } = null!;

    }
}
