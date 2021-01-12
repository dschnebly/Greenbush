﻿using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class ILPController : Controller
    {

        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: ILP
        public ActionResult Index()
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (user != null)
            {

                ILPPortalViewModel model = new ILPPortalViewModel
                {
                    User = user,
                    Locations = db.tbl_ILP_UserLocations.Where(l => l.UserID == user.UserID).ToList(),
                    Programs = db.tbl_ILP_Programs.ToList(),
                    Students = db.usp_ILP_UserList(user.UserID, null, null, false).ToList()
                };

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult FilterUserList(string LocationId, int? RoleId = null, string ProgramId = null, int? Archived = 0)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (user != null)
            {
                if (LocationId == "-1") { LocationId = null; } // Any Location
                if (ProgramId == "-1") { ProgramId = null; } // Any Program
                bool? isArchived = Archived == -1 ? (bool?)null : Archived == 1;  // Archived

                List<usp_ILP_UserList_Result> userList = db.usp_ILP_UserList(user.UserID, LocationId, ProgramId, isArchived).ToList();
                if (RoleId != -1)
                {
                    userList = userList.Where(u => u.RoleID == RoleId).ToList();
                }

                return Json(new { Result = "success", Message = userList }, JsonRequestBehavior.AllowGet);
            }

            // Unknow user or view.
            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateUser()
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (user != null)
            {
                ILPUser model = new ILPUser
                {
                    User = user,
                    Locations = db.vw_ILP_Locations.ToList(),
                    Buildings = db.tblBuildings.ToList()
                };

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

    }
}