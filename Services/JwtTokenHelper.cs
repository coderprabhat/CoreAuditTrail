using System.Security.Claims;

namespace CoreAuditTrail.Services
{
    public class JwtTokenHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
        }
    }
}
