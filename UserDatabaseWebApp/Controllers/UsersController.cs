﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using UserDatabaseWebApp.Data;
using UserDatabaseWebApp.Models;

namespace UserDatabaseWebApp.Controllers
{
    public class UsersController : Controller
    {
        private UserDatabaseWebAppContext db = new UserDatabaseWebAppContext();

        [Authorize]
        public ActionResult Index()
        {
            var blockedCheck = CheckIfUserBlocked();
            if (blockedCheck != null)
            {
                return blockedCheck;
            }

            return View(db.Users.OrderByDescending(u => u.LastSeen).ToList());
        }

        private ActionResult CheckIfUserBlocked()
        {
            string currentUserEmail = User.Identity.Name;
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (currentUser != null && currentUser.Blocked)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", new { message = "Your account has been blocked." });
            }

            return null;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            var cookie = Request.Cookies["RememberMe"];
            if (cookie != null)
            {
                ViewBag.RememberedEmail = cookie.Value;
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult SignUp()
        {
            var cookie = Request.Cookies["RememberMe"];
            if (cookie != null)
            {
                ViewBag.RememberedEmail = cookie.Value;
            }
            return View();
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

        [ValidateAntiForgeryToken]
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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user != null)
            {
                if (user.Blocked)
                {
                    ModelState.AddModelError("", "This account has been blocked. Please contact an administrator.");
                    return View(model);
                }

                if (user.Password == Crypto.Hash(model.Password))
                {
                    user.LastSeen = DateTime.Now;
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    return RedirectToAction("Index", "Users");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
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
                Password = Crypto.Hash(Password),
                LastSeen = DateTime.Now,
                Blocked = false
            };

            db.Users.Add(user);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.Email, false);

            return RedirectToAction("Index", "Users");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkDelete(int[] selectedUsers)
        {
            string currentUserEmail = User.Identity.Name;
            bool currentUserDeleted = false;

            if (selectedUsers != null && selectedUsers.Length > 0)
            {
                foreach (var id in selectedUsers)
                {
                    var user = db.Users.Find(id);
                    if (user != null)
                    {
                        if (user.Email == currentUserEmail)
                        {
                            currentUserDeleted = true;
                        }
                        db.Users.Remove(user);
                    }
                }
                db.SaveChanges();

                if (currentUserDeleted)
                {
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkBlock(int[] selectedUsers)
        {
            string currentUserEmail = User.Identity.Name;
            bool currentUserBlocked = false;

            if (selectedUsers != null && selectedUsers.Length > 0)
            {
                foreach (var id in selectedUsers)
                {
                    var user = db.Users.Find(id);
                    if (user != null)
                    {
                        user.Blocked = true;
                        if (user.Email == currentUserEmail)
                        {
                            currentUserBlocked = true;
                        }
                    }
                }
                db.SaveChanges();

                if (currentUserBlocked)
                {
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkUnblock(int[] selectedUsers)
        {
            if (selectedUsers != null && selectedUsers.Length > 0)
            {
                foreach (var id in selectedUsers)
                {
                    var user = db.Users.Find(id);
                    if (user != null)
                    {
                        user.Blocked = false;
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}