using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages
{
    [Authorize(Roles = "Customer,Seller")]
    public class WishlistModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WishlistModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            WishlistItems = await _context.WishlistItems
                .Where(w => w.UserId == user.Id)
                .Include(w => w.Product) 
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: You must be logged in.";
                return RedirectToPage("/Identity/Account/Login");
            }

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (item == null)
            {
                StatusMessage = "Error: Wishlist item not found or does not belong to you.";
                return RedirectToPage();
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            StatusMessage = "Success: Item removed from your wishlist.";
            return RedirectToPage();
        }
    }
}