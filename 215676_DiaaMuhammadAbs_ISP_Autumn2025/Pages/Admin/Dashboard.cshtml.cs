using _215676_DiaaMuhammadAbs_ISP_Autumn2025.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public decimal TotalSales { get; set; }
    public int TotalSellers { get; set; }
    public int PendingOrdersCount { get; set; }

    public AdminDashboardModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task OnGetAsync()
    {
        TotalSales = await _context.Orders
                                    .SumAsync(o => o.TotalAmount);

        var allUsers = await _userManager.Users.ToListAsync();
        var sellerUsers = new List<IdentityUser>();

        foreach (var user in allUsers)
        {
            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                sellerUsers.Add(user);
            }
        }
        TotalSellers = sellerUsers.Count;

        PendingOrdersCount = await _context.Orders
                                        .CountAsync(o => o.Status == "Pending");

    }
}