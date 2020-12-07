using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class ILPController : Controller
    {

        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: ILP
        public ActionResult Index()
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                ILPPortalViewModel model = new ILPPortalViewModel
                {
                    User = MIS,
                    Students = db.usp_ILP_UserList(MIS.UserID, "S0521-7432", 2, false).ToList()
                };

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}