
namespace API.model
{
    public class AuthModels
    {
        public record AuthResponse(string Token, string Email);
    }
}