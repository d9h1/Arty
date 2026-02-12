using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _215676_DiaaMuhammadAbs_ISP_Autumn2025.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; } // Primary Key

        public int OrderId { get; set; }// Foreign Key

        public Order Order { get; set; }
        public int ProductId { get; set; }// Foreign Key
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
    }
}
