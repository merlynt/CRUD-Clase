using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace appWeb2.Filters
{
    public class SessionAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetString("usuario");

            if (usuario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

           
        }
    }
}