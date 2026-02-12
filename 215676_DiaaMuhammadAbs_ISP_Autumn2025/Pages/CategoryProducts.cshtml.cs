using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // 🟢 جديد: مطلوب للوصول إلى المستخدم الحالي
using Microsoft.AspNetCore.Authorization; // 🟢 جديد: مطلوب لـ [Authorize] إن أردت

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages
{
    public class CategoryProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;  

        public CategoryProductsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager) 
        {
            _context = context;
            _userManager = userManager; 
        }

        public Category Category { get; set; }
        public IList<Product> Products { get; set; }

        [TempData]
        public string StatusMessage { get; set; } 

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories
                                     .FirstOrDefaultAsync(c => c.Id == id);

            if (Category == null)
            {
                return NotFound();
            }

            Products = await _context.Products
                .Include(p => p.Seller)
                .Where(p => p.CategoryId == id && p.StockQuantity > 0)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddToWishlistAsync(int productId, int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: You must be logged in to add items to your wishlist.";
                return RedirectToPage("/Identity/Account/Login");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                StatusMessage = "Error: Product not found.";
            }
            else
            {
                var existingItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

                if (existingItem != null)
                {
                    StatusMessage = $"Info: {product.Name} is already in your wishlist.";
                }
                else
                {
                    var wishlistItem = new WishlistItem
                    {
                        UserId = user.Id,
                        ProductId = productId,
                        AddedDate = DateTime.UtcNow
                    };

                    _context.WishlistItems.Add(wishlistItem);
                    await _context.SaveChangesAsync();
                    StatusMessage = $"Success: {product.Name} added to your wishlist.";
                }
            }

            return RedirectToPage("./CategoryProducts", new { id = id });
        }
    }
}