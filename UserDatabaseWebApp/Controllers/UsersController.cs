using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using UserDatabaseWebApp.Data;
using UserDatabaseWebApp.Models;

namespace UserDatabaseWebApp.Controllers
{
    public class UsersController : Controller
    {
        private UserDatabaseWebAppContext db = new UserDatabaseWebAppContext();

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        private bool VerifyPassword(string password1, string password2)
        {
            return password1 == password2;
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Email,Password,LastSeen,Blocked")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Email,Password,LastSeen,Blocked")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult Login(User model)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user != null)
            {
                bool isPasswordValid = VerifyPassword(model.Password, user.Password);
                if (isPasswordValid)
                {
                    return RedirectToAction("Index", "Users");
                }
                    user.LastSeen = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index", "Users");
            }
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        [HttpPost]
        public ActionResult SignUp(string Username, string Email, string Password)
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Email == Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "A user with this email already exists");
                TempData["Email"] = Email;
                return RedirectToAction("Login", "Users");
            }

            var user = new User
            {
                Name = Username,
                Email = Email,
                Password = Password,
                LastSeen = DateTime.Now,
                Blocked = false
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Index", "Users");
        }
    }
}
