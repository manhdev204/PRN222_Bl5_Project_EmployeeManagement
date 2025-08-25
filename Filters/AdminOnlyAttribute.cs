using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PRN222_BL5_Project_EmployeeManagement.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        private const string SessionKeyRole = "AUTH_ROLE";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var role = httpContext.Session.GetString(SessionKeyRole);
            if (string.IsNullOrEmpty(role) || role != "3")
            {
                context.Result = new RedirectToActionResult("Login", "Authentication", null);
            }
        }
    }
}


