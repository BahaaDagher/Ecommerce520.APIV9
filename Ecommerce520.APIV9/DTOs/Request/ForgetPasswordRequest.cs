namespace Ecommerce520.APIV9.DTOs.Request
{
    public class ForgetPasswordRequest
    {
        public int Id { get; set; }
        public string UserNameOrEmail { get; set; } = string.Empty;

    }
}
