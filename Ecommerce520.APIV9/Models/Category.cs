using Ecommerce520.APIV9.Validations;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Ecommerce520.APIV9.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage= "هذا الحقل مطلوب")]
        //[MinLength(3)]
        //[MaxLength(20)]
        [CustomLength(3,20)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Description { get; set; }
        public bool  Status { get; set; }
        public ICollection<Product>? Products { get; set; } 


    }
}
