using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UserDatabaseWebApp.Data;

namespace UserDatabaseWebApp.Filters
{
    public class UserAuthorizationFilter : AuthorizeAttribute
    {
        private readonly UserDatabaseWebAppContext db = new UserDatabaseWebAppContext();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            string userEmail = httpContext.User.Identity.Name;

            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null || user.Blocked)
            {
                FormsAuthentication.SignOut();
                return false;
            }

            user.LastSeen = DateTime.Now;
            db.SaveChanges();

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new { controller = "Users", action = "Login", returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery }
                    )
                );
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Users" &&
                (filterContext.ActionDescriptor.ActionName == "Login" ||
                 filterContext.ActionDescriptor.ActionName == "SignUp"))
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}
