
using System.ComponentModel.DataAnnotations;

namespace Ecommerce520.APIV9.DTOs.Request
{
    public class CreateBrandRequest
    {
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public bool Status { get; set; }
        [AllowedExtentions(new[] { ".png" ,".jpg" , ".jpeg" , ".gif"})]
        public IFormFile FormImg { get; set; }
    }
}
