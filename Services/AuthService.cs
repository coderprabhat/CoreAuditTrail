using System.Security.Cryptography;
using System.Text;

namespace CoreAuditTrail.Services
{
    public class AuthService
    {
        public static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = $"{password}{salt}";
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hash);
        }

        public static string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public static bool VerifyPassword(string password, string salt, string hash)
        {
            var hashedPassword = HashPassword(password, salt);
            return hashedPassword == hash;
        }
    }
}
