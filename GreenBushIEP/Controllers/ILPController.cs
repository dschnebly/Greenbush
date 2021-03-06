﻿using GreenBushIEP.Models;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
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

        [HttpGet]
        [Authorize]
        public ActionResult LearnerProcedures(int stid, int? rtnId = null)
        {
            StudentProcedureViewModel model = new StudentProcedureViewModel();

            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            tblUser learner = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();
            tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == learner.UserID).FirstOrDefault();
            tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
            tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

            ViewBag.UserRoleId = currentUser.RoleID;
            //ViewBag.ReturnBtn = rtnId.HasValue ? string.Format("/Home/TeacherStudentsRole/{0}", rtnId.Value) : ""; //re-route back button

            if (learner != null)
            {
                model.student = learner;
                model.birthDate = info.DateOfBirth;
                model.KIDSID = info.KIDSID.ToString();

                //We need a ILPModel to get us what we need
                tblIEP ILP = db.tblIEPs.Where(u => u.UserID == learner.UserID).FirstOrDefault();
                if(ILP != null)
                {
                    ViewBag.ILPid = ILP.IEPid;
                }

                //model.PRIMARYPARENT = relationship != null;
                //model.STUDENTGRADE = theIEP.studentGrade;
                //model.STUDENTCODE = theIEP.studentCode;
                //model.STUDENTSERVICES = db.tblServices.Where(s => s.IEPid == iepID).OrderBy(s => s.StartDate).ThenBy(s => s.ServiceCode).ToList();
                //model.studentAge = theIEP.GetCalculatedAge(info.DateOfBirth, model.isDoc);

                //// need to check if transition plan is required and completed
                //if (theIEP.isTransitionNeeded(model.studentAge, model.isDoc) && !model.isGiftedOnly && (theIEP.iepStatusType == "DRAFT" || theIEP.iepStatusType == "AMENDMENT"))
                //{
                //    if (theIEP.isTransitionCompleted == false && theIEP.isAllCompleted)
                //    {
                //        //transition plan must be completed
                //        theIEP.isAllCompleted = false;
                //    }
                //}

            }

            //switch (model.studentIEP.iepStatusType)
            //{
            //    case IEPStatus.PLAN:
            //        return View(model); //PLAN
            //    case IEPStatus.ACTIVE:
            //        return View("~/Views/Home/ActiveIEP/index.cshtml", model); //ACTIVE
            //    case IEPStatus.AMENDMENT:
            //        return View("~/Views/Home/AmmendmentIEP/index.cshtml", model); //AMMENDMENT
            //    case IEPStatus.DRAFT:
            //        if (model.studentIEP.anyStudentIEPActive && !model.studentIEP.current.Amendment) //ANNUAL
            //        {
            //            return View("~/Views/Home/AnnualIEP/index.cshtml", model);
            //        }
            //        return View("~/Views/Home/DraftIEP/index.cshtml", model);   //DRAFT
            //}

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult LoadModuleSection(int studentId, int ilpId, string view)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(s => s.UserID == studentId);

            ViewBag.modifiedByFullName = string.Empty;
            //ViewBag.studentName = student.FirstName + " " + student.LastName;
            //tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            //bool isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

            try
            {
                tblUser modifier = db.tblUsers.FirstOrDefault();
                switch (view)
                {
                    case "EducationHistoryModule":
                        return PartialView("_EducationHistoryModule");
                    case "AssessmentModule":
                        return PartialView("_AssessmentModule");
                    case "EducationPlanModule":
                        return PartialView("_EducationPlanModule");
                    case "ProgressCreditsModule":
                        return PartialView("_ProgressCreditModule");
                    case "DocumentStorageModule":
                        return PartialView("_DocumentStorageModule");
                    default:
                        return Json(new { Result = "error", Message = "Unknown View" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateEducationHistory()
        {
            return RedirectToAction("LearnerProcedures", new { stid = 4352 });
        }
    }
}