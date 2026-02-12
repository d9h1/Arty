using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Categories = _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            Products = _context.Products
                .Include(p => p.Category)
                .Take(100)
                .ToList();

        }
    }
}