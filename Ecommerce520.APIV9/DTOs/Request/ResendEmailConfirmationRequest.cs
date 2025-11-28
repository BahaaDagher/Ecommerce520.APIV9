namespace Ecommerce520.APIV9.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        public int Id { get; set; }
        public string UserNameOrEmail { get; set; }  = string.Empty;
    }
}
