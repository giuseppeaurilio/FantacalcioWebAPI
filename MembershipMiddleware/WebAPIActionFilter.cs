using Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace JwtTokenMiddleware
{
    public class UserAuthorizationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //var param = context.ActionArguments.SingleOrDefault(p => p.Value is AuthorizedUser);
            var identity = context.HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                context.Result = new BadRequestObjectResult("identity is null");
                return;
            }

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
            var username = identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier)
                   .Select(c => c.Value).SingleOrDefault();
            if (!User.canExecute(username,
                context.ActionDescriptor.RouteValues["controller"],
                context.ActionDescriptor.RouteValues["action"]))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //var identity = context.HttpContext.User.Identity as ClaimsIdentity;
            //if (identity.IsAuthenticated)
            //{
            //    //Il client potrà usare questo nuovo token nella sua prossima richiesta
            //    context.HttpContext.Response.Headers.Add("X-Token", CreateTokenForIdentity(identity));
            //}
        }
    }
}
