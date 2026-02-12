using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Please enter your phone number.")]
        [Display(Name = "Phone Number")]
        //[Phone]//
        public string CustomerPhone { get; set; }
        [BindNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [BindNever]
        public IdentityUser User { get; set; }
        [BindNever]
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Please enter your shipping address.")]
        public string ShippingAddress { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [BindNever]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        [BindNever]
        public string Status { get; set; }
        [BindNever]
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}