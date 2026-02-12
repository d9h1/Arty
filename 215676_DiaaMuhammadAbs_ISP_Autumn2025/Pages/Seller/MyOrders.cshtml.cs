using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class MyOrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyOrdersModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Order> SellerOrders { get; set; } = new List<Order>();
        public string CurrentSellerId { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }
            CurrentSellerId = user.Id;

            SellerOrders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) 
                .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == CurrentSellerId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                StatusMessage = "Error: A valid status must be selected.";
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var orderToUpdate = await _context.Orders.FindAsync(orderId);
            if (orderToUpdate == null)
            {
                StatusMessage = "Error: Order not found.";
                return RedirectToPage();
            }

            bool isSellerForThisOrder = await _context.OrderItems
                .AnyAsync(oi => oi.OrderId == orderId && oi.Product.SellerId == user.Id);

            if (!isSellerForThisOrder)
            {
                StatusMessage = "Error: You do not have permission to update this order.";
                return RedirectToPage();
            }

            orderToUpdate.Status = status;
            await _context.SaveChangesAsync();

            StatusMessage = $"Success: Order #{orderId} status updated to '{status}'.";
            return RedirectToPage();
        }
    }
}