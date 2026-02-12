using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Extensions;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization; 

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Products
{
    public class ProductDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager; 
        private const string CartSessionKey = "ShoppingCart";

        public ProductDetailsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager) 
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public Product Product { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCart(int productId, int quantity)
        {
            var productToAdd = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (productToAdd == null || quantity <= 0)
            {
                StatusMessage = "Error: Invalid product or quantity.";
                Product = productToAdd;

                if (Product == null)
                {
                    return NotFound();
                }

                return Page();
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Price = productToAdd.Price
                };
                cart.Add(cartItem);
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            StatusMessage = $"Success: {quantity} x {productToAdd.Name} added to cart!";
            return RedirectToPage("/ShoppingCart");
        }

        public async Task<IActionResult> OnPostAddToWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: You must be logged in to add items to your wishlist.";
                return RedirectToPage("/Identity/Account/Login");
            }

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["StatusMessage"] = "Error: Product not found.";
                return RedirectToPage("./Index");
            }

            var existingItem = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

            if (existingItem != null)
            {
                TempData["StatusMessage"] = "Info: This product is already in your wishlist.";
                Product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == productId);
                return Page();
            }

            var wishlistItem = new WishlistItem
            {
                UserId = user.Id,
                ProductId = productId,
                AddedDate = DateTime.UtcNow
            };

            _db.WishlistItems.Add(wishlistItem);
            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = $"Success: {product.Name} added to your wishlist.";

            Product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == productId);
            return Page();
        }
    }
}