using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GreenbushIep.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string Email, string Password)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                using (IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities())
                {
                    string email = Email;
                    string password = Password;

                    // Now if our password was enctypted or hashed we would have done the
                    // same operation on the user entered password here, But for now
                    // since the password is in plain text lets just authenticate directly
                    try
                    {
                        tblUser user = db.tblUsers.FirstOrDefault(u => u.Email == email && u.Password == password);

                        // User found in the database
                        if (user != null)
                        {
                            FormsAuthentication.SetAuthCookie(email, false);

                            string ReturnUrl = "/Home/Portal";

                            return Json(new { portal = ReturnUrl, success = true });
                        }

                    }
                    catch (Exception e)
                    {
                        return Json(new { success = false, message = e.InnerException.Message.ToString() });
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult ResetPassword(int id)
        {
            tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (user != null)
            {
                // generate ourselves a new random password of 8 characters in length.
                string password = RandomPassword.Generate(8);

                try
                {
                    EmailPassword.Send(user, password);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "Error", Message = "Unable to email the password. Error: " + e.InnerException.Message.ToString() });
                }

                user.Password = password;
                db.SaveChanges();
            }

            return Json(new { Result = "Success", Message = "Wooty Woot" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Clear();  // This may not be needed -- but can't hurt
            Session.Abandon();

            // Clear authentication cookie
            HttpCookie rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            rFormsCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(rFormsCookie);

            // Clear session cookie 
            HttpCookie rSessionCookie = new HttpCookie("ASP.NET_SessionId", "");
            rSessionCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(rSessionCookie);

            return RedirectToAction("Index", "Home");
        }
    }
}