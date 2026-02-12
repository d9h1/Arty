using System;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}