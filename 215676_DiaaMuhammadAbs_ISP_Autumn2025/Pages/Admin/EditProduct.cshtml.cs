using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditProductModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product = await _context.Products
                                    .Include(p => p.Category)
                                    .Include(p => p.Seller)
                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(Product.ImageUrl) && !Product.ImageUrl.StartsWith("/") && !Product.ImageUrl.StartsWith("http"))
            {
                Product.ImageUrl = "/images/products/" + Product.ImageUrl.TrimStart('~', '/');
            }

            await PopulateSelectListsAsync();

            return Page();
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

            if (UploadedImage == null && ModelState.ContainsKey("UploadedImage"))
            {
                ModelState.Remove("UploadedImage");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            var existingProduct = await _context.Products.AsNoTracking()
                                                .Select(p => new { p.Id, p.ImageUrl })
                                                .FirstOrDefaultAsync(p => p.Id == Product.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

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
                Product.ImageUrl = existingProduct.ImageUrl;
            }


            _context.Attach(Product).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("./ManageProducts");
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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error saving changes: " + ex.Message);
                await PopulateSelectListsAsync();
                return Page();
            }

            await PopulateSelectListsAsync();
            return Page();
        }

        private async Task PopulateSelectListsAsync()
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
    }
}