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
                var isApiRequest = context.HttpContext.Request.Path.StartsWithSegments("/api");

                if (isApiRequest)
                {
                    context.Result = new JsonResult(new
                    {
                        error = "Sesión expirada",
                        message = "Debe iniciar sesión para realizar esta acción."
                    })
                    { StatusCode = 401 };
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }
    }
}