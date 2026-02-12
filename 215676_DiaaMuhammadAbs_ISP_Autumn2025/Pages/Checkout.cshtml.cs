using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Extensions;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.ViewModels;
using System.Linq;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages
{
    [Authorize(Roles = "Customer,Seller")]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private const string CartSessionKey = "ShoppingCart";

        public CheckoutModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Order ShippingOrder { get; set; } = new Order();

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal CartTotal { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (!CartItems.Any())
            {
                StatusMessage = "Error: Your cart is empty. Cannot proceed to checkout.";
                return RedirectToPage("/ShoppingCart");
            }

            CartTotal = CartItems.Sum(item => item.Total);

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ShippingOrder.CustomerName = user.UserName;
                ShippingOrder.CustomerPhone = user.PhoneNumber;
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            CartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            CartTotal = CartItems.Sum(item => item.Total);

            ModelState.Remove("ShippingOrder.TotalAmount");
            ModelState.Remove("ShippingOrder.OrderDate");
            ModelState.Remove("ShippingOrder.Status");
            ModelState.Remove("ShippingOrder.User");
            ModelState.Remove("ShippingOrder.UserId");
            ModelState.Remove("ShippingOrder.OrderItems");

            if (!CartItems.Any())
            {
                StatusMessage = "Error: Your cart is empty.";
                return RedirectToPage("/ShoppingCart");
            }

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        var fieldName = state.Key;
                        var errorMessage = state.Value.Errors.First().ErrorMessage;
                        StatusMessage = $"Validation Error: Field '{fieldName}' failed check. Reason: {errorMessage}";
                        return Page();
                    }
                }
                StatusMessage = "Error: General validation failed. Please check all fields.";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: You must be logged in to place an order.";
                return RedirectToPage("/Identity/Account/Login");
            }

            foreach (var item in CartItems)
            {
                var productInDb = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (productInDb == null)
                {
                    StatusMessage = $"Error: Product with ID {item.ProductId} creates an issue (Not Found).";
                    return Page();
                }

                if (item.Quantity > productInDb.StockQuantity)
                {
                    StatusMessage = $"Error: Insufficient stock for '{productInDb.Name}'. You requested {item.Quantity}, but only {productInDb.StockQuantity} are available.";
                    return Page(); 
                }
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = user.Id,
                        CustomerName = ShippingOrder.CustomerName,
                        CustomerPhone = ShippingOrder.CustomerPhone,
                        ShippingAddress = ShippingOrder.ShippingAddress,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = CartTotal,
                        Status = "Pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in CartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        };
                        _context.OrderItems.Add(orderItem);

                        var productToUpdate = await _context.Products.FindAsync(item.ProductId);
                        if (productToUpdate != null)
                        {
                            productToUpdate.StockQuantity -= item.Quantity; 
                        }
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    HttpContext.Session.Remove(CartSessionKey);

                    StatusMessage = "Success: Your order has been placed successfully!";
                    return RedirectToPage("/Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    StatusMessage = $"Error: System failed to place order. Detail: {ex.Message}";
                    return Page();
                }
            }
        }
    }
}