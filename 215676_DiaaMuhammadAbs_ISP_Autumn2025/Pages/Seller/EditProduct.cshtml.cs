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
    public class EditProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EditProductModel(
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
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            Product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (Product == null)
            {
                StatusMessage = "Error: Product not found or you do not have permission to edit it.";
                return RedirectToPage("./MyProducts");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == "art work");
            if (category != null)
            {
                ArtWorkCategoryId = category.Id;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == "art work");
            if (category == null)
            {
                StatusMessage = "Error: 'Art Work' category not found.";
                return Page();
            }
            ArtWorkCategoryId = category.Id;

            ModelState.Remove("Product.SellerId");
            ModelState.Remove("Product.Seller"); 
            ModelState.Remove("Product.CategoryId");
            ModelState.Remove("Product.Category"); 
            ModelState.Remove("Product.ImageUrl"); 
            ModelState.Remove("Upload"); 

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState.Values.SelectMany(v => v.Errors))
                {
                    StatusMessage = $"Error: {state.ErrorMessage}";
                    break;
                }
                return Page();
            }

            var productToUpdate = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == Product.Id && p.SellerId == user.Id);

            if (productToUpdate == null)
            {
                StatusMessage = "Error: Product not found or unauthorized access.";
                return RedirectToPage("./MyProducts");
            }

            if (Upload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Upload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(fileStream);
                }


                productToUpdate.ImageUrl = uniqueFileName; 
            }
            productToUpdate.Name = Product.Name;
            productToUpdate.Description = Product.Description;
            productToUpdate.Price = Product.Price;
            productToUpdate.StockQuantity = Product.StockQuantity;
            productToUpdate.CategoryId = ArtWorkCategoryId; 

            try
            {
                _context.Attach(productToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            StatusMessage = $"Success: '{productToUpdate.Name}' has been updated.";
            return RedirectToPage("./MyProducts");
        }
    }
}