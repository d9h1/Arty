using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System; 

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class NewProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public NewProductModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public Product Product { get; set; } = new Product();

        [BindProperty]
        public IFormFile Upload { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private int ArtWorkCategoryId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == "art work");

            if (category == null)
            {
                StatusMessage = "Error: The 'Art Work' category must be created before adding products.";
                return RedirectToPage("./Dashboard");
            }

            ArtWorkCategoryId = category.Id;
            Product.CategoryId = ArtWorkCategoryId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == "art work");

            if (category == null)
            {
                StatusMessage = "Error: 'Art Work' category not found.";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            Product.CategoryId = category.Id;
            Product.SellerId = user.Id;

            ModelState.Remove("Product.SellerId");
            ModelState.Remove("Product.CategoryId");
            ModelState.Remove("Product.ImageUrl");
            ModelState.Remove("Product.Seller");
            ModelState.Remove("Product.Category");

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        var fieldName = state.Key;
                        var errorMessage = state.Value.Errors.First().ErrorMessage;

                        StatusMessage = $"Error in field '{fieldName}': {errorMessage}";

                        Product.CategoryId = ArtWorkCategoryId;
                        return Page();
                    }
                }

                StatusMessage = "Error: Please correct the highlighted fields.";
                Product.CategoryId = ArtWorkCategoryId;
                return Page();
            }

            try
            {
                if (Upload != null && Upload.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Upload.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Upload.CopyToAsync(stream);
                    }

                    Product.ImageUrl = fileName; 

                }
                else
                {
                    Product.ImageUrl = "default.jpg";
                }

                _context.Products.Add(Product);
                await _context.SaveChangesAsync();

                StatusMessage = $"✅ '{Product.Name}' added successfully!";
                return RedirectToPage("./MyProducts");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}