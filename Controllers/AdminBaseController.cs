using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SparshaERP.Controllers
{
    public class AdminBaseController : Controller
    {
        // MUST be public (same as base Controller)
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check admin login session
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                context.Result = RedirectToAction("Login", "Auth");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
