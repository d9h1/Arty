using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using System.Net;


public class AddressViewModel
{
    public string FullName { get; set; }
    public string ShippingAddress { get; set; }
    public string CustomerPhone { get; set; }
}

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; }
}

public class OrderDetailsViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public AddressViewModel ShippingAddress { get; set; }
    public List<OrderItemViewModel> Items { get; set; }
}

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Cart
{
    public class OrderDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public OrderDetailsViewModel OrderDetails { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0) return NotFound();

            var orderEntity = await _context.Orders
                .Include(o => o.OrderItems) 
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orderEntity == null)
            {
                return Page();
            }

            OrderDetails = MapOrderToViewModel(orderEntity);

            return Page();
        }


        private OrderDetailsViewModel MapOrderToViewModel(Order orderEntity)
        {
            return new OrderDetailsViewModel
            {
                Id = orderEntity.Id,
                OrderDate = orderEntity.OrderDate,
                Status = orderEntity.Status,
                TotalAmount = orderEntity.TotalAmount,

                ShippingAddress = new AddressViewModel
                {
                    FullName = orderEntity.CustomerName,
                    CustomerPhone = orderEntity.CustomerPhone,
                    ShippingAddress = orderEntity.ShippingAddress 
                },

                Items = orderEntity.OrderItems.Select(itemEntity => new OrderItemViewModel
                {
                    ProductId = itemEntity.ProductId,
                    ProductName = itemEntity.Product?.Name,
                    Price = itemEntity.UnitPrice, 
                    Quantity = itemEntity.Quantity,
                    ImageUrl = itemEntity.Product?.ImageUrl
                }).ToList()
            };
        }
    }
}