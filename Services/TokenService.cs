using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreAuditTrail.Services
{
    public class TokenService
    {
        public static string GenerateToken(string username, string role, string email, Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Ensure the secret key is at least 16 characters long
            var key = Encoding.UTF8.GetBytes("A7%f#9LgT@H2!pWzKqRt&9XpMvT!LkQd");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "coderprabhat",
                Audience = "coderprabhat-audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static IDictionary<string, string> DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            return claims;
        }

        public static (string Email, string Subject) DecodeJwt(string token)
        {
            // Initialize a JwtSecurityTokenHandler
            var handler = new JwtSecurityTokenHandler();

            // Validate if the token is in a valid format
            if (!handler.CanReadToken(token))
            {
                throw new ArgumentException("Invalid JWT token");
            }

            // Decode the token
            var jwtToken = handler.ReadJwtToken(token);

            // Extract claims from the payload
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var subject = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return (email, subject);
        }
    }
}
