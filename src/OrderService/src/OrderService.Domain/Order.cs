using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderService.Domain
{
    public class Order
    {
        public Guid Id { get; private set; }
        public string CustomerName { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        // Private constructor for EF Core
        private Order() { }

        public Order(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

            Id = Guid.NewGuid();
            CustomerName = customerName;
            Status = "Created";
            CreatedAt = DateTime.UtcNow;
        }

        public void AddItem(string productName, decimal unitPrice, int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            if (unitPrice < 0) throw new ArgumentException("Price cannot be negative.");

            var item = new OrderItem(Id, productName, unitPrice, quantity);
            _items.Add(item);

            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            TotalAmount = _items.Sum(x => x.UnitPrice * x.Quantity);
        }

        public void MarkAsProcessed()
        {
            Status = "Processed";
        }
    }

    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string ProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        private OrderItem() { }

        internal OrderItem(Guid orderId, string productName, decimal unitPrice, int quantity)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
    }
}
