using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageUsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<UserModel> Users { get; set; } = new List<UserModel>();

        public class UserModel : IdentityUser
        {
            public string CurrentRole { get; set; } = "None";
            public IList<string> AvailableRoles { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            foreach (var user in allUsers)
            {
                var rolesList = await _userManager.GetRolesAsync(user);

                string primaryRole = "Customer"; 

                if (rolesList.Contains("Admin"))
                {
                    primaryRole = "Admin";
                }
                else if (rolesList.Contains("Seller"))
                {
                    primaryRole = "Seller";
                }

                Users.Add(new UserModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    CurrentRole = primaryRole,
                    AvailableRoles = roles
                });
            }
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.ToList();
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!string.IsNullOrEmpty(newRole) && newRole != "None")
            {
                if (await _roleManager.RoleExistsAsync(newRole))
                {
                    await _userManager.AddToRoleAsync(user, newRole);
                }
            }
            return RedirectToPage();
        }
    }
}