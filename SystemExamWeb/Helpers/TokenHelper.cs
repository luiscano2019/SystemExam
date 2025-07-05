using Microsoft.AspNetCore.Http;

namespace SystemExamWeb.Helpers
{
    public static class TokenHelper
    {
        public static string? GetJwtToken(HttpRequest request)
        {
            return request.Cookies.TryGetValue("jwt_token", out var token) ? token : null;
        }
    }
}