using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; 

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateProductModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Product Product { get; set; }

        public SelectList CategoryNameSL { get; set; }

        public SelectList SellerNameSL { get; set; }

        [BindProperty]
        public IFormFile UploadedImage { get; set; }

        public async Task OnGetAsync()
        {
            CategoryNameSL = new SelectList(_context.Categories, "Id", "Name");

            var adminUser = await _userManager.GetUserAsync(User);
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");

            var usersForSL = new List<IdentityUser>(sellers);
            if (adminUser != null && !usersForSL.Any(u => u.Id == adminUser.Id))
            {
                usersForSL.Add(adminUser);
            }

            SellerNameSL = new SelectList(usersForSL.OrderBy(u => u.UserName), "Id", "UserName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.ContainsKey("Product.Seller"))
            {
                ModelState.Remove("Product.Seller");
            }
            if (ModelState.ContainsKey("Product.Category"))
            {
                ModelState.Remove("Product.Category");
            }
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState invalid, returning Page()");
                foreach (var kv in ModelState.Where(m => m.Value.Errors.Count > 0))
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error - Key: {kv.Key}, Message: {err.ErrorMessage}");
                    }
                }

                await OnGetAsync();
                return Page();
            }
            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + UploadedImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedImage.CopyToAsync(fileStream);
                }

                Product.ImageUrl = "/images/products/" + uniqueFileName;
            }
            else
            {
                Product.ImageUrl = "/images/products/default.png";
            }

            try
            {
                _context.Products.Add(Product);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Product saved, id: " + Product.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception saving product: " + ex.ToString());
                ModelState.AddModelError(string.Empty, "Error saving product. Check DB constraints: " + ex.Message);
                await OnGetAsync();
                return Page();
            }

            return RedirectToPage("./ManageProducts");
        }

    }
}