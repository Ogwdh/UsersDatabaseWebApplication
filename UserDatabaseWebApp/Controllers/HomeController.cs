using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserDatabaseWebApp.Data;

namespace UserDatabaseWebApp.Controllers
{
    public class HomeController : Controller
    {

        //public ActionResult Index()
        //{
        //    using (var context = new UserDatabaseWebAppContext())
        //    {
        //        var users = context.Users.ToList();
        //        return View(users);
        //    }
        //}

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}

        public ActionResult SignIn()
        {
            return View();
        }
    }
}