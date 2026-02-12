using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Seller
{
    [Authorize(Roles = "Seller")] 
    public class MyProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyProductsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Product> MyProducts { get; set; } = new List<Product>();

        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            MyProducts = await _context.Products
                .Where(p => p.SellerId == user.Id)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: Authentication failed.";
                return RedirectToPage("/Identity/Account/Login");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
            {
                StatusMessage = "Error: Product not found or you don't own it.";
                return RedirectToPage();
            }


            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            StatusMessage = $"Success: Product '{product.Name}' was deleted.";
            return RedirectToPage();
        }
    }
}