using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class SellerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public decimal TotalRevenue { get; set; }
        public int TotalListings { get; set; }
        public int ShippedOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }

        public string LastOrderNumber { get; set; } = "N/A";
        public string LastOrderDate { get; set; } = "N/A";

        public List<ProductViewModel> RecommendedProducts { get; set; } = new List<ProductViewModel>();

        public SellerDashboardModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            string currentSellerId = user.Id;


            TotalListings = await _context.Products
                                         .CountAsync(p => p.SellerId == currentSellerId);

            var sellerOrderItems = await _context.OrderItems
                                                 .Include(oi => oi.Product)
                                                 .Include(oi => oi.Order)
                                                 .Where(oi => oi.Product.SellerId == currentSellerId)
                                                 .ToListAsync();

            TotalRevenue = sellerOrderItems
                                 .Sum(oi => oi.Quantity * oi.UnitPrice);

            var sellerOrders = sellerOrderItems
                                 .Select(oi => oi.Order)
                                 .DistinctBy(o => o.Id) 
                                 .ToList();

            ShippedOrdersCount = sellerOrders
                                     .Count(o => o.Status == "Shipped");

            PendingOrdersCount = sellerOrders
                                     .Count(o => o.Status == "Pending");

            var lastOrder = await _context.Orders
                .Where(o => o.UserId == currentSellerId) 
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new { o.Id, o.OrderDate })
                .FirstOrDefaultAsync();

            if (lastOrder != null)
            {
                LastOrderNumber = lastOrder.Id.ToString();
                LastOrderDate = lastOrder.OrderDate.ToShortDateString();
            }

            RecommendedProducts = await _context.Products
                .OrderBy(p => Guid.NewGuid()) 
                .Take(4)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Page();
        }
    }
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}