using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Customer
{
    [Authorize(Roles = "Customer,Admin,Seller")]
    public class CustomerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public string LastOrderNumber { get; set; } = "N/A";
        public string LastOrderDate { get; set; } = "N/A";

        public List<Product> RecommendedProducts { get; set; } = new List<Product>();

        public CustomerDashboardModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
            string currentUserId = user.Id;

           
            var lastOrder = await _context.Orders
                                          .Where(o => o.UserId == currentUserId)
                                          .OrderByDescending(o => o.OrderDate) 
                                          .FirstOrDefaultAsync();

            if (lastOrder != null)
            {
                LastOrderNumber = $"#{lastOrder.Id:D4}"; 
                LastOrderDate = lastOrder.OrderDate.ToString("yyyy-MM-dd");
            }

            RecommendedProducts = await _context.Products
                                                .OrderBy(p => Guid.NewGuid())
                                                .Take(4)
                                                .ToListAsync();

            return Page();
        }
    }
}