using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; 

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? UrlSegment { get; set; } 

        [StringLength(50)]
        public string? Icon { get; set; } 

        [Required(ErrorMessage = "Display order is required.")]
        public int DisplayOrder { get; set; }

        public ICollection<Product>? Products { get; set; } = new List<Product>();
    }
}