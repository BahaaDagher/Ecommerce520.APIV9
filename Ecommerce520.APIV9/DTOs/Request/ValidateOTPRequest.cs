namespace Ecommerce520.APIV9.DTOs.Request
{
    public class ValidateOTPRequest
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;

    }
}
