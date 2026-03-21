using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isAdmin = context.HttpContext.Session.GetString("IsAdmin");

        if (isAdmin != "true")
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }
}