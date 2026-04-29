using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using appWeb2.Models;

namespace appWeb2.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly int _rolRequerido;

        public SessionAuthorizeAttribute(int rolRequerido = 0)
        {
            _rolRequerido = rolRequerido;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var jsonUsuario = context.HttpContext.Session.GetString("usuario");

            if (string.IsNullOrEmpty(jsonUsuario))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_rolRequerido > 0)
            {
                var user = JsonSerializer.Deserialize<Usuario>(jsonUsuario);

                if (user == null || user.rol.id != _rolRequerido)
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}