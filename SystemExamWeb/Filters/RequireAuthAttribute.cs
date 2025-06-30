using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SystemExamWeb.Filters
{
    public class RequireAuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasToken = context.HttpContext.Request.Cookies.ContainsKey("jwt_token");
            if (!hasToken)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
} 