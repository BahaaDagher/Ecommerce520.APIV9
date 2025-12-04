namespace Ecommerce520.APIV9.DTOs.Response
{
    public class AuthenticatedResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
