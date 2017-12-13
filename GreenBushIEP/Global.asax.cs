using GreenBushIEP.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;


namespace GreenBushIEP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilters.Filters.Add(new UserProfileFilter(), 0);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    try
                    {
                        //let us take out the username now                
                        string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                        string roles = string.Empty;

                        using (IndividualizedEducationProgramEntities entities = new IndividualizedEducationProgramEntities())
                        {
                            tblUser user = entities.tblUsers.SingleOrDefault(u => u.Email == username);
                            roles = user.RoleID;
                        }
                        //let us extract the roles and names from our own custom cookie


                        //Let us set the Pricipal with our user specific details
                        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                          new System.Security.Principal.GenericIdentity(username, "Forms"), roles.Split(';'));
                    }
                    catch (Exception)
                    {
                        //somehting went wrong
                    }
                }
            }
        }
    }

    public class UserProfileFilter : ActionFilterAttribute
    {

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            string username = HttpContext.Current.User.Identity.Name;
            using (IndividualizedEducationProgramEntities entities = new IndividualizedEducationProgramEntities())
            {
                tblUser userProfile = entities.tblUsers.FirstOrDefault(u => u.Email == username);

                if (userProfile != null)
                {
                    filterContext.Controller.ViewBag.DisplayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(userProfile.FirstName) + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(userProfile.LastName);
                }
            }
        }

    }
}
