using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageCategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageCategoriesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Category> Category { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            Category = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if (id == null)
            {
                StatusMessage = "Error: Category ID not provided.";
                return RedirectToPage();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                StatusMessage = "Error: Category not found.";
                return RedirectToPage();
            }

            try
            {
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    StatusMessage = $"Error: Cannot delete category '{category.Name}' because it has active products associated with it.";
                    return RedirectToPage();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                // رسالة نجاح
                StatusMessage = $"Success: Category '{category.Name}' deleted successfully.";
                return RedirectToPage();
            }
            catch (DbUpdateException)
            {
                // في حالة فشل قاعدة البيانات (نادر الحدوث بعد التحقق من المنتجات)
                StatusMessage = "Error: Failed to delete category due to database constraints.";
                return RedirectToPage();
            }
        }
    }
}