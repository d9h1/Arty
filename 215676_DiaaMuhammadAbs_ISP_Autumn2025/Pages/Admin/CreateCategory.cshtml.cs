using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateCategoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateCategoryModel> _logger;

        public CreateCategoryModel(ApplicationDbContext context, ILogger<CreateCategoryModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Category Category { get; set; } = new Category();

        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Category.UrlSegment) && !string.IsNullOrWhiteSpace(Category.Name))
            {
                Category.UrlSegment = Category.Name.ToLower().Replace(' ', '-');
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Category creation failed validation.");
                return Page();
            }

            try
            {
                _context.Categories.Add(Category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category saved, ID: {CategoryId}", Category.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception saving category: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error saving category. Check DB constraints: " + ex.Message);
                return Page();
            }

            return RedirectToPage("./ManageCategories");
        }
    }
}