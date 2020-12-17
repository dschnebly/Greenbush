using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GreenbushIep.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(FormCollection collection)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                string email = collection["Email"];

                tblUser user = db.tblUsers.Where(u => u.Email == email).FirstOrDefault();

                // we silently fail if the email entered doesn't match a system user.
                if (user != null)
                {
                    ResetPassword(user.UserID);
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string Email, string Password)
        {
            // Lets first check if the Model is valid or not
            if (ModelState.IsValid)
            {
                string email = Email;
                string password = Password;

                // Now if our password was enctypted or hashed we would have done the
                // same operation on the user entered password here, But for now
                // since the password is in plain text lets just authenticate directly
                try
                {
                    tblUser user = db.tblUsers.FirstOrDefault(u => u.Email == email);
                    if (user != null && user.Archive.HasValue && user.Archive.Value)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    byte[] saltBytes = user.Salt;
                    byte[] hashBytes = user.Password;
                    PasswordHash hash = new PasswordHash(saltBytes, hashBytes);

                    if (hash.Verify(password))
                    {
                        //User found in the database
                        if (user != null)
                        {
                            FormsAuthentication.SetAuthCookie(email, false);

                            List<uspGetUserBooks_Result> books = db.uspGetUserBooks(user.UserID, email).ToList();
                            if (books.Count == 1 && books.Any(b => b.BookID == "_IEP_"))
                            {
                                // IEP Portal
                                return Json(new { portal = "/Home/Portal?logon=1", success = true });
                            }
                            else if (books.Count == 1 && books.Any(b => b.BookID == "_ILP_"))
                            {
                                // ILP Portal
                                return Json(new { portal = "/ILP/Index", success = true });
                            }
                            else if(books.Count > 1)
                            {
                                // choose your own adventure
                                return Json(new { portal = "/Account/ChooseYourBackpack", success = true });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = e.InnerException.Message.ToString() });
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(int id)
        {
            tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (user != null)
            {
                try
                {
                    // generate ourselves a new random password of 8 characters in length.
                    string emailPassword = RandomPassword.Generate(10);
                    PasswordHash hash = new PasswordHash(emailPassword);

                    user.Password = hash.Hash;
                    user.Salt = hash.Salt;

                    EmailPassword.Send(user, emailPassword);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "Error", Message = "Unable to email the password. Error: " + e.InnerException.Message.ToString() });
                }

                db.SaveChanges();
            }

            return Json(new { Result = "Success", Message = "Wooty Woot" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ResetMyPassword(int id, string password, bool sendEmail = false)
        {
            tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (user != null)
            {
                try
                {
                    PasswordHash hash = new PasswordHash(password);

                    user.Password = hash.Hash;
                    user.Salt = hash.Salt;

                    db.SaveChanges();

                    if (sendEmail)
                    {
                        EmailPassword.Send(user, password);
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Result = "Error", Message = "Unable to email the password. Error: " + e.InnerException.Message.ToString() });
                }

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
            HttpCookie rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(rFormsCookie);

            // Clear session cookie 
            HttpCookie rSessionCookie = new HttpCookie("ASP.NET_SessionId", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(rSessionCookie);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult WhichBackpack()
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            List<uspGetUserBooks_Result> books = db.uspGetUserBooks(user.UserID, user.Email).ToList();
            if (books.Count == 1 && books.Any(b => b.BookID == "_IEP_"))
            {
                // IEP Portal
                return RedirectToAction("Portal", "Home");
            }
            else if (books.Count == 1 && books.Any(b => b.BookID == "_ILP_"))
            {
                // ILP Portal
                return RedirectToAction("Index", "ILP");
            }
            else if (books.Count > 1)
            {
                // choose your own adventure
                return RedirectToAction("ChooseYourBackpack");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ChooseYourBackpack()
        {
            return View();
        }
    }
}