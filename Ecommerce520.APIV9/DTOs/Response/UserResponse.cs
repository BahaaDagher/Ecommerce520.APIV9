using System.ComponentModel.DataAnnotations;

namespace Ecommerce520.APIV9.DTOs.Response
{
    public class UserResponse
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

    }
}
