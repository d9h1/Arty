using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.ViewModels;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Cart
{
    [Authorize(Roles = "Customer,Seller")]
    public class ShoppingCartModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "ShoppingCart";

        public ShoppingCartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal CartTotal => CartItems.Sum(item => item.Total);

        public void OnGet()
        {
            CartItems = GetCartItems();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1)
        {
            if (quantity <= 0) return RedirectToPage();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return RedirectToPage();
            }

            CartItems = GetCartItems();

            var existingItem = CartItems.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity
                };
                CartItems.Add(cartItem);
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, CartItems);

            return RedirectToPage("./ShoppingCart");
        }

        public IActionResult OnPostUpdateQuantity(int productId, int quantity)
        {
            CartItems = GetCartItems();

            if (quantity <= 0)
            {
                return OnPostRemove(productId);
            }

            var itemToUpdate = CartItems.FirstOrDefault(i => i.ProductId == productId);

            if (itemToUpdate != null)
            {
                itemToUpdate.Quantity = quantity;

                HttpContext.Session.SetObjectAsJson(CartSessionKey, CartItems);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int productId)
        {
            CartItems = GetCartItems();

            var itemToRemove = CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                CartItems.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, CartItems);
            }

            return RedirectToPage();
        }

        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }
    }
}