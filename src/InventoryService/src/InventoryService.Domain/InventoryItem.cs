using System;

namespace InventoryService.Domain
{
    public class InventoryItem
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}
