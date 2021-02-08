using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private const string owner = "1"; //level 5
        private const string mis = "2"; //level 4
        private const string admin = "3"; //level 3
        private const string teacher = "4"; //level 2
        private const string student = "5";
        private const string nurse = "6"; //level 1

        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: Manage
        public ActionResult Index()
        {
            return View();
        }

        // GET: Manage/Details/5
        [HttpPost]
        public ActionResult Details(int id)
        {
            tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
            List<tblDistrict> districts = db.tblDistricts.ToList();
            var buildings = from b in db.tblBuildings
                            join bm in db.tblBuildingMappings on b.BuildingID equals bm.BuildingID
                            where bm.UserID == id & b.Active == 1
                            select new { b.BuildingName, b.BuildingID, b.USD };

            if (user != null)
            {
                return Json(new { Result = "success", User = user, Districts = districts, Buildings = buildings });
            }

            return Json(new { Result = "error", User = user });
        }

        // GET: Manage/Create
        public ActionResult Create()
        {
            UserDetailsViewModel model = new UserDetailsViewModel
            {
                submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name)
            };
            model.districts = model.submitter.RoleID == "1" ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            return View("~/Views/Home/CreateUser.cshtml", model);
        }

        // POST: Manage/Create
        [HttpPost]
        public ActionResult Create(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                string emailPassword = RandomPassword.Generate(10);
                PasswordHash hash = new PasswordHash(emailPassword);

                // CREATE new user
                tblUser user = new tblUser
                {
                    TeacherID = (!string.IsNullOrEmpty(collection["teacherid"]) ? collection["teacherid"] : null),
                    RoleID = collection["role"],
                    FirstName = collection["firstname"],
                    LastName = collection["lastname"],
                    Email = collection["email"],
                    Create_Date = DateTime.Now,
                    Update_Date = DateTime.Now,
                    Password = hash.Hash,
                    Salt = hash.Salt
                };

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(adminpersona.FileName);
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), fileName);
                    user.ImageURL = fileName;
                    adminpersona.SaveAs(path);
                }

                if (db.tblUsers.Any(o => o.Email == user.Email))
                {
                    return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                }

                // Add to Database
                db.tblUsers.Add(user);
                db.SaveChanges();

                List<tblOrganizationMapping> districtMappings = new List<tblOrganizationMapping>();
                List<tblBuildingMapping> buildingMappings = new List<tblBuildingMapping>();
                List<string> districts = new List<string>();
                List<string> buildings = new List<string>();

                if (collection["misDistrict"] != null)
                {
                    districts = new List<string>(collection["misDistrict"].ToString().Split(','));
                }

                if (collection["AttendanceBuildingId"] != null)
                {
                    buildings = new List<string>(collection["AttendanceBuildingId"].ToString().Split(','));
                }

                // save the user to all the districts that were selected.
                foreach (string usd in collection["misDistrict"].ToString().Split(','))
                {
                    tblOrganizationMapping org = new tblOrganizationMapping
                    {
                        AdminID = submitter.UserID,
                        UserID = user.UserID,
                        USD = usd,
                        Create_Date = DateTime.Now
                    };

                    db.tblOrganizationMappings.Add(org);
                    db.SaveChanges();

                    tblBuildingMapping district = new tblBuildingMapping
                    {
                        BuildingID = "0",
                        USD = usd,
                        UserID = user.UserID,
                        Create_Date = DateTime.Now
                    };

                    db.tblBuildingMappings.Add(district);
                    db.SaveChanges();
                }

                tblUserRole roles = new tblUserRole()
                {
                    UserID = user.UserID,
                    RoleID = Convert.ToInt32(user.RoleID),
                    BookID = "_IEP_"
                };
                db.tblUserRoles.Add(roles);
                db.SaveChanges();

                // removes any buildings not in the current list of usd's.
                List<tblBuilding> userBuildings = db.tblBuildings.Where(b => buildings.Contains(b.BuildingID) && districts.Contains(b.USD) && b.BuildingID != "0").ToList();

                if (buildings != null)
                {
                    foreach (tblBuilding building in userBuildings)
                    {
                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = building.BuildingID,
                            UserID = user.UserID,
                            USD = building.USD,
                            Create_Date = DateTime.Now,
                        });
                    }
                }

                db.tblBuildingMappings.AddRange(buildingMappings);
                db.tblAuditLogs.Add(new tblAuditLog() { Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", ModifiedBy = submitter.UserID, UserID = user.UserID, Value = "Created User " + user.FirstName + " " + user.LastName });
                db.SaveChanges();

                // Email the new password to the user.
                EmailPassword.Send(user, emailPassword);

                return Json(new { Result = "success", Message = "Successfully created a new user." });
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message + " Contact an adminstrator for additional help" });
            }
        }

        // POST: Manage/Create
        [HttpPost]
        public ActionResult CreateILPUser(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                string emailPassword = RandomPassword.Generate(10);
                PasswordHash hash = new PasswordHash(emailPassword);

                // CREATE new user
                tblUser user = new tblUser
                {
                    TeacherID = (!string.IsNullOrEmpty(collection["teacherid"]) ? collection["teacherid"] : null),
                    RoleID = collection["role"],
                    FirstName = collection["firstname"],
                    LastName = collection["lastname"],
                    Email = collection["email"],
                    Create_Date = DateTime.Now,
                    Update_Date = DateTime.Now,
                    Password = hash.Hash,
                    Salt = hash.Salt
                };

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(adminpersona.FileName);
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), fileName);
                    user.ImageURL = fileName;
                    adminpersona.SaveAs(path);
                }

                if (db.tblUsers.Any(o => o.Email == user.Email))
                {
                    return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                }

                // Add to Database
                db.tblUsers.Add(user);
                db.SaveChanges();

                List<tblOrganizationMapping> districtMappings = new List<tblOrganizationMapping>();
                List<tblBuildingMapping> buildingMappings = new List<tblBuildingMapping>();
                List<string> districts = new List<string>();
                List<string> buildings = new List<string>();

                if (collection["misDistrict"] != null)
                {
                    districts = new List<string>(collection["misDistrict"].ToString().Split(','));
                }

                // save the user to all the districts that were selected.
                foreach (string usd in collection["misDistrict"].ToString().Split(','))
                {
                    tblOrganizationMapping org = new tblOrganizationMapping
                    {
                        AdminID = submitter.UserID,
                        UserID = user.UserID,
                        USD = usd,
                        Create_Date = DateTime.Now
                    };

                    db.tblOrganizationMappings.Add(org);
                    db.SaveChanges();

                    tblBuildingMapping district = new tblBuildingMapping
                    {
                        BuildingID = "0",
                        USD = usd,
                        UserID = user.UserID,
                        Create_Date = DateTime.Now
                    };

                    db.tblBuildingMappings.Add(district);
                    db.SaveChanges();
                }

                tblUserRole roles = new tblUserRole()
                {
                    UserID = user.UserID,
                    RoleID = Convert.ToInt32(user.RoleID),
                    BookID = "_ILP_"
                };

                db.tblUserRoles.Add(roles);
                db.tblAuditLogs.Add(new tblAuditLog() { Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", ModifiedBy = submitter.UserID, UserID = user.UserID, Value = "Created User " + user.FirstName + " " + user.LastName });
                db.SaveChanges();

                // Email the new password to the user.
                EmailPassword.Send(user, emailPassword);

                return Json(new { Result = "success", Message = "Successfully created a new user." });
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message + " Contact an adminstrator for additional help" });
            }
        }

        [HttpPost]
        public ActionResult FilterReferrals(int searchType)
        {
            tblUser currentUser = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            bool? completeType = null;

            if (searchType == 1)
            {
                completeType = false;
            }
            else if (searchType == 2)
            {
                completeType = true;
            }

            List<ReferralViewModel> referralList = new List<ReferralViewModel>();

            if (currentUser.RoleID == nurse || currentUser.RoleID == teacher)
            {

                IQueryable<tblReferralInfo> referrals = (from refInfo in db.tblReferralInfoes
                                                         join rr in db.tblReferralRequests on refInfo.ReferralID equals rr.ReferralID
                                                         where
                                                         refInfo.UserID == currentUser.UserID
                                                         && rr.UserID_Requster == currentUser.UserID
                                                         && rr.Complete == false
                                                         && rr.Submit_Date == null
                                                         select refInfo).Distinct();

                foreach (tblReferralInfo referral in referrals)
                {

                    ReferralViewModel model = new ReferralViewModel();

                    tblReferralRequest request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();
                    if (request != null)
                    {
                        model.submitDate = request.Create_Date.ToShortDateString();
                        model.isComplete = false;
                    }
                    model.referralId = referral.ReferralID;
                    model.lastName = referral.LastName;
                    model.firstName = referral.FirstName;
                    model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                    model.notes = referral.ReferralNotes;
                    referralList.Add(model);
                }

            }
            else
            {

                IQueryable<tblOrganizationMapping> districts = (from org in db.tblOrganizationMappings
                                                                join user in db.tblUsers
                                                                    on org.UserID equals user.UserID
                                                                where (user.UserID == currentUser.UserID)
                                                                select org).Distinct();

                if (districts != null)
                {


                    foreach (tblOrganizationMapping district in districts)
                    {

                        IQueryable<tblReferralInfo> referrals = (from refInfo in db.tblReferralInfoes
                                                                 join rr in db.tblReferralRequests
                                                                     on refInfo.ReferralID equals rr.ReferralID
                                                                 where
                                                                 (refInfo.AssignedUSD == district.USD)
                                                                 && rr.Submit_Date != null
                                                                 && ((completeType == null) || (rr.Complete == completeType.Value))
                                                                 select refInfo).Distinct();

                        foreach (tblReferralInfo referral in referrals)
                        {
                            //if duplicated skip

                            ReferralViewModel model = new ReferralViewModel();
                            tblReferralRequest request = null;

                            if (db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).Count() > 0 && completeType != null)
                            {
                                request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID && o.Complete == completeType).FirstOrDefault();
                            }
                            else if (db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).Count() > 0 && completeType == null)
                            {
                                request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).OrderByDescending(o => o.Complete).FirstOrDefault();
                            }
                            else
                            {
                                request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();
                            }

                            if (request != null)
                            {
                                model.submitDate = request.Create_Date.ToShortDateString();
                                model.isComplete = request.Complete;
                            }

                            model.referralId = referral.ReferralID;
                            model.lastName = referral.LastName;
                            model.firstName = referral.FirstName;
                            model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                            model.notes = referral.ReferralNotes;
                            referralList.Add(model);
                        }
                    }

                }
            }

            return Json(new { Result = "success", FilterList = referralList.OrderBy(o => o.lastName).ThenBy(o => o.firstName).ToList() });
        }

        // GET: Manage/Referrals
        [HttpGet]
        public ActionResult Referrals()
        {
            tblUser currentUser = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (currentUser.RoleID == nurse || currentUser.RoleID == teacher)
            {
                List<ReferralViewModel> referralList = new List<ReferralViewModel>();

                IQueryable<tblReferralInfo> referrals = (from refInfo in db.tblReferralInfoes
                                                         join rr in db.tblReferralRequests on refInfo.ReferralID equals rr.ReferralID
                                                         where
                                                         refInfo.UserID == currentUser.UserID
                                                         && rr.UserID_Requster == currentUser.UserID
                                                         && rr.Complete == false
                                                         && rr.Submit_Date == null
                                                         select refInfo).Distinct();

                foreach (tblReferralInfo referral in referrals)
                {

                    ReferralViewModel model = new ReferralViewModel();

                    tblReferralRequest request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();
                    if (request != null)
                    {
                        var submitter = db.tblUsers.Where(o => o.UserID == request.UserID_Requster).FirstOrDefault();

                        model.submitDate = request.Create_Date.ToShortDateString();
                        model.isComplete = false;
                        model.submittedBy = submitter != null ? string.Format("{0} {1}", submitter.FirstName, submitter.LastName) : "";
                    }

                    model.referralId = referral.ReferralID;
                    model.lastName = referral.LastName;
                    model.firstName = referral.FirstName;
                    model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                    model.notes = referral.ReferralNotes;

                    referralList.Add(model);
                }

                ViewBag.Referrals = referralList.OrderBy(o => o.lastName).ThenBy(o => o.firstName).ToList();
                ViewBag.CanCreate = true;
                return View("~/Views/Home/Referrals.cshtml");
            }

            else
            {
                IQueryable<tblOrganizationMapping> districts = (from org in db.tblOrganizationMappings
                                                                join user in db.tblUsers
                                                                    on org.UserID equals user.UserID
                                                                where (user.UserID == currentUser.UserID)
                                                                select org).Distinct();

                List<ReferralViewModel> referralList = new List<ReferralViewModel>();
                if (districts != null)
                {
                    foreach (tblOrganizationMapping district in districts)
                    {

                        IQueryable<tblReferralInfo> referrals = (from refInfo in db.tblReferralInfoes
                                                                 join rr in db.tblReferralRequests
                                                                     on refInfo.ReferralID equals rr.ReferralID
                                                                 where
                                                                 (refInfo.AssignedUSD == district.USD)
                                                                 && rr.Complete == false
                                                                 && rr.Submit_Date != null
                                                                 select refInfo).Distinct();

                        foreach (tblReferralInfo referral in referrals)
                        {
                            //if duplicated skip
                            if (!referral.tblReferralRequests.Any(o => o.Complete == true))
                            {
                                ReferralViewModel model = new ReferralViewModel();

                                tblReferralRequest request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();
                                if (request != null)
                                {
                                    var submitter = db.tblUsers.Where(o => o.UserID == request.UserID_Requster).FirstOrDefault();

                                    model.submitDate = request.Submit_Date.HasValue ? request.Submit_Date.Value.ToShortDateString() : request.Create_Date.ToShortDateString();
                                    model.isComplete = request.Complete;
                                    model.submittedBy = submitter != null ? string.Format("{0} {1}", submitter.FirstName, submitter.LastName) : "";
                                }
                                model.referralId = referral.ReferralID;
                                model.lastName = referral.LastName;
                                model.firstName = referral.FirstName;
                                model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                                model.notes = referral.ReferralNotes;
                                referralList.Add(model);
                            }

                        }
                    }

                }

                ViewBag.Referrals = referralList.OrderBy(o => o.lastName).ThenBy(o => o.firstName).ToList();
                ViewBag.CanCreate = false;

                return View("~/Views/Home/Referrals.cshtml");
            }

        }

        // GET: Manage/EditReferral
        [HttpGet]
        public ActionResult EditReferral(int id)
        {
            ReferralDetailsViewModel model = new ReferralDetailsViewModel();
            bool isComplete = false;
            model.student = new tblReferralInfo();

            tblReferralRequest referralReq = db.tblReferralRequests.Where(o => o.ReferralID == id).FirstOrDefault();
            if (referralReq != null)
            {
                isComplete = referralReq.Complete;
            }

            tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == id).FirstOrDefault();
            if (student != null)
            {
                model.student = student;
                model.request = db.tblReferralRequests.Where(u => u.ReferralID == id).FirstOrDefault();
                if (!string.IsNullOrEmpty(model.student.AttendingUSD))
                {
                    string[] attendingUSDs = model.student.AttendingUSD.Split(',');
                    model.selectedDistrict = (from d in db.tblDistricts join o in db.tblOrganizationMappings on d.USD equals o.USD where attendingUSDs.Contains(o.USD) select d).Distinct().ToList();
                }
            }

            List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(r => r.ReferralID == id).ToList();
            if (relationships != null)
            {
                foreach (tblReferralRelationship relationship in relationships)
                {
                    model.contacts.Add(new tblStudentRelationship()
                    {
                        FirstName = relationship.FirstName,
                        MiddleName = relationship.MiddleName,
                        LastName = relationship.LastName,
                        Address1 = relationship.Address1,
                        Address2 = relationship.Address2,
                        City = relationship.City,
                        State = relationship.State,
                        Zip = relationship.Zip,
                        Email = relationship.Email,
                        Phone = relationship.Phone,
                        Realtionship = relationship.Realtionship,
                        RealtionshipID = relationship.RealtionshipID,
                        PrimaryContact = relationship.PrimaryContact.HasValue && relationship.PrimaryContact == 1 ? 1 : 0
                    });
                }
            }

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.allDistricts = db.tblDistricts.ToList();
            model.districts = model.submitter.RoleID == "1" ? model.allDistricts.Where(d => d.Active == 1).ToList() : (from d in model.allDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == student.AssignedUSD
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();


            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();



            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            ViewBag.ReferralComplete = isComplete;

            return View("~/Views/Home/EditReferral.cshtml", model);
        }


        [HttpPost]
        public JsonResult DeleteReferral(int referralId)
        {
            try
            {
                List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(r => r.ReferralID == referralId).ToList();
                if (relationships != null && relationships.Count > 0)
                {
                    db.tblReferralRelationships.RemoveRange(relationships);
                }

                tblReferralInfo referral = db.tblReferralInfoes.Where(r => r.ReferralID == referralId).FirstOrDefault();
                if (referral != null)
                {
                    db.tblReferralInfoes.Remove(referral);
                }

                tblReferralRequest referralReq = db.tblReferralRequests.Where(r => r.ReferralID == referralId).FirstOrDefault();
                if (referralReq != null)
                {
                    db.tblReferralRequests.Remove(referralReq);
                }

                db.SaveChanges();

            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "There was an error while trying to delete the Referral. \n\n" + e.InnerException.ToString() });
            }

            return Json(new { Result = "success", Message = "The student was successfully deleted." });


        }

        [HttpPost]
        public JsonResult EditReferral(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // check that the kidsIS doesn't already exsist in the system.
                    long kidsID = collection["kidsid"] == null ? 000000000 : Convert.ToInt64(collection["kidsid"]);
                    int referralId = Convert.ToInt32(collection["referralId"]);

                    //if (kidsID == 0)
                    //{
                    //    return Json(new { Result = "error", Message = "The KIDS ID is invalid. Please enter another KIDS ID." });
                    //}

                    tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID && kidsID != 000000000).FirstOrDefault();
                    tblReferralRequest referralReq = db.tblReferralRequests.Where(o => o.ReferralID == referralId).FirstOrDefault();

                    if (exsistingStudent != null)
                    {
                        if (referralReq != null && referralReq.Complete)
                        {
                            return Json(new { Result = "error", Message = "This Referral was already completed and the student has been created. Please contact Greenbush." });
                        }

                        //student has been created but it is not complete - don't create new record
                        return Json(new { Result = "error", Message = "The student is already in the Greenbush system. Please contact Greenbush." });
                    }


                    if (!string.IsNullOrEmpty(collection["email"]))
                    {
                        string email = collection["email"].ToString();
                        if (db.tblUsers.Any(o => o.Email == email))
                        {
                            return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                        }
                    }

                    // Create New StudentInfo
                    DateTime dobDate = DateTime.MinValue;

                    DateTime.TryParse(collection["dob"], out dobDate);

                    if (dobDate == DateTime.MinValue)
                    {
                        return Json(new { Result = "error", Message = "The Birthdate supplied in not a valid date." });
                    }

                    if (collection["misDistrict"] == null)
                    {
                        return Json(new { Result = "error", Message = "The Assigned USD is required." });
                    }

                    if (collection["AttendanceBuildingId"] == null)
                    {
                        return Json(new { Result = "error", Message = "The AttendanceBuildingId is required." });
                    }

                    if (collection["AttendanceBuildingId"] == null)
                    {
                        return Json(new { Result = "error", Message = "The Attendance Building is required." });
                    }

                    if (collection["NeighborhoodBuildingID"] == null)
                    {
                        return Json(new { Result = "error", Message = "The Neighborhood Building is required." });
                    }

                    if (collection["assignChildCount"] == null)
                    {
                        return Json(new { Result = "error", Message = "Assign Child Count is required." });
                    }

                    // tblStudentInfo
                    tblReferralInfo referInfo = db.tblReferralInfoes.Where(o => o.ReferralID == referralId).FirstOrDefault();

                    //UserID = student.UserID,
                    referInfo.FirstName = collection["firstname"];
                    referInfo.MiddleInitial = collection["middlename"];
                    referInfo.LastName = collection["lastname"];
                    referInfo.KIDSID = kidsID;
                    referInfo.DateOfBirth = dobDate;
                    referInfo.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                    referInfo.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                    referInfo.AssignedUSD = collection["assignChildCount"] != null ? collection["assignChildCount"].ToString() : "";
                    referInfo.AttendingUSD = collection["misDistrict"];
                    referInfo.ResponsibleBuildingID = collection["AttendanceBuildingId"];
                    referInfo.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                    referInfo.Status = "PENDING";
                    referInfo.Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                    referInfo.Update_Date = DateTime.Now;
                    referInfo.PlacementCode = collection["studentPlacement"];
                    referInfo.ClaimingCode = collection["claimingCode"] != null && collection["claimingCode"] == "on" ? true : false;
                    referInfo.isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on" ? true : false;
                    referInfo.Email = collection["email"];
                    referInfo.ImageURL = collection["adminpersona"];

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        // Retrieve the error messages as a list of strings.
                        IEnumerable<string> errorMessages = ex.EntityValidationErrors
                                .SelectMany(x => x.ValidationErrors)
                                .Select(x => x.ErrorMessage);

                        // Join the list to a single string.
                        string fullErrorMessage = string.Join("; ", errorMessages);

                        // Combine the original exception message with the new one.
                        string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + exceptionMessage });
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    return Json(new { Result = "success", Message = referralId });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the user. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult EditReferralOptions(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (collection["referralId"] == null)
                    {
                        return Json(new { Result = "error", Message = "The referralId ID is missing. Please contact a system administrator to review the problem." });
                    }

                    int referralId = Convert.ToInt32(collection["referralId"]);


                    tblReferralInfo info = db.tblReferralInfoes.Where(i => i.ReferralID == referralId).FirstOrDefault();
                    if (info != null)
                    {
                        info.Address1 = collection["studentStreetAddress1"].ToString();
                        info.Address2 = collection["studentStreetAddress2"].ToString();
                        info.City = collection["studentCity"].ToString();
                        info.State = collection["studentState"].ToString();
                        info.Zip = collection["studentZipCode"].ToString();
                        info.County = collection["studentCounty"].ToString();
                        info.Grade = Convert.ToInt32(collection["studentGrade"]);
                        info.RaceCode = collection["studentRace"].ToString();
                        info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
                        info.Ethicity = collection["studentEthnic"].ToString();
                        info.StudentLanguage = collection["studentLanguage"].ToString();
                        info.ParentLanguage = collection["parentLanguage"].ToString();
                        info.ClaimingCode = collection["claimingCode"] == "on" ? true : false;
                        info.FullDayKG = collection["fullDayKindergarten"] == "on" ? true : false;
                        info.StatusCode = collection["statusCode"].ToString();
                        info.ExitNotes = collection["exitNotes"];
                        info.Update_Date = DateTime.Now;

                        if (!string.IsNullOrEmpty(collection["initialIEPDate"]))
                        {
                            info.InitialIEPDate = Convert.ToDateTime(collection["initialIEPDate"]);
                        }
                        else
                        {
                            info.InitialIEPDate = null;
                        }

                        if (!string.IsNullOrEmpty(collection["exitDate"]))
                        {
                            info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
                        }
                        else
                        {
                            info.ExitDate = null;
                        }

                        if (!string.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }
                        else
                        {
                            info.InitialEvalConsentSigned = null;
                        }

                        if (!string.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                        {
                            info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                        }
                        else
                        {
                            info.InitialEvalDetermination = null;
                        }

                        if (!string.IsNullOrEmpty(collection["reEvaluationSignature"]))
                        {
                            info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                        }
                        else
                        {
                            info.ReEvalConsentSigned = null;
                        }

                        if (!string.IsNullOrEmpty(collection["reEvalCompleted"]))
                        {
                            info.ReEvalCompleted = Convert.ToDateTime(collection["reEvalCompleted"]);
                        }
                        else
                        {
                            info.ReEvalCompleted = null;
                        }
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = referralId });
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to add the student information. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to update the students options. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult EditReferralContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (collection["referralId"] == null)
                    {
                        return Json(new { Result = "error", Message = "The referralId ID is missing. Please contact a system administrator to review the problem." });
                    }

                    int referralId = Convert.ToInt32(collection["referralId"]);

                    //delete old
                    List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(o => o.ReferralID == referralId).ToList();
                    if (relationships.Any())
                    {
                        foreach (tblReferralRelationship existingRelationship in relationships)
                        {
                            db.tblReferralRelationships.Remove(existingRelationship);

                        }
                        db.SaveChanges();
                    }


                    List<string> uniqueKeys = new List<string>();
                    int i = 0;
                    string num = "-1";
                    while (++i < collection.AllKeys.Length)
                    {

                        string nameColl = collection.AllKeys[i];
                        int start = nameColl.IndexOf('[');
                        int end = nameColl.IndexOf(']');
                        string val = nameColl.Substring(start + 1, (end - 1) - start);

                        if (num != val)
                        {
                            uniqueKeys.Add(val);
                        }

                        num = val;

                        i++;
                    }


                    foreach (string keyVal in uniqueKeys)
                    {
                        string labelName = string.Format("contact[{0}].", keyVal);
                        tblReferralRelationship contact = new tblReferralRelationship
                        {
                            ReferralID = referralId,
                            FirstName = collection[string.Format("{0}ContactFirstName", labelName)].ToString(),
                            LastName = collection[string.Format("{0}ContactLastName", labelName)].ToString(),
                            Realtionship = collection[string.Format("{0}ContactRelationship", labelName)].ToString(),
                            Address1 = collection[string.Format("{0}ContactStreetAddress1", labelName)].ToString(),
                            Address2 = collection[string.Format("{0}ContactStreetAddress2", labelName)].ToString(),
                            City = collection[string.Format("{0}ContactCity", labelName)].ToString(),
                            State = collection[string.Format("{0}ContactState", labelName)].ToString(),
                            Zip = collection[string.Format("{0}ContactZipCode", labelName)].ToString(),
                            Phone = collection[string.Format("{0}ContactPhoneNumber", labelName)].ToString(),
                            Email = collection[string.Format("{0}ContactEmail", labelName)].ToString()
                        };

                        if (collection[string.Format("{0}PrimaryContact", labelName)] != null)
                        {
                            contact.PrimaryContact = 1;
                        }

                        db.tblReferralRelationships.Add(contact);
                    }

                    db.SaveChanges();

                    //get teachers list
                    tblReferralInfo info = db.tblReferralInfoes.Where(o => o.ReferralID == referralId).FirstOrDefault();
                    List<TeacherView> teachers = new List<TeacherView>();

                    if (info != null)
                    {
                        teachers = GetTeacherByBuilding(info.ResponsibleBuildingID, info.AssignedUSD);
                    }

                    return Json(new { Result = "success", Message = referralId, teacherList = teachers });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to add the student contacts. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the student's contacts. Please try again or contact your administrator." });
        }

        [HttpPost]
        public ActionResult SubmitStudentReferral(HttpPostedFileBase adminpersona, FormCollection collection)
        {

            int isDraft = Convert.ToInt32(collection["IsDraftSubmit"]);
            int referralID = Convert.ToInt32(collection["referralId"]);

            if (isDraft == 1)
            {
                if (adminpersona != null)
                {
                    try
                    {

                        if (adminpersona.ContentLength > 0)
                        {
                            tblReferralInfo refer = db.tblReferralInfoes.Where(o => o.ReferralID == referralID).FirstOrDefault();

                            string fileName = Path.GetFileName(adminpersona.FileName);
                            string random = Guid.NewGuid() + fileName;
                            string path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                            if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                            }

                            refer.ImageURL = random;
                            refer.Update_Date = DateTime.Now;
                            adminpersona.SaveAs(path);

                            db.SaveChanges();
                        }

                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                int studentId = CreateStudentFromReferral(referralID, submitter.UserID);

                string teacherList = collection["selectedTeachers"];
                if (!string.IsNullOrEmpty(teacherList))
                {
                    int[] teacherArray = Array.ConvertAll(teacherList.Split(','), int.Parse);
                    StudentAssignments(studentId, teacherArray);
                }


                tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                try
                {
                    if (student != null)
                    {
                        //// UPLOAD the image
                        if (adminpersona != null && adminpersona.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(adminpersona.FileName);
                            string random = Guid.NewGuid() + fileName;
                            string path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                            if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                            }

                            student.ImageURL = random;
                            student.Update_Date = DateTime.Now;
                            adminpersona.SaveAs(path);

                            db.SaveChanges();
                        }
                    }

                    IQueryable<tblReferralRequest> rrList = db.tblReferralRequests.Where(o => o.ReferralID == referralID);
                    if (rrList != null)
                    {
                        foreach (tblReferralRequest rr in rrList)
                        {
                            rr.Complete = true;
                            rr.Create_Date = DateTime.Now;
                            rr.Update_Date = DateTime.Now;
                        }

                        db.SaveChanges();

                    }
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to add the student. \n\n" + e.InnerException.ToString() });
                }
            }

            return RedirectToAction("Referrals", "Manage");

        }


        private int CreateStudentFromReferral(int referralId, int submitterId)
        {
            try
            {

                tblReferralInfo referral = db.tblReferralInfoes.Where(o => o.ReferralID == referralId).FirstOrDefault();
                if (referral != null)
                {

                    // Create New User 
                    tblUser newStudent = new tblUser()
                    {
                        RoleID = "5",
                        FirstName = referral.FirstName,
                        MiddleName = referral.MiddleInitial,
                        LastName = referral.LastName,
                        Address1 = referral.Address1,
                        Address2 = referral.Address2,
                        City = referral.City,
                        State = referral.State,
                        Zip = referral.Zip,
                        CreatedBy = submitterId,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };


                    db.tblUsers.Add(newStudent);
                    db.SaveChanges();

                    // Create New StudentInfo				
                    tblStudentInfo studentInfo = new tblStudentInfo()
                    {
                        UserID = newStudent.UserID,
                        KIDSID = referral.KIDSID.HasValue ? referral.KIDSID.Value : 0,
                        DateOfBirth = referral.DateOfBirth.Value,
                        Primary_DisabilityCode = referral.Primary_DisabilityCode,
                        Secondary_DisabilityCode = referral.Secondary_DisabilityCode,
                        AssignedUSD = referral.AssignedUSD,
                        USD = referral.AttendingUSD,
                        BuildingID = referral.ResponsibleBuildingID,
                        NeighborhoodBuildingID = referral.NeighborhoodBuildingID,
                        Status = "PENDING",
                        Gender = referral.Gender,
                        CreatedBy = submitterId,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                        PlacementCode = referral.PlacementCode,
                        ClaimingCode = referral.ClaimingCode.HasValue && referral.ClaimingCode.Value ? true : false,
                        isGifted = referral.isGifted.HasValue && referral.isGifted.Value ? true : false,
                        InitialIEPDate = referral.InitialIEPDate,
                        InitialEvalConsentSigned = referral.InitialEvalConsentSigned,
                        ReEvalCompleted = referral.ReEvalCompleted,
                        ReEvalConsentSigned = referral.ReEvalConsentSigned,
                        InitialEvalDetermination = referral.InitialEvalDetermination,
                        ExitDate = referral.ExitDate,
                        ExitNotes = referral.ExitNotes,
                        County = referral.County,
                        Grade = referral.Grade,
                        Race = referral.Race,
                        RaceCode = referral.RaceCode,
                        Ethicity = referral.Ethicity,
                        ParentLanguage = referral.ParentLanguage,
                        StudentLanguage = referral.StudentLanguage,
                        StatusCode = referral.StatusCode,
                        FullDayKG = referral.FullDayKG,
                        FundSource = referral.FundSource


                    };

                    db.tblStudentInfoes.Add(studentInfo);

                    IQueryable<tblReferralRelationship> contacts = db.tblReferralRelationships.Where(o => o.ReferralID == referralId);
                    foreach (tblReferralRelationship contact in contacts)
                    {

                        tblStudentRelationship newRelationship = new tblStudentRelationship
                        {
                            UserID = newStudent.UserID,
                            FirstName = contact.FirstName,
                            LastName = contact.LastName,
                            Realtionship = contact.Realtionship,
                            Address1 = contact.Address1,
                            Address2 = contact.Address2,
                            City = contact.City,
                            State = contact.State,
                            Zip = contact.Zip,
                            Phone = contact.Phone,
                            Email = contact.Email,
                            PrimaryContact = contact.PrimaryContact.HasValue && contact.PrimaryContact.Value == 1 ? 1 : 0,
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now
                        };
                        db.tblStudentRelationships.Add(newRelationship);

                    }
                    db.SaveChanges();

                    // map the buildings in the building mapping table
                    db.tblBuildingMappings.Add(new tblBuildingMapping() { BuildingID = studentInfo.BuildingID, USD = studentInfo.AssignedUSD, UserID = studentInfo.UserID, Create_Date = DateTime.Now });
                    db.SaveChanges();

                    // save to organization chart
                    // save the user to all the districts that was selected.
                    // tblOrganizationMapping and tblBuildingMapping

                    string districtValues = referral.AttendingUSD;

                    if (!string.IsNullOrEmpty(districtValues))
                    {
                        string[] districtArray = districtValues.Split(',');

                        foreach (string usd in districtArray)
                        {
                            tblOrganizationMapping org = new tblOrganizationMapping
                            {
                                AdminID = submitterId,
                                UserID = newStudent.UserID,
                                USD = usd,
                                Create_Date = DateTime.Now
                            };

                            db.tblOrganizationMappings.Add(org);
                            db.SaveChanges();
                        }
                    }

                    return newStudent.UserID;
                }
            }
            catch
            {
                return -1;
            }


            return -1;
        }
        [HttpGet]
        public ActionResult CreateReferral(int? id)
        {
            var submitterObj = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            ReferralDetailsViewModel model = new ReferralDetailsViewModel
            {
                submitter = submitterObj,
                allDistricts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitterObj.UserID select district).Distinct().ToList()
            };
            model.student.DateOfBirth = DateTime.Now.AddYears(-5);
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();

            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == "101"
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();


            int referralID = id.HasValue ? id.Value : 0;

            tblReferralInfo existingReferral = db.tblReferralInfoes.Where(o => o.ReferralID == referralID).FirstOrDefault();

            if (existingReferral != null)
            {
                ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                     where b.USD == existingReferral.AssignedUSD
                                                     select new BuildingsViewModel
                                                     {
                                                         BuildingName = b.BuildingName,
                                                         BuildingID = b.BuildingID,
                                                         BuildingUSD = b.USD
                                                     }).OrderBy(b => b.BuildingName).ToList();


                //List<vw_BuildingList> buildings = db.vw_BuildingList.Where(b => b.USD == districtId).ToList();

                model.referralId = referralID;
                model.student.FirstName = existingReferral.FirstName;
                model.student.LastName = existingReferral.LastName;
                model.student.MiddleInitial = existingReferral.MiddleInitial;
                model.student.Address1 = existingReferral.Address1;
                model.student.Address2 = existingReferral.Address2;
                model.student.City = existingReferral.City;
                model.student.State = existingReferral.State;
                model.student.Zip = existingReferral.Zip;
                model.student.UserID = existingReferral.ReferralID;
                model.student.ReferralNotes = existingReferral.ReferralNotes;
                model.student.DateOfBirth = existingReferral.DateOfBirth.HasValue ? existingReferral.DateOfBirth.Value : DateTime.Today.AddYears(-5);
                model.request = db.tblReferralRequests.Where(o => o.ReferralID == referralID).FirstOrDefault();

                model.info = new tblStudentInfo()
                {
                    KIDSID = existingReferral.KIDSID.HasValue ? existingReferral.KIDSID.Value : 0,
                    Gender = existingReferral.Gender,
                    County = existingReferral.County,
                    Grade = Convert.ToInt32(existingReferral.Grade),
                    RaceCode = existingReferral.RaceCode,
                    Race = db.tblRaces.Where(r => r.RaceCode == existingReferral.RaceCode).FirstOrDefault().RaceDescription,
                    Ethicity = existingReferral.Ethicity,
                    StudentLanguage = existingReferral.StudentLanguage,
                    ParentLanguage = existingReferral.ParentLanguage,
                    AssignedUSD = existingReferral.AssignedUSD,
                    BuildingID = existingReferral.ResponsibleBuildingID,
                    NeighborhoodBuildingID = existingReferral.NeighborhoodBuildingID,
                    InitialEvalConsentSigned = existingReferral.InitialEvalConsentSigned,
                    Primary_DisabilityCode = existingReferral.Primary_DisabilityCode,
                    Secondary_DisabilityCode = existingReferral.Secondary_DisabilityCode,

                };

                List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(r => r.ReferralID == referralID).ToList();
                if (relationships != null)
                {
                    foreach (tblReferralRelationship relationship in relationships)
                    {
                        model.contacts.Add(new tblStudentRelationship()
                        {
                            FirstName = relationship.FirstName,
                            MiddleName = relationship.MiddleName,
                            LastName = relationship.LastName,
                            Address1 = relationship.Address1,
                            Address2 = relationship.Address2,
                            City = relationship.City,
                            State = relationship.State,
                            Zip = relationship.Zip,
                            Email = relationship.Email,
                            Phone = relationship.Phone,
                            Realtionship = relationship.Realtionship,
                            RealtionshipID = relationship.RealtionshipID,
                            PrimaryContact = relationship.PrimaryContact.HasValue && relationship.PrimaryContact == 1 ? 1 : 0,

                        });
                    }
                }
                else
                {

                    model.contacts.Add(new tblStudentRelationship() { Realtionship = "parent", State = "KS" });
                }


            }



            return View("~/Views/Home/CreateReferral.cshtml", model);
        }

        // POST: Manage/CreateReferral
        [HttpPost]
        public JsonResult CreateReferral(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    tblOrganizationMapping submitterDistrict = (from org in db.tblOrganizationMappings
                                                                join user in db.tblUsers
                                                                    on org.UserID equals user.UserID
                                                                where user.UserID == submitter.UserID
                                                                select org).Distinct().FirstOrDefault();


                    int studentId = (collection["studentId"] == null || collection["studentId"] == "") ? 0 : Convert.ToInt32(collection["studentId"]);

                    // check that the kidsIS doesn't already exsist in the system.
                    string kidsIdStr = collection["kidsid"] == null ? "0" : collection["kidsid"].ToString();
                    long kidsID = 0;
                    int referralRequestId = 0;

                    if (!string.IsNullOrEmpty(kidsIdStr))
                    {
                        kidsID = Convert.ToInt64(kidsIdStr);
                        if (kidsID > 0)
                        {
                            tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID && kidsID != 000000000).FirstOrDefault();
                            if (exsistingStudent != null)
                            {
                                return Json(new { Result = "error", Message = "The student is already in the system. Please contact Greenbush." });
                            }
                        }

                    }

                    string raceCodeVal = collection["studentRace"].ToString();
                    tblReferralInfo existingReferral = db.tblReferralInfoes.Where(o => o.ReferralID == studentId).FirstOrDefault();

                    if (studentId == 0 || existingReferral == null)
                    {
                        // Create New					
                        tblReferralInfo studentInfo = new tblReferralInfo()
                        {
                            UserID = submitter.UserID,
                            FirstName = collection["firstname"],
                            MiddleInitial = collection["middlename"],
                            LastName = collection["lastname"],
                            KIDSID = kidsID,
                            Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F",
                            Address1 = collection["studentStreetAddress1"].ToString(),
                            Address2 = collection["studentStreetAddress2"].ToString(),
                            City = collection["studentCity"].ToString(),
                            State = collection["studentState"].ToString(),
                            Zip = collection["studentZipCode"].ToString(),
                            County = collection["studentCounty"].ToString(),
                            Grade = Convert.ToInt32(collection["studentGrade"]),
                            RaceCode = raceCodeVal,
                            Race = db.tblRaces.Where(r => r.RaceCode == raceCodeVal).FirstOrDefault().RaceDescription,
                            Ethicity = collection["studentEthnic"].ToString(),
                            StudentLanguage = collection["studentLanguage"].ToString(),
                            ParentLanguage = collection["parentLanguage"].ToString(),
                            Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "",
                            Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "",
                            CreatedBy = submitter.UserID,
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now
                        };

                        if (!string.IsNullOrEmpty(collection["dob"]))
                        {
                            studentInfo.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                        }

                        try
                        {
                            db.tblReferralInfoes.Add(studentInfo);
                            db.SaveChanges();

                            tblReferralRequest request = new tblReferralRequest
                            {
                                UserID_Requster = submitter.UserID,
                                UserID_District = submitterDistrict != null ? submitterDistrict.USD : "",
                                ReferralID = studentInfo.ReferralID,
                                ReferralType = collection["referralType"].ToString(),
                                Create_Date = DateTime.Now,
                                Update_Date = DateTime.Now
                            };

                            if (!string.IsNullOrEmpty(collection["enrollmentDate"]))
                            {
                                request.EnrollmentDate = Convert.ToDateTime(collection["enrollmentDate"]);
                            }

                            db.tblReferralRequests.Add(request);
                            db.SaveChanges();

                            referralRequestId = request.ReferalRequestID;

                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to create the referral. \n\n" + e.InnerException.ToString() });
                        }

                        return Json(new { Result = "success", Message = studentInfo.ReferralID });

                    }
                    else
                    {

                        if (existingReferral != null)
                        {

                            existingReferral.FirstName = collection["firstname"];
                            existingReferral.MiddleInitial = collection["middlename"];
                            existingReferral.LastName = collection["lastname"];
                            existingReferral.KIDSID = kidsID;
                            existingReferral.Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                            existingReferral.Address1 = collection["studentStreetAddress1"].ToString();
                            existingReferral.Address2 = collection["studentStreetAddress2"].ToString();
                            existingReferral.City = collection["studentCity"].ToString();
                            existingReferral.State = collection["studentState"].ToString();
                            existingReferral.Zip = collection["studentZipCode"].ToString();
                            existingReferral.County = collection["studentCounty"].ToString();
                            existingReferral.Grade = Convert.ToInt32(collection["studentGrade"]);
                            existingReferral.RaceCode = raceCodeVal;
                            existingReferral.Race = db.tblRaces.Where(r => r.RaceCode == raceCodeVal).FirstOrDefault().RaceDescription;
                            existingReferral.Ethicity = collection["studentEthnic"].ToString();
                            existingReferral.StudentLanguage = collection["studentLanguage"].ToString();
                            existingReferral.ParentLanguage = collection["parentLanguage"].ToString();
                            existingReferral.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                            existingReferral.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                            existingReferral.CreatedBy = submitter.UserID;
                            existingReferral.Create_Date = DateTime.Now;
                            existingReferral.Update_Date = DateTime.Now;
                        };

                        if (!string.IsNullOrEmpty(collection["dob"]))
                        {
                            existingReferral.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                        }
                        else
                        {
                            existingReferral.DateOfBirth = null;
                        }

                        tblReferralRequest existingRequest = db.tblReferralRequests.Where(o => o.ReferralID == studentId).FirstOrDefault();
                        if (existingRequest != null)
                        {
                            existingRequest.ReferralType = collection["referralType"].ToString();
                            if (!string.IsNullOrEmpty(collection["enrollmentDate"]))
                            {
                                existingRequest.EnrollmentDate = Convert.ToDateTime(collection["enrollmentDate"]);
                            }
                            else
                            {
                                existingRequest.EnrollmentDate = null;
                            }

                        }

                        db.SaveChanges();

                        return Json(new
                        {
                            Result = "success"
                            ,
                            Message = existingReferral.ReferralID

                        });
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the referral. Please try again or contact your administrator." });
        }


        // POST: Manage/CreateStudent
        [HttpPost]
        public JsonResult CreateReferralSchoolData(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    int studentId = Convert.ToInt32(collection["studentId"]);
                    string assignedUsd = collection["assignChildCount"];

                    if (string.IsNullOrEmpty(assignedUsd))
                    {
                        return Json(new { Result = "error", Message = "Please select an Assign Child Count" });
                    }

                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == studentId).FirstOrDefault();

                    if (student != null)
                    {
                        student.AssignedUSD = assignedUsd.ToString();
                        student.ResponsibleBuildingID = collection["AttendanceBuildingId"];
                        student.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                        student.ReferralNotes = collection["ReferralNotes"];

                        if (!string.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            student.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }

                        db.SaveChanges();
                    }
                    return Json(new { Result = "success", Message = student.ReferralID });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the user. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult CreateReferralContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int referralId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == referralId).FirstOrDefault();
                    tblReferralRequest request = db.tblReferralRequests.Where(r => r.ReferralID == referralId).FirstOrDefault();

                    //delete old
                    List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(o => o.ReferralID == student.ReferralID).ToList();
                    if (relationships.Any())
                    {
                        foreach (tblReferralRelationship existingRelationship in relationships)
                        {
                            db.tblReferralRelationships.Remove(existingRelationship);

                        }
                        db.SaveChanges();
                    }

                    List<string> uniqueKeys = new List<string>();
                    int i = 0;
                    string num = "-1";
                    while (++i < collection.AllKeys.Length)
                    {

                        string nameColl = collection.AllKeys[i];
                        int start = nameColl.IndexOf('[');
                        int end = nameColl.IndexOf(']');
                        string val = nameColl.Substring(start + 1, (end - 1) - start);

                        if (num != val)
                        {
                            uniqueKeys.Add(val);
                        }

                        num = val;

                        i++;
                    }


                    foreach (string keyVal in uniqueKeys)
                    {
                        string labelName = string.Format("contact[{0}].", keyVal);
                        tblReferralRelationship contact = new tblReferralRelationship
                        {
                            ReferralID = referralId,
                            FirstName = collection[string.Format("{0}ContactFirstName", labelName)].ToString(),
                            LastName = collection[string.Format("{0}ContactLastName", labelName)].ToString(),
                            Realtionship = collection[string.Format("{0}ContactRelationship", labelName)].ToString(),
                            Address1 = collection[string.Format("{0}ContactStreetAddress1", labelName)].ToString(),
                            Address2 = collection[string.Format("{0}ContactStreetAddress2", labelName)].ToString(),
                            City = collection[string.Format("{0}ContactCity", labelName)].ToString(),
                            State = collection[string.Format("{0}ContactState", labelName)].ToString(),
                            Zip = collection[string.Format("{0}ContactZipCode", labelName)].ToString(),
                            Phone = collection[string.Format("{0}ContactPhoneNumber", labelName)].ToString(),
                            Email = collection[string.Format("{0}ContactEmail", labelName)].ToString()
                        };

                        if (collection[string.Format("{0}PrimaryContact", labelName)] != null)
                        {
                            contact.PrimaryContact = 1;
                        }

                        db.tblReferralRelationships.Add(contact);
                    }

                    db.SaveChanges();

                    //create summary
                    string summaryText = CreateSummary(student, request);

                    return Json(new { Result = "success", Message = student.ReferralID, Summary = summaryText });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the referral's contacts. Please try again or contact your administrator." });
        }

        private string CreateSummary(tblReferralInfo student, tblReferralRequest referralRequest)
        {
            //create summary
            string neighborhoodBuilding = "";
            if (student.NeighborhoodBuildingID != null)
            {
                tblBuilding nb = db.tblBuildings.Where(o => o.BuildingID == student.NeighborhoodBuildingID).FirstOrDefault();
                neighborhoodBuilding = nb.BuildingName;
            }

            string responsibleBuilding = "";
            if (student.ResponsibleBuildingID != null)
            {
                tblBuilding nb = db.tblBuildings.Where(o => o.BuildingID == student.ResponsibleBuildingID).FirstOrDefault();
                responsibleBuilding = nb.BuildingName;
            }

            string grade = "";
            if (student.Grade != null)
            {
                tblGrade nb = db.tblGrades.Where(o => o.gradeID == student.Grade).FirstOrDefault();
                grade = nb.description;
            }

            string county = "";
            if (student.County != null)
            {
                tblCounty cty = db.tblCounties.Where(o => o.CountyCode == student.County).FirstOrDefault();
                county = cty.CountyName;
            }



            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<h3>Student Information</h3>");

            sb.AppendFormat("<b>Referral Type:</b> {0}", referralRequest.ReferralType);
            sb.Append("<br/>");
            if (referralRequest.ReferralType == "Incoming")
            {
                sb.AppendFormat("<b>District Enrollment Date:</b> {0}", referralRequest.EnrollmentDate.HasValue ? referralRequest.EnrollmentDate.Value.ToShortDateString() : "");
                sb.Append("<br/>");
            }

            sb.AppendFormat("<b>KIDSID:</b> {0} ", student.KIDSID.HasValue && student.KIDSID.Value != 0 ? student.KIDSID.ToString() : student.KIDSID.Value == 0 ? "0000000000" : "");
            sb.Append("<br/>");
            sb.AppendFormat("<b>Student Name:</b> {0} {1} {2}", student.FirstName, student.MiddleInitial, student.LastName);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Birthdate:</b> {0}", student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToShortDateString() : "");
            sb.Append("<br/>");
            sb.AppendFormat("<b>Gender:</b> {0}", student.Gender);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Address:</b> {0} {1} {2}, {3} {4}", student.Address1, student.Address2, student.City, student.State, student.Zip);
            sb.Append("<br/>");

            if (referralRequest.ReferralType == "Incoming")
            {
                sb.AppendFormat("<b>Primary Disability:</b> {0}", GetDisability(student.Primary_DisabilityCode));
                sb.Append("<br/>");
                sb.AppendFormat("<b>Secondary Disability:</b> {0}", GetDisability(student.Secondary_DisabilityCode));
                sb.Append("<br/>");
            }


            sb.AppendFormat("<b>County of Residence:</b> ({0}) {1}", student.County, county);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Grade:</b> {0}", grade);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Race:</b> {0}", student.Race);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Hispanic Ethnicity:</b> {0}", student.Ethicity);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Student Language:</b> {0}", GetLanguage(student.StudentLanguage));
            sb.Append("<br/>");
            sb.AppendFormat("<b>Parents Language:</b> {0}", GetLanguage(student.ParentLanguage));
            sb.Append("<br/>");
            sb.AppendFormat("<b>Assign Child Count:</b> {0}", student.AssignedUSD);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Neighborhood School:</b> {0}", neighborhoodBuilding);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Responsible School:</b> {0}", responsibleBuilding);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Initial Evaluation Consent Received:</b> {0}", student.InitialEvalConsentSigned.HasValue ? student.InitialEvalConsentSigned.Value.ToShortDateString() : "");

            sb.Append("<br/>");
            sb.Append("<br/>");
            sb.AppendFormat("<h3>Parent/Guardian Information</h3>");

            foreach (tblReferralRelationship pc in db.tblReferralRelationships.Where(o => o.ReferralID == student.ReferralID))
            {
                sb.AppendFormat("<b>Relationship:</b><span style='text-transform: capitalize;'> {0}</span>", pc.Realtionship);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Parent/Guardian:</b> {0} {1}", pc.FirstName, pc.LastName);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Address:</b> {0} {1} {2}, {3} {4}", pc.Address1, pc.Address2, pc.City, pc.State, pc.Zip);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Email:</b> {0}", pc.Email);
                sb.Append("<br/>");
                sb.Append("<br/>");
            }

            sb.Append("<br/>");
            sb.AppendFormat("<h3>Additional Information</h3>");
            sb.AppendFormat("{0}", student.ReferralNotes);

            return sb.ToString();
        }

        [HttpPost]
        public JsonResult SubmitReferral(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == studentId).FirstOrDefault();
                    tblReferralRequest existingReferalReq = db.tblReferralRequests.Where(r => r.ReferralID == studentId).FirstOrDefault();
                    List<tblUser> list = new List<tblUser>();

                    string misRole = "2"; //level 4
                    list = (from org in db.tblOrganizationMappings
                            join user in db.tblUsers
                                on org.UserID equals user.UserID
                            where !(user.Archive ?? false) && (user.RoleID == misRole) && org.USD == student.AssignedUSD
                            select user).Distinct().ToList();


                    if (list != null && list.Any())
                    {

                        SmtpClient smtpClient = new SmtpClient();
                        MailMessage mailMessage = new MailMessage();
                        mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress("GreenbushIEP@greenbush.org"));

                        bool hasValidEmail = false;

                        foreach (tblUser misUser in list)
                        {
                            if (!string.IsNullOrEmpty(misUser.Email) && IsValidEmail(misUser.Email))
                            {
                                mailMessage.To.Add(misUser.Email);
                                hasValidEmail = true;
                            }
                        }

                        //create summary
                        string summaryText = CreateSummary(student, existingReferalReq);
                        string subject = string.Format("Referral Request {0}", existingReferalReq.ReferralType == "Incoming" ? "(Incoming)" : "");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("The following new Referral Request has been created. Please log into the IEP Backpack to review the details.<br/><br/>");
                        sb.AppendFormat("<b>Submitted by:</b> {0}, {1}<br/><br/>", submitter.FirstName, submitter.LastName);
                        sb.Append(summaryText);
                        sb.Append("<br/><br/>Contact melanie.johnson@greenbush.org or (620) 724-6281 if you need any assistance.<br/>URL: https://greenbushbackpack.org ");
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Subject = subject;
                        mailMessage.Body = sb.ToString();

                        if (hasValidEmail)
                            smtpClient.Send(mailMessage);

                    }

                    tblOrganizationMapping submitterDistrict = (from org in db.tblOrganizationMappings
                                                                join user in db.tblUsers
                                                                    on org.UserID equals user.UserID
                                                                where user.UserID == submitter.UserID
                                                                select org).Distinct().FirstOrDefault();


                    //tblReferralRequest existingReferalReq = db.tblReferralRequests.Where(o => o.ReferralID == student.ReferralID).FirstOrDefault();

                    if (existingReferalReq == null)
                    {
                        tblReferralRequest request = new tblReferralRequest
                        {
                            UserID_Requster = submitter.UserID,
                            UserID_District = submitterDistrict.USD,
                            ReferralID = student.ReferralID,
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now,
                            Submit_Date = DateTime.Now
                        };
                        db.tblReferralRequests.Add(request);
                    }
                    else
                    {
                        existingReferalReq.Update_Date = DateTime.Now;
                        existingReferalReq.UserID_Requster = submitter.UserID;
                        existingReferalReq.UserID_District = submitterDistrict.USD;
                        existingReferalReq.Submit_Date = DateTime.Now;
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = student.ReferralID });
                }

                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
                catch (Exception ex)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to send the referral email. Please try again or contact your administrator. " + ex.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to send the referral email. Please try again or contact your administrator." });
        }

        [HttpGet]
        public ActionResult CreateLearner()
        {
            LearnerDetailsViewModel model = new LearnerDetailsViewModel
            {
                submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name)
            };

            model.locations = (from l in db.vw_ILP_Locations join ul in db.tbl_ILP_UserLocations on l.LocationID equals ul.LocationID where model.submitter.UserID == ul.UserID select l).Distinct().ToList();
            model.allLocations = db.vw_ILP_Locations.ToList();
            model.student.DateOfBirth = DateTime.Now.AddYears(-5);
            model.programs = db.tbl_ILP_Programs.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.RoleName = ConvertToILPRoleName(model.submitter.UserID);
            ViewBag.CanAssignTeacher = model.submitter.RoleID == mis || model.submitter.RoleID == owner ? true : false;

            return View("~/Views/ILP/CreateLearner.cshtml", model);
        }

        [HttpPost]
        public JsonResult CreateLearner(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // check that the kidsIS doesn't already exsist in the system.
                    long kidsID = Convert.ToInt64(collection["kidsid"]);
                    var studentId = Convert.ToInt64(collection["studentId"]);

                    tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID && kidsID != 000000000).FirstOrDefault();
                    if (exsistingStudent != null && studentId == 0)
                    {
                        return Json(new { Result = "error", Message = "The student is already in the Greenbush system. Please contact Greenbush." });
                    }

                    //check if we already created this student

                    tblUser student = new tblUser();

                    if (studentId != 0)
                    {
                        student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                        student.FirstName = collection["firstname"];
                        student.MiddleName = collection["middlename"];
                        student.LastName = collection["lastname"];
                        student.Email = string.IsNullOrEmpty(collection["email"]) ? null : collection["email"].ToString();
                    }
                    else
                    {
                        // Create New User 
                        student = new tblUser()
                        {
                            RoleID = "10",
                            FirstName = collection["firstname"],
                            MiddleName = collection["middlename"],
                            LastName = collection["lastname"],
                            Email = ((!string.IsNullOrEmpty(collection["email"])) ? collection["email"].ToString() : null),
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now,
                        };
                    }

                    // try catch. If the email is the same as another student show error gracefully.
                    try
                    {
                        if (!string.IsNullOrEmpty(student.Email) && db.tblUsers.Any(o => o.Email == student.Email))
                        {
                            return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                        }
                        else if (collection["misDistrict"] == null)
                        {
                            return Json(new { Result = "error", Message = "Please choose an attending location." });
                        }
                        else
                        {
                            if (studentId == 0)
                            {
                                db.tblUsers.Add(student);
                            }
                            db.tblAuditLogs.Add(new tblAuditLog() { Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", ModifiedBy = submitter.UserID, UserID = student.UserID, Value = "Created User " + submitter.FirstName + " " + submitter.LastName });
                            db.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    tblStudentInfo studentInfo = new tblStudentInfo();
                    if (studentId != 0)
                    {
                        // remove all the buildingId. Blow it all away.
                        db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == studentId));
                        db.SaveChanges();

                        //updating existing
                        studentInfo = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
                    }

                    studentInfo.UserID = student.UserID;
                    studentInfo.KIDSID = kidsID;
                    studentInfo.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                    studentInfo.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                    studentInfo.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                    studentInfo.USD = collection["misDistrict"];
                    studentInfo.BuildingID = "0";
                    studentInfo.Status = "PENDING";
                    studentInfo.Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                    studentInfo.CreatedBy = submitter.UserID;
                    studentInfo.Create_Date = DateTime.Now;
                    studentInfo.Update_Date = DateTime.Now;
                    studentInfo.PlacementCode = null;
                    studentInfo.ClaimingCode = null;

                    // map the buildings in the building mapping table
                    try
                    {
                        db.tblBuildingMappings.Add(new tblBuildingMapping() { BuildingID = "0", USD = studentInfo.USD, UserID = studentInfo.UserID, Create_Date = DateTime.Now });
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    // map the user to a "book"
                    try
                    {
                        if (studentId == 0)
                        {
                            db.tblStudentInfoes.Add(studentInfo);

                            tblUserRole roles = new tblUserRole()
                            {
                                UserID = student.UserID,
                                RoleID = Convert.ToInt32(student.RoleID),
                                BookID = "_ILP_"
                            };
                            db.tblUserRoles.Add(roles);
                        }
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    // map the user to userPrograms
                    try
                    {

                        string programValues = collection["studentPlacement"];

                        if (!string.IsNullOrEmpty(programValues))
                        {
                            string[] programArray = programValues.Split(',');

                            if (studentId != 0)
                            {
                                //updating existing
                                List<tbl_ILP_UserPrograms> fullList = db.tbl_ILP_UserPrograms.Where(o => o.UserID == studentId).ToList();
                                db.tbl_ILP_UserPrograms.RemoveRange(fullList);
                                db.SaveChanges();
                            }

                            foreach (string program in programArray)
                            {
                                tbl_ILP_UserPrograms prog = new tbl_ILP_UserPrograms
                                {
                                    ProgramCode = program,
                                    UserID = student.UserID,
                                    LocationID = null
                                };

                                db.tbl_ILP_UserPrograms.Add(prog);
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while adding the user to the UserPrograms table. \n\n" + e.InnerException.ToString() });
                    }

                    // save to organization chart
                    // save the user to all the districts that was selected.
                    // tblOrganizationMapping and tblBuildingMapping
                    string districtValues = collection["misDistrict"];

                    if (!string.IsNullOrEmpty(districtValues))
                    {
                        string[] districtArray = districtValues.Split(',');

                        if (studentId != 0)
                        {
                            //updating existing
                            List<tblOrganizationMapping> fullList = db.tblOrganizationMappings.Where(o => o.UserID == studentId).ToList();
                            db.tblOrganizationMappings.RemoveRange(fullList);
                            db.SaveChanges();
                        }

                        foreach (string usd in districtArray)
                        {
                            tblOrganizationMapping org = new tblOrganizationMapping
                            {
                                AdminID = submitter.UserID,
                                UserID = student.UserID,
                                Create_Date = DateTime.Now,
                                USD = usd
                            };

                            db.tblOrganizationMappings.Add(org);
                            db.SaveChanges();
                        }
                    }

                    return Json(new { Result = "success", Message = student.UserID });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the user. Please try again or contact your administrator." });
        }


        // GET: Manage/CreateStudent
        [HttpGet]
        public ActionResult CreateStudent()
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel
            {
                submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name)
            };
            model.districts = model.submitter.RoleID == "1" ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.allDistricts = db.tblDistricts.ToList();
            model.student.DateOfBirth = DateTime.Now.AddYears(-5);
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.contacts.Add(new tblStudentRelationship() { Realtionship = "parent", State = "KS" });
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == "101"
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            ViewBag.CanAssignTeacher = model.submitter.RoleID == mis || model.submitter.RoleID == owner ? true : false;

            return View("~/Views/Home/CreateStudent.cshtml", model);
        }

        // POST: Manage/CreateStudent
        [HttpPost]
        public JsonResult CreateStudent(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // check that the kidsIS doesn't already exsist in the system.
                    long kidsID = Convert.ToInt64(collection["kidsid"]);
                    var studentId = Convert.ToInt64(collection["studentId"]);

                    tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID && kidsID != 000000000).FirstOrDefault();
                    if (exsistingStudent != null && studentId == 0)
                    {
                        return Json(new { Result = "error", Message = "The student is already in the Greenbush system. Please contact Greenbush." });
                    }

                    //check if we already created this student

                    tblUser student = new tblUser();

                    if (studentId != 0)
                    {
                        student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                        student.FirstName = collection["firstname"];
                        student.MiddleName = collection["middlename"];
                        student.LastName = collection["lastname"];
                        student.Email = string.IsNullOrEmpty(collection["email"]) ? null : collection["email"].ToString();
                    }
                    else
                    {
                        // Create New User 
                        student = new tblUser()
                        {
                            RoleID = "5",
                            FirstName = collection["firstname"],
                            MiddleName = collection["middlename"],
                            LastName = collection["lastname"],
                            Email = ((!string.IsNullOrEmpty(collection["email"])) ? collection["email"].ToString() : null),
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now,
                        };
                    }

                    // try catch. If the email is the same as another student show error gracefully.
                    try
                    {
                        if (!string.IsNullOrEmpty(student.Email) && db.tblUsers.Any(o => o.Email == student.Email))
                        {
                            return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                        }
                        else if (collection["misDistrict"] == null)
                        {
                            return Json(new { Result = "error", Message = "Please choose an attending district." });
                        }
                        else
                        {
                            if (studentId == 0)
                            {
                                db.tblUsers.Add(student);
                            }
                            db.tblAuditLogs.Add(new tblAuditLog() { Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", ModifiedBy = submitter.UserID, UserID = student.UserID, Value = "Created User " + submitter.FirstName + " " + submitter.LastName });
                            db.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    tblStudentInfo studentInfo = new tblStudentInfo();
                    if (studentId != 0)
                    {
                        // remove all the buildingId. Blow it all away.
                        db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == studentId));
                        db.SaveChanges();

                        //updating existing
                        studentInfo = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
                    }

                    studentInfo.UserID = student.UserID;
                    studentInfo.KIDSID = kidsID;
                    studentInfo.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                    studentInfo.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                    studentInfo.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                    studentInfo.AssignedUSD = collection["assignChildCount"].ToString();
                    studentInfo.USD = collection["misDistrict"];
                    studentInfo.BuildingID = collection["AttendanceBuildingId"];
                    studentInfo.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                    studentInfo.Status = "PENDING";
                    studentInfo.Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                    studentInfo.CreatedBy = submitter.UserID;
                    studentInfo.Create_Date = DateTime.Now;
                    studentInfo.Update_Date = DateTime.Now;
                    studentInfo.PlacementCode = collection["studentPlacement"];
                    studentInfo.ClaimingCode = true; // set to default true unless they change it on the second page.
                    studentInfo.isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on";


                    // map the buildings in the building mapping table
                    try
                    {
                        db.tblBuildingMappings.Add(new tblBuildingMapping() { BuildingID = studentInfo.BuildingID, USD = studentInfo.AssignedUSD, UserID = studentInfo.UserID, Create_Date = DateTime.Now });
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    try
                    {
                        if (studentId == 0)
                        {
                            db.tblStudentInfoes.Add(studentInfo);

                            tblUserRole roles = new tblUserRole()
                            {
                                UserID = student.UserID,
                                RoleID = Convert.ToInt32(student.RoleID),
                                BookID = "_IEP_"
                            };
                            db.tblUserRoles.Add(roles);
                        }
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    // save to organization chart
                    // save the user to all the districts that was selected.
                    // tblOrganizationMapping and tblBuildingMapping
                    string districtValues = collection["misDistrict"];

                    if (!string.IsNullOrEmpty(districtValues))
                    {
                        string[] districtArray = districtValues.Split(',');

                        if (studentId != 0)
                        {
                            //updating existing
                            List<tblOrganizationMapping> fullList = db.tblOrganizationMappings.Where(o => o.UserID == studentId).ToList();
                            db.tblOrganizationMappings.RemoveRange(fullList);
                            db.SaveChanges();
                        }

                        foreach (string usd in districtArray)
                        {
                            tblOrganizationMapping org = new tblOrganizationMapping
                            {
                                AdminID = submitter.UserID,
                                UserID = student.UserID,
                                Create_Date = DateTime.Now,
                                USD = usd
                            };

                            db.tblOrganizationMappings.Add(org);
                            db.SaveChanges();
                        }
                    }

                    return Json(new { Result = "success", Message = student.UserID });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the user. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult CreateStudentOptions(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser user = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                    if (user != null)
                    {
                        user.Address1 = collection["studentStreetAddress1"].ToString();
                        user.Address2 = collection["studentStreetAddress2"].ToString();
                        user.City = collection["studentCity"].ToString();
                        user.State = collection["studentState"].ToString();
                        user.Zip = collection["studentZipCode"].ToString();
                    }
                    db.SaveChanges();

                    tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    if (info != null)
                    {
                        info.County = collection["studentCounty"].ToString();
                        info.Grade = Convert.ToInt32(collection["studentGrade"]);
                        info.RaceCode = collection["studentRace"].ToString();
                        info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
                        info.Ethicity = collection["studentEthnic"].ToString();
                        info.StudentLanguage = collection["studentLanguage"].ToString();
                        info.ParentLanguage = collection["parentLanguage"].ToString();
                        info.ClaimingCode = collection["claimingCode"] == "on" ? true : false;
                        info.FullDayKG = collection["fullDayKindergarten"] == "on";
                        info.StatusCode = collection["statusCode"].ToString();

                        if (!string.IsNullOrEmpty(collection["initialIEPDate"]))
                        {
                            info.InitialIEPDate = Convert.ToDateTime(collection["initialIEPDate"]);
                        }

                        if (!string.IsNullOrEmpty(collection["exitDate"]))
                        {
                            info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
                        }

                        if (!string.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }

                        if (!string.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                        {
                            info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                        }

                        if (!string.IsNullOrEmpty(collection["reEvaluationSignature"]))
                        {
                            info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                        }

                        if (!string.IsNullOrEmpty(collection["reEvalCompleted"]))
                        {
                            info.ReEvalCompleted = Convert.ToDateTime(collection["reEvalCompleted"]);
                        }
                    }
                    db.SaveChanges();

                    if (info != null && info.ReEvalCompleted.HasValue)
                    {
                        CreateReevalCompletedArchive(studentId, info.ReEvalCompleted.Value);
                    }

                    if (info != null && info.ReEvalConsentSigned.HasValue)
                    {
                        CreateReevalSignedArchive(studentId, info.ReEvalConsentSigned.Value);
                    }

                    return Json(new { Result = "success", Message = studentId });
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to add the student's contacts. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to update the user's options. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult CreateStudentContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

                    // blow away any previous contacts because this is a bug and prevents duplicate parents.
                    db.tblStudentRelationships.RemoveRange(db.tblStudentRelationships.Where(r => r.UserID == studentId));
                    db.SaveChanges();

                    int j = 0;
                    int loopCounter = 1;
                    while (++j < collection.Count - 1)
                    {
                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[j].ToString(),
                            LastName = collection[++j].ToString(),
                            Realtionship = collection[++j].ToString(),
                            Address1 = collection[++j].ToString(),
                            Address2 = collection[++j].ToString(),
                            City = collection[++j].ToString(),
                            State = collection[++j].ToString(),
                            Zip = collection[++j].ToString(),
                            Phone = collection[++j].ToString(),
                            Email = collection[++j].ToString(),
                            Create_Date = DateTime.Now,
                            CreatedBy = submitter.UserID,
                            ModifiedBy = submitter.UserID,
                            Update_Date = DateTime.Now,
                        };

                        /////////////////////////////
                        // This whole if block is due to the fact that checkbox false values are NOT passed to our collection
                        // and the checkbox is the last value in the collection fields.
                        /////////////////////////////
                        if (++j <= collection.Count - 1) // test if this is the end of the collection i.e. out of range issues.
                        {
                            if (collection.GetKey(j) == string.Format("contacts[{0}].PrimaryContact", loopCounter))
                            {
                                contact.PrimaryContact = collection[j] == "on" ? 1 : 0;
                            }
                            else { j--; }
                        }

                        try
                        {
                            db.tblStudentRelationships.Add(contact);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to add the student's contacts. \n\n" + e.InnerException.ToString() });
                        }

                        loopCounter++;
                    }

                    //get teachers list
                    tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    List<TeacherView> teachers = new List<TeacherView>();

                    if (info != null)
                    {
                        teachers = GetTeacherByBuilding(info.BuildingID, info.AssignedUSD);
                    }

                    return Json(new { Result = "success", Message = student.UserID, teacherList = teachers });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    IEnumerable<string> errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    string fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    string exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the student's contacts. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult CreateStudentAssignments(int studentId, int[] teachers)
        {
            try
            {
                StudentAssignments(studentId, teachers);

                return Json(new { Result = "success", Message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        private void StudentAssignments(int studentId, int[] teachers)
        {
            tblUser studentUser = db.tblUsers.Where(u => u.UserID == studentId).SingleOrDefault();
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            List<tblOrganizationMapping> existingAssignments = db.tblOrganizationMappings.Where(u => u.UserID == studentId).ToList();

            if (existingAssignments.Any())
            {
                //delete assignments 
                foreach (tblOrganizationMapping existing in existingAssignments)
                {
                    db.tblOrganizationMappings.Remove(existing);
                    db.SaveChanges();
                }
            }

            //add all assignments the user is submitting
            foreach (int teacher in teachers)
            {
                var existingAssignment = db.tblOrganizationMappings.Any(u => u.UserID == studentId && u.AdminID == teacher);

                if (!existingAssignment)
                {
                    tblOrganizationMapping newRelation = new tblOrganizationMapping()
                    {
                        AdminID = teacher,
                        UserID = studentUser.UserID,
                        Create_Date = DateTime.Now,
                        USD = (from bm in db.tblBuildingMappings where bm.UserID == studentUser.UserID select bm.USD).FirstOrDefault(),
                    };
                    db.tblOrganizationMappings.Add(newRelation);
                    db.SaveChanges();
                }
            }
        }

        [HttpPost]
        public ActionResult CreateStudentAvatar(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["studentId"]);

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                //// UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(adminpersona.FileName);
                    string random = Guid.NewGuid() + fileName;
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                    }

                    student.ImageURL = random;
                    adminpersona.SaveAs(path);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Portal", "Home");
        }


        // GET: Manage/EditStudent/5
        [HttpGet]
        public ActionResult EditStudent(int id, bool backToStudentIEP = false)
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel
            {
                student = new Student()
            };
            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (student != null)
            {
                model.student.UserID = id;
                model.student.FirstName = student.FirstName;
                model.student.MiddleName = student.MiddleName;
                model.student.LastName = student.LastName;
                model.student.City = student.City;
                model.student.State = student.State;
                model.student.Email = student.Email;
                model.student.RoleID = "5";
                model.student.ImageURL = student.ImageURL;
                model.student.Address1 = student.Address1;
                model.student.Address2 = student.Address2;
                model.student.Zip = student.Zip;
            }

            tblStudentInfo studentinfo = db.tblStudentInfoes.Where(i => i.UserID == id).FirstOrDefault();
            if (studentinfo != null)
            {
                model.student.KidsID = studentinfo.KIDSID;
                model.student.DateOfBirth = studentinfo.DateOfBirth;
                model.student.USD = studentinfo.USD;
                model.student.BuildingID = studentinfo.BuildingID;
                model.student.NeighborhoodBuildingID = studentinfo.NeighborhoodBuildingID;
                model.info = studentinfo;
            }

            List<tblStudentRelationship> relationships = db.tblStudentRelationships.Where(r => r.UserID == student.UserID).ToList();
            if (relationships != null)
            {
                foreach (tblStudentRelationship relationship in relationships)
                {
                    model.contacts.Add(new tblStudentRelationship()
                    {
                        FirstName = relationship.FirstName,
                        MiddleName = relationship.MiddleName,
                        LastName = relationship.LastName,
                        Address1 = relationship.Address1,
                        Address2 = relationship.Address2,
                        City = relationship.City,
                        State = relationship.State,
                        Zip = relationship.Zip,
                        Email = relationship.Email,
                        Phone = relationship.Phone,
                        Realtionship = relationship.Realtionship,
                        UserID = relationship.UserID,
                        RealtionshipID = relationship.RealtionshipID,
                        PrimaryContact = relationship.PrimaryContact
                    });
                }
            }

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.races = db.tblRaces.ToList();
            model.selectedRace = db.tblRaces.Where(r => r.RaceCode == studentinfo.RaceCode).FirstOrDefault();
            model.allDistricts = db.tblDistricts.ToList();
            model.districts = model.submitter.RoleID == "1" ? model.allDistricts.Where(d => d.Active == 1).ToList() : (from d in model.allDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            //model.selectedDistrict = (from d in db.tblDistricts join o in db.tblOrganizationMappings on d.USD equals o.USD where model.student.UserID == o.UserID select d).Distinct().ToList();

            if (model.student.USD != null)
            {
                var attendingDistricts = model.student.USD.Split(',').ToList();
                model.selectedDistrict = db.tblDistricts.Where(o => attendingDistricts.Contains(o.USD)).ToList();
            }



            string districtList = string.Join(", ", model.districts.Select(o => o.USD).Distinct());

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == studentinfo.AssignedUSD
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();
            ViewBag.CanAssignTeacher = model.submitter.RoleID == mis || model.submitter.RoleID == owner ? true : false;
            ViewBag.backToStudentIEP = backToStudentIEP;

            return View("~/Views/Home/EditStudent.cshtml", model);
        }

        // POST: Manage/EditStudent
        [HttpPost]
        public JsonResult EditStudent(FormCollection collection)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            int studentId = Convert.ToInt32(collection["id"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                student.FirstName = collection["firstname"];
                student.MiddleName = collection["middlename"];
                student.LastName = collection["lastname"];
                student.Email = string.IsNullOrEmpty(collection["email"]) ? null : collection["email"].ToString();
                student.RoleID = "5";
            }
            else
            {
                return Json(new { Result = "error", Message = "There was an error while trying to edit the user." });
            }

            tblStudentInfo info = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
            if (info != null)
            {
                // remove all the buildingId. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == studentId));

                db.SaveChanges();

                // save to organization chart
                // save the user to all the districts that was selected.
                // tblOrganizationMapping and tblBuildingMapping
                string districtValues = collection["misDistrict"];

                if (!string.IsNullOrEmpty(districtValues))
                {
                    string[] districtArray = districtValues.Split(',');

                    List<tblOrganizationMapping> fullList = db.tblOrganizationMappings.Where(o => o.UserID == studentId).ToList();
                    List<tblOrganizationMapping> removeList = fullList.Where(o => !districtArray.Contains(o.USD)).ToList();
                    db.tblOrganizationMappings.RemoveRange(removeList);
                    db.SaveChanges();

                    try
                    {
                        foreach (string usd in districtArray)
                        {
                            if (fullList.Any(l => !l.USD.Contains(usd)))
                            {
                                int mappingCount = db.tblOrganizationMappings.Where(o => o.AdminID == submitter.UserID && o.UserID == student.UserID && o.USD == usd).Count();

                                if (mappingCount == 0)
                                {
                                    db.tblOrganizationMappings.Add(new tblOrganizationMapping()
                                    {
                                        AdminID = submitter.UserID,
                                        UserID = student.UserID,
                                        USD = usd,
                                        Create_Date = DateTime.Now,
                                        CreatedBy = submitter.UserID,
                                    });

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to add the student to the Attending USD. \n\n" + e.InnerException.ToString() });
                    }
                }

                // map the buildings in the building mapping table
                try
                {
                    db.tblBuildingMappings.Add(new tblBuildingMapping()
                    {
                        BuildingID = collection["AttendanceBuildingId"],
                        USD = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).Select(b => b.USD).FirstOrDefault(), //info.USD,
                        UserID = info.UserID,
                        CreatedBy = submitter.UserID,
                        Create_Date = DateTime.Now,
                    });

                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                }

                info.UserID = student.UserID;
                info.KIDSID = Convert.ToInt64(collection["kidsid"]);
                info.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                info.AssignedUSD = collection["assignChildCount"];
                info.BuildingID = collection["AttendanceBuildingId"];
                info.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                info.Status = "PENDING";
                info.Gender = (string.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                info.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                info.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                info.PlacementCode = collection["studentPlacement"];
                info.USD = collection["misDistrict"];
                info.isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on" ? true : false;
            }
            else
            {
                return Json(new { Result = "error", Message = "There was an error while trying to edit the user." });
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
            }

            return Json(new { Result = "success", Message = student.UserID });
        }

        // POST: Manage/EditStudentContacts
        [HttpPost]
        public JsonResult EditStudentOptions(FormCollection collection)
        {

            int studentId = Convert.ToInt32(collection["studentId"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

            if (student != null)
            {
                try
                {
                    student.Address1 = collection["studentStreetAddress1"].ToString();
                    student.Address2 = collection["studentStreetAddress2"].ToString();
                    student.City = collection["studentCity"].ToString();
                    student.State = collection["studentState"].ToString();
                    student.Zip = collection["studentZipCode"].ToString();
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's options. \n\n" + e.InnerException.ToString() });
                }
            }

            tblStudentInfo info = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
            if (info != null)
            {
                try
                {
                    string currentStatusCode = info.StatusCode;

                    info.County = collection["studentCounty"].ToString();
                    info.Grade = Convert.ToInt32(collection["studentGrade"]);
                    info.RaceCode = collection["studentRace"].ToString();
                    info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
                    info.Ethicity = collection["studentEthnic"].ToString();
                    info.StudentLanguage = collection["studentLanguage"].ToString();
                    info.ParentLanguage = collection["parentLanguage"].ToString();
                    info.ClaimingCode = collection["claimingCode"] == "on" ? true : false;
                    info.FullDayKG = collection["fullDayKindergarten"] == "on" ? true : false;
                    info.StatusCode = collection["statusCode"].ToString();
                    info.ExitNotes = !string.IsNullOrEmpty(collection["exitNotes"]) ? collection["exitNotes"].ToString() : "";

                    if (!string.IsNullOrEmpty(collection["initialIEPDate"]))
                    {
                        info.InitialIEPDate = Convert.ToDateTime(collection["initialIEPDate"]);
                    }
                    else
                    {
                        info.InitialIEPDate = null;
                    }

                    if (!string.IsNullOrEmpty(collection["exitDate"]))
                    {

                        DateTime exitDate;

                        if (DateTime.TryParseExact(collection["exitDate"], "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out exitDate))
                        {
                            info.ExitDate = exitDate;
                        }
                        else
                        {
                            return Json(new { Result = "error", Message = "The Exit Date is invalid. Please use the format: MM/DD/YYYY." });
                        }
                    }
                    else
                    {
                        info.ExitDate = null;
                    }

                    if (!string.IsNullOrEmpty(collection["initialConsentSignature"]))
                    {
                        info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                    }
                    else
                    {
                        info.InitialEvalConsentSigned = null;
                    }

                    if (!string.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                    {
                        info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                    }
                    else
                    {
                        info.InitialEvalDetermination = null;
                    }

                    if (!string.IsNullOrEmpty(collection["reEvaluationSignature"]))
                    {
                        info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                    }
                    else
                    {
                        info.ReEvalConsentSigned = null;
                    }

                    if (!string.IsNullOrEmpty(collection["reEvalCompleted"]))
                    {
                        info.ReEvalCompleted = Convert.ToDateTime(collection["reEvalCompleted"]);
                    }
                    else
                    {
                        info.ReEvalCompleted = null;
                    }

                    //check if exit
                    tblStatusCode statusCodeObj = db.tblStatusCodes.Where(o => o.StatusCode == info.StatusCode).FirstOrDefault();
                    bool isExit = statusCodeObj != null && statusCodeObj.Type.ToLower() == "inactive";
                    if (isExit && !info.ExitDate.HasValue)
                    {
                        return Json(new { Result = "error", Message = "Please enter an Exit date." });
                    }

                    db.SaveChanges();

                    if (info != null && info.ReEvalCompleted.HasValue)
                    {
                        CreateReevalCompletedArchive(studentId, info.ReEvalCompleted.Value);
                    }

                    if (info != null && info.ReEvalConsentSigned.HasValue)
                    {
                        CreateReevalSignedArchive(studentId, info.ReEvalConsentSigned.Value);
                    }

                    //check for exit code and send email if it was just changed to an exist code
                    if (currentStatusCode != info.StatusCode)
                    {
                        if (isExit)
                        {
                            // keep an audit trail on the students that come and go due to real life circumstances 

                            if (info.ExitDate.HasValue)
                            {
                                var archive = new tblArchiveIEPExit
                                {
                                    userID = studentId,
                                    exitDate = info.ExitDate.Value,
                                    exitNotes = info.ExitNotes,
                                    BuildingID = info.BuildingID,
                                    StatusCode = info.StatusCode,
                                    USD = info.USD,
                                    CreatedBy = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name).UserID,
                                    Create_Date = DateTime.Now,
                                    Update_Date = DateTime.Now
                                };

                                db.tblArchiveIEPExits.Add(archive);
                                db.SaveChanges();

                                SendExitEmail(info.AssignedUSD
                                    , string.Format("{0}, {1}", student.LastName, student.FirstName)
                                    , info.ExitDate.HasValue ? info.ExitDate.Value.ToShortDateString() : ""
                                    , string.Format("({0}) {1}", info.StatusCode, statusCodeObj.Description)
                                    , info.ExitNotes
                                    );
                            }
                        }
                    }
                }
                catch (SmtpException stmpError)
                {
                    return Json(new { Result = "error", Message = "There was an error sending the MIS Exit notification email.\n\n" + stmpError.Message });
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's options. \n\n" + e.Message });
                }


                return Json(new { Result = "success", Message = student.UserID });
            }

            return Json(new { Result = "error", Message = "There was an error while trying to edit students. \n\n" });
        }

        // POST: Manage/EditStudentContancts
        [HttpPost]
        public JsonResult EditStudentContacts(FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["id"]);
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

            if (student != null)
            {
                try
                {
                    // BLOW AWAY all the relationships to the students.
                    List<tblStudentRelationship> relationships = db.tblStudentRelationships.Where(r => r.UserID == student.UserID).ToList();
                    if (relationships != null && relationships.Count > 0)
                    {
                        db.tblStudentRelationships.RemoveRange(relationships);
                        db.SaveChanges();
                    }

                    int j = 0;
                    while (++j < collection.Count - 1)
                    {
                        int startIndex = collection.GetKey(j).ToString().IndexOf("[") + 1;
                        int endIndex = collection.GetKey(j).ToString().IndexOf("]");
                        string relId = collection.GetKey(j).ToString().Substring(startIndex, endIndex - startIndex);

                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[j].ToString(),
                            LastName = collection[++j].ToString(),
                            Realtionship = collection[++j].ToString(),
                            Address1 = collection[++j].ToString(),
                            Address2 = collection[++j].ToString(),
                            City = collection[++j].ToString(),
                            State = collection[++j].ToString(),
                            Zip = collection[++j].ToString(),
                            Phone = collection[++j].ToString(),
                            Email = collection[++j].ToString(),
                            CreatedBy = submitter.UserID,
                            Create_Date = DateTime.Now,
                            Update_Date = DateTime.Now,
                            ModifiedBy = submitter.UserID,
                        };

                        /////////////////////////////
                        // This whole if block is due to the fact that checkbox false values are NOT passed to our collection
                        // and the checkbox is the last value in the collection fields.
                        /////////////////////////////
                        if (++j <= collection.Count - 1) // test if this is the end of the collection i.e. out of range issues.
                        {
                            if (collection.GetKey(j) == string.Format("contact[{0}].PrimaryContact", relId))
                            {
                                contact.PrimaryContact = collection[j] == "on" ? 1 : 0;
                            }
                            else { j--; }
                        }

                        try
                        {
                            db.tblStudentRelationships.Add(contact);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts. \n\n" + e.InnerException.ToString() });
                        }
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts. \n\n" + e.InnerException.ToString() });
                }

                //get teachers list
                tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                List<TeacherView> teachers = new List<TeacherView>();
                List<int> currentAssignments = new List<int>();
                if (info != null)
                {
                    teachers = GetTeacherByBuilding(info.BuildingID, info.AssignedUSD);

                    currentAssignments = db.tblOrganizationMappings.Where(o => o.UserID == studentId).Select(o => o.AdminID).ToList();

                }

                return Json(new { Result = "success", Message = student.UserID, teacherList = teachers, assignments = currentAssignments });


            }

            return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts." });
        }

        [HttpPost]
        public ActionResult EditStudentAvatar(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["id"]);

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                string filePath = Server.MapPath("~/Avatar/" + student.ImageURL);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    db.SaveChanges();
                }

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(adminpersona.FileName);
                    string random = Guid.NewGuid() + fileName;
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                    }
                    student.ImageURL = random;
                    adminpersona.SaveAs(path);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Portal", "Home");
        }



        // GET: Manage/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            UserDetailsViewModel model = new UserDetailsViewModel
            {
                user = db.tblUsers.Where(u => u.UserID == id).SingleOrDefault(),
                submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name)
            };
            model.selectedDistrict = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.user.UserID == bm.UserID && bm.BuildingID == "0" select d).Distinct().ToList();
            model.districts = (model.submitter.RoleID == "1") ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.buildings = (from bm in db.tblBuildingMappings
                               join b in db.tblBuildings on bm.USD equals b.USD
                               where bm.UserID == model.user.UserID && b.Active == 1 && bm.BuildingID == b.BuildingID
                               select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);

            if (model.user.UserID == model.submitter.UserID)
            {
                return View("~/Views/Home/EditMe.cshtml", model);
            }

            return View("~/Views/Home/EditUser.cshtml", model);
        }

        // GET: Manage/Edit/5
        [HttpGet]
        public ActionResult EditILPUser(int id)
        {
            tblUser user = db.tblUsers.Where(u => u.UserID == id).SingleOrDefault();

            ILPUserDetailsViewModel model = new ILPUserDetailsViewModel
            {
                user = user,
                submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name),
                locations = db.vw_ILP_Locations.ToList(),
                selectedLocations = (from l in db.vw_ILP_Locations join bm in db.tblBuildingMappings on l.LocationID equals bm.USD where user.UserID == bm.UserID && bm.BuildingID == "0" select l).Distinct().ToList()
            };

            ViewBag.RoleName = ConvertToILPRoleName(model.submitter.UserID);

            return View("~/Views/ILP/EditILPUser.cshtml", model);
        }


        // POST: Manage/Edit
        [HttpPost]
        public ActionResult Edit(int id, HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                tblUser misUser = FindSupervisor.GetUSersMIS(user);

                if (misUser == null)
                {
                    misUser = FindSupervisor.GetUSersMIS(submitter);
                }

                if (misUser == null || user.UserID == misUser.UserID)
                {  // I'm my own grandpapa 
                    misUser = db.tblUsers.Where(u => u.UserID == 1).FirstOrDefault();
                }

                // EDIT the user
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(collection["teacherID"])) { user.TeacherID = collection["teacherID"]; }
                    if (!string.IsNullOrEmpty(collection["role"])) { user.RoleID = collection["role"]; }
                    user.FirstName = collection["FirstName"];
                    user.LastName = collection["LastName"];
                    user.Email = collection["userEmail"];

                    if (submitter.RoleID == "1" || submitter.RoleID == "7")
                    {
                        user.Archive = collection["isArchived"] != null;
                    }

                    if (collection.AllKeys.Contains("password"))
                    {
                        string password = collection["password"].ToString();

                        PasswordHash hash = new PasswordHash(password);

                        user.Password = hash.Hash;
                        user.Salt = hash.Salt;

                        db.SaveChanges();
                    }
                }

                // EDIT their avatar
                if (adminpersona != null)
                {
                    if (!string.IsNullOrEmpty(user.ImageURL))
                    {
                        // Delete exiting file
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Avatar/"), user.ImageURL));
                    }

                    // Save new file
                    string filename = Guid.NewGuid() + Path.GetFileName(adminpersona.FileName);
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), filename);

                    try
                    {
                        adminpersona.SaveAs(path);
                    }
                    catch (Exception e)
                    {
                        Console.Write("Avatar file can't be save. Exception:" + e.InnerException.ToString());
                    }

                    user.ImageURL = filename;
                }

                db.SaveChanges();

                List<tblOrganizationMapping> districtMappings = new List<tblOrganizationMapping>();
                List<tblBuildingMapping> buildingMappings = new List<tblBuildingMapping>();
                List<string> districts = new List<string>();
                List<string> buildings = new List<string>();

                if (collection["misDistrict"] != null)
                {
                    districts = new List<string>(collection["misDistrict"].ToString().Split(','));
                }

                if (collection["buildingIds"] != null)
                {
                    buildings = new List<string>(collection["buildingIds"].ToString().Split(','));
                }

                // removes any buildings not in the current list of usd's.
                List<tblBuilding> userBuildings = db.tblBuildings.Where(b => buildings.Contains(b.BuildingID) && districts.Contains(b.USD) && b.BuildingID != "0").ToList();

                if (districts != null)
                {
                    foreach (string district in districts)
                    {
                        districtMappings.Add(new tblOrganizationMapping()
                        {
                            AdminID = misUser.UserID,
                            UserID = id,
                            USD = district,
                            Create_Date = DateTime.Now,
                        });

                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = "0",
                            UserID = id,
                            USD = district,
                            Create_Date = DateTime.Now,
                        });
                    }
                }

                if (buildings != null)
                {
                    foreach (tblBuilding building in userBuildings)
                    {
                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = building.BuildingID,
                            UserID = id,
                            USD = building.USD,
                            Create_Date = DateTime.Now,
                        });
                    }
                }

                //remove all the district relationships. Blow it all away.
                db.tblOrganizationMappings.RemoveRange(db.tblOrganizationMappings.Where(o => o.UserID == id));

                // remove all building relationships. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == id));

                db.SaveChanges();

                // add back the connections to the database.
                db.tblOrganizationMappings.AddRange(districtMappings);
                db.tblBuildingMappings.AddRange(buildingMappings);
                db.tblAuditLogs.Add(new tblAuditLog() { UserID = user.UserID, Create_Date = DateTime.Now, Update_Date = DateTime.Now, ModifiedBy = submitter.UserID, TableName = "tblUsers, tblOrginzation, tbleBuildingMapping", Value = "The student " + user.FirstName + " " + user.LastName + " was edit" });
                db.SaveChanges();

                return RedirectToAction("Portal", "Home");
            }
            catch (Exception e)
            {
                return RedirectToAction("Edit", "Manage", new { id, message = e.Message.ToString() });
            }
        }

        // POST: Manage/EditILPUser
        [HttpPost]
        public ActionResult EditILPUser(int id, HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                tblUser misUser = FindSupervisor.GetUSersMIS(user);

                if (misUser == null)
                {
                    misUser = FindSupervisor.GetUSersMIS(submitter);
                }

                if (misUser == null || user.UserID == misUser.UserID)
                {  // I'm my own grandpapa 
                    misUser = db.tblUsers.Where(u => u.UserID == 1).FirstOrDefault();
                }

                // EDIT the user
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(collection["teacherID"])) { user.TeacherID = collection["teacherID"]; }
                    if (!string.IsNullOrEmpty(collection["role"])) { user.RoleID = collection["role"]; }
                    user.FirstName = collection["FirstName"];
                    user.LastName = collection["LastName"];
                    user.Email = collection["userEmail"];

                    if (submitter.RoleID == "1" || submitter.RoleID == "7")
                    {
                        user.Archive = collection["isArchived"] != null;
                    }

                    //tblUserRole role = db.tblUserRoles.Where(u => u.UserID == id).FirstOrDefault();
                    //role.tblRole.RoleID = Convert.ToInt32(user.RoleID);

                    db.SaveChanges();
                }

                //UPDATE tblUserRoles SET RoleID = @NewRoleID WHERE UserID = @UserID and BookID = @BookID
                tblUserRole userRole = db.tblUserRoles.Where(u => u.UserID == id && u.BookID == "_ILP_").FirstOrDefault();
                userRole.RoleID = Convert.ToInt32(user.RoleID);

                // EDIT their avatar
                if (adminpersona != null)
                {
                    if (!string.IsNullOrEmpty(user.ImageURL))
                    {
                        // Delete exiting file
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Avatar/"), user.ImageURL));
                    }

                    // Save new file
                    string filename = Guid.NewGuid() + Path.GetFileName(adminpersona.FileName);
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), filename);

                    try
                    {
                        adminpersona.SaveAs(path);
                    }
                    catch (Exception e)
                    {
                        Console.Write("Avatar file can't be save. Exception:" + e.InnerException.ToString());
                    }

                    user.ImageURL = filename;
                }

                db.SaveChanges();

                List<tblOrganizationMapping> districtMappings = new List<tblOrganizationMapping>();
                List<tblBuildingMapping> buildingMappings = new List<tblBuildingMapping>();
                List<string> districts = new List<string>();
                List<string> buildings = new List<string>();

                if (collection["misDistrict"] != null)
                {
                    districts = new List<string>(collection["misDistrict"].ToString().Split(','));
                }

                if (collection["buildingIds"] != null)
                {
                    buildings = new List<string>(collection["buildingIds"].ToString().Split(','));
                }

                // removes any buildings not in the current list of usd's.
                List<tblBuilding> userBuildings = db.tblBuildings.Where(b => buildings.Contains(b.BuildingID) && districts.Contains(b.USD) && b.BuildingID != "0").ToList();

                if (districts != null)
                {
                    foreach (string district in districts)
                    {
                        districtMappings.Add(new tblOrganizationMapping()
                        {
                            AdminID = misUser.UserID,
                            UserID = id,
                            USD = district,
                            Create_Date = DateTime.Now,
                        });

                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = "0",
                            UserID = id,
                            USD = district,
                            Create_Date = DateTime.Now,
                        });
                    }
                }

                //remove all the district relationships. Blow it all away.
                db.tblOrganizationMappings.RemoveRange(db.tblOrganizationMappings.Where(o => o.UserID == id));

                // remove all building relationships. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == id));

                db.SaveChanges();

                // add back the connections to the database.
                db.tblOrganizationMappings.AddRange(districtMappings);
                db.tblBuildingMappings.AddRange(buildingMappings);
                db.tblAuditLogs.Add(new tblAuditLog() { UserID = user.UserID, Create_Date = DateTime.Now, Update_Date = DateTime.Now, ModifiedBy = submitter.UserID, TableName = "tblUsers, tblOrginzation, tbleBuildingMapping", Value = "The student " + user.FirstName + " " + user.LastName + " was edit" });
                db.SaveChanges();

                return RedirectToAction("Index", "ILP");
            }
            catch (Exception e)
            {
                return RedirectToAction("EditILPUser", "Manage", new { id, message = e.Message.ToString() });
            }
        }

        // GET: Manage/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Manage/Delete
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // RULES:
                // 1) Cannot delete yourself.
                // 2) Cannot delete if have staff
                // 3) Cannot delete a teacher if they have IEP data opened.
                // 4) Cannot delete a student based on these rules have data associated with them. Otherwise - archive

                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                if (user != null)
                {
                    // Cannot delete yourself foo.
                    if (User.Identity.Name == user.Email)
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</ strong > You cannot delete yourself from the system." });
                    }

                    // Cannot delete if the user has staff
                    if (db.tblOrganizationMappings.Any(u => u.AdminID == id))
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</strong> You must first remove all of the users under " + user.FirstName.ToString() + " " + user.LastName.ToString() + " before you can delete them." });
                    }

                    // TODO: Cannot delete the teacher if they have opened/associated IEPs

                    // Cannot delete a student if they have any IEP associated to them
                    if (db.tblIEPs.Any(u => u.UserID == id))
                    {
                        user.Archive = true; // you can archieve a student, not delete them.
                        db.SaveChanges();

                        return Json(new { Result = "warning", Message = "<strong>Warning!</strong> The student was archived." });
                    }

                    //delete studentInfo table if the userid is there.
                    //WE CAN NO LONGER DELETE STUDENT INFO. EVER!!!!!. 11/25/2019
                    //var info = db.tblStudentInfoes.FirstOrDefault(u => u.UserID == id);
                    //if (info != null)
                    //{
                    //    db.tblStudentInfoes.Remove(info);
                    //    db.SaveChanges();
                    //}

                    //delete from tlbOrganizationMapping all userId references.
                    List<tblOrganizationMapping> mappings = db.tblOrganizationMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (mappings.Count > 0)
                    {
                        db.tblOrganizationMappings.RemoveRange(mappings);
                        db.SaveChanges();
                    }

                    //delete from tblBuildingMapping all userId references.
                    List<tblBuildingMapping> buildings = db.tblBuildingMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (buildings.Count > 0)
                    {
                        db.tblBuildingMappings.RemoveRange(buildings);
                        db.SaveChanges();
                    }

                    //delete from tblStudentRelationships all userId references.
                    //WE CAN NO LONGER DELETE STUDENT RELATIONSHIPS. EVER!!!!!. 11/25/2019
                    //var relatioships = db.tblStudentRelationships.Where(r => r.UserID == user.UserID).ToList();
                    //if (relatioships.Count > 0)
                    //{
                    //    db.tblStudentRelationships.RemoveRange(relatioships);
                    //    db.SaveChanges();
                    //}

                    // archive user
                    user.Archive = true;

                    tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                    db.tblAuditLogs.Add(new tblAuditLog() { UserID = user.UserID, ModifiedBy = submitter.UserID, Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", Value = "Archived User " + user.FirstName + " " + user.LastName });

                    //db.tblUsers.Remove(user);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "<strong>Success!</strong> The user was successfully deleted from the system." });
                }

                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to find the user. Contact your admin." });
            }
            catch
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to delete the user. Contact your admin." });
            }
        }

        // POST: Manage/DeleteILPUser
        [HttpPost]
        public ActionResult DeleteILPUser(int id, FormCollection collection)
        {
            try
            {
                // RULES:
                // 1) Cannot delete yourself.
                // 2) Cannot delete if have staff
                // 3) Cannot delete a teacher if they have IEP data opened.
                // 4) Cannot delete a student based on these rules have data associated with them. Otherwise - archive

                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                if (user != null)
                {
                    // Cannot delete yourself foo.
                    if (User.Identity.Name == user.Email)
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</ strong > You cannot delete yourself from the system." });
                    }

                    // Cannot delete if the user has staff
                    if (db.tblOrganizationMappings.Any(u => u.AdminID == id))
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</strong> You must first remove all of the users under " + user.FirstName.ToString() + " " + user.LastName.ToString() + " before you can delete them." });
                    }

                    // TODO: Cannot delete the teacher if they have opened/associated IEPs

                    // Cannot delete a student if they have any IEP associated to them
                    //if (db.tblIEPs.Any(u => u.UserID == id))
                    //{
                    //    user.Archive = true; // you can archieve a student, not delete them.
                    //    db.SaveChanges();

                    //    return Json(new { Result = "warning", Message = "<strong>Warning!</strong> The student was archived." });
                    //}

                    //delete studentInfo table if the userid is there.
                    //WE CAN NO LONGER DELETE STUDENT INFO. EVER!!!!!. 11/25/2019
                    //var info = db.tblStudentInfoes.FirstOrDefault(u => u.UserID == id);
                    //if (info != null)
                    //{
                    //    db.tblStudentInfoes.Remove(info);
                    //    db.SaveChanges();
                    //}

                    //delete from tlbOrganizationMapping all userId references.
                    List<tblOrganizationMapping> mappings = db.tblOrganizationMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (mappings.Count > 0)
                    {
                        db.tblOrganizationMappings.RemoveRange(mappings);
                        db.SaveChanges();
                    }

                    //delete from tblBuildingMapping all userId references.
                    List<tblBuildingMapping> buildings = db.tblBuildingMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (buildings.Count > 0)
                    {
                        db.tblBuildingMappings.RemoveRange(buildings);
                        db.SaveChanges();
                    }

                    //delete from tblStudentRelationships all userId references.
                    //WE CAN NO LONGER DELETE STUDENT RELATIONSHIPS. EVER!!!!!. 11/25/2019
                    //var relatioships = db.tblStudentRelationships.Where(r => r.UserID == user.UserID).ToList();
                    //if (relatioships.Count > 0)
                    //{
                    //    db.tblStudentRelationships.RemoveRange(relatioships);
                    //    db.SaveChanges();
                    //}

                    // archive user
                    user.Archive = true;

                    tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                    db.tblAuditLogs.Add(new tblAuditLog() { UserID = user.UserID, ModifiedBy = submitter.UserID, Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "tblUsers", Value = "Archived ILP User " + user.FirstName + " " + user.LastName });

                    //db.tblUsers.Remove(user);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "<strong>Success!</strong> The user was successfully deleted from the system." });
                }

                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to find the user. Contact your admin." });
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to delete the user. Msg: " + e.Message + ". Contact your admin." });
            }
        }

        // POST: Manage/RemoveFromList/5
        [HttpPost]
        public ActionResult RemoveFromList(int id)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            tblOrganizationMapping boss = db.tblOrganizationMappings.Where(u => u.UserID == submitter.UserID).FirstOrDefault();
            tblOrganizationMapping userToRemove = db.tblOrganizationMappings.Where(u => u.AdminID == submitter.UserID && u.UserID == id).SingleOrDefault();

            if (userToRemove != null && boss != null)
            {
                db.tblOrganizationMappings.Add(
                    new tblOrganizationMapping
                    {
                        AdminID = boss.AdminID,
                        UserID = id,
                        USD = userToRemove.USD,
                        Create_Date = DateTime.Now,
                    });
                db.SaveChanges();

                db.tblOrganizationMappings.Remove(userToRemove);
                db.SaveChanges();

                return Json(new { Result = "Success", Message = "User successfully removed from your list." });
            }

            return Json(new { Result = "Error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/FilterUserList
        [HttpPost]
        public ActionResult FilterUserList(string DistrictId, string BuildingId, string RoleId, int? userId, int? activeType, int? statusActive)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (submitter != null)
            {
                string selectedDistrict = DistrictId == "-1" ? null : DistrictId;
                string selectedBuilding = BuildingId == "-1" ? null : BuildingId;
                int? searchUserId = userId.HasValue && userId.Value > -1 ? userId.Value : -1;
                int? searchHasIEP = activeType.HasValue && activeType == 1 ? 1 : activeType.HasValue && activeType == 2 ? 0 : -1;
                //bool? searchArchieve = submitter.RoleID == "1" ? true : false;

                Dictionary<string, object> NewPortalObject = new Dictionary<string, object>
                {
                    { "selectedDistrict", DistrictId },
                    { "selectedBuilding", BuildingId },
                    { "selectedRole", RoleId }
                };

                List<UserView> members = new List<UserView>();

                if (RoleId != "999")
                {
                    //members = db.uspUserList(submitter.UserID, selectedDistrict, selectedBuilding, null, null).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, RoleID = u.RoleID, isAssigned = u.isAssgined ?? false, statusCode = u.StatusCode, statusActive = u.StatusActive, hasIEP = u.hasIEP ?? false }).ToList();
                    members = db.uspUserAssignedList(submitter.UserID, selectedDistrict, selectedBuilding, null, null).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, isAssigned = false, statusCode = u.StatusCode, statusActive = u.StatusActive, hasIEP = u.hasIEP ?? false, KidsID = u.KIDSID.ToString() }).ToList();

                    if (searchUserId != -1)
                    {
                        members = members.Where(u => u.UserID == searchUserId).ToList();
                    }

                    if (RoleId != "-1")
                    {
                        members = members.Where(u => u.RoleID == RoleId).ToList();
                    }

                    if (searchHasIEP.HasValue && searchHasIEP != -1 && RoleId == "5")
                    {
                        bool hasIEP = searchHasIEP == 1;
                        members = members.Where(u => u.hasIEP == hasIEP).ToList();
                    }

                    if (statusActive.HasValue && RoleId == "5") //educational status
                    {
                        members = members.Where(u => u.statusActive == statusActive).ToList();
                    }
                }
                else // Unassigned Users.
                {
                    //members = db.uspUserList(submitter.UserID, selectedDistrict, selectedBuilding, null, true).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, RoleID = u.RoleID, isAssigned = u.isAssgined ?? false, hasIEP = u.hasIEP ?? false }).ToList();
                    members = db.uspUserAssignedList(submitter.UserID, selectedDistrict, selectedBuilding, null, true).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, isAssigned = false, statusCode = u.StatusCode, statusActive = u.StatusActive, hasIEP = u.hasIEP ?? false, KidsID = u.KIDSID.ToString() }).ToList();

                    if (searchUserId != -1)
                    {
                        members = members.Where(u => u.UserID == searchUserId).ToList();
                    }
                }

                string districtVal = DistrictId;

                if (string.IsNullOrEmpty(DistrictId) || DistrictId == "-1")
                {
                    districtVal = null;
                }

                var buildings = new List<BuildingsViewModel>();

                var buildingList = (from bm in db.tblBuildingMappings
                                    join b in db.tblBuildings on bm.USD equals b.USD
                                    where b.Active == 1
                                    && bm.BuildingID == b.BuildingID
                                    && bm.UserID == submitter.UserID
                                    && ((districtVal == null) || (b.USD == districtVal))
                                    orderby b.BuildingName
                                    select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

                buildings.AddRange(buildingList);

                NewPortalObject.Add("buildings", buildings);


                NewPortalObject.Add("members", members);
                return Json(new { Result = "success", Message = NewPortalObject }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        public ActionResult FilterStudentList(string DistrictId, string BuildingId, int? userId, int? activeType)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (submitter != null)
            {
                int? searchUserId = null;

                if (userId != null)
                {
                    if (userId.Value > -1)
                    {
                        searchUserId = userId.Value;
                    }
                }

                bool? searchActiveType = null;

                if (activeType.HasValue && activeType == 1)
                {
                    searchActiveType = true;
                }
                else if (activeType.HasValue && activeType == 2)
                {
                    searchActiveType = false;
                }


                List<string> myDistricts = new List<string>();
                List<string> myBuildings = new List<string>();
                List<string> myRoles = new List<string>() { "5" };

                Dictionary<string, object> NewPortalObject = new Dictionary<string, object>
                {
                    { "selectedDistrict", DistrictId },
                    { "selectedBuilding", BuildingId }
                };


                if (DistrictId == "-1")
                {
                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitter.UserID select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }
                else
                {
                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitter.UserID && org.USD == DistrictId select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }

                if (BuildingId == "-1")
                {
                    List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == submitter.UserID && myDistricts.Contains(buildingMap.USD) select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");
                }
                else
                {
                    List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == submitter.UserID && buildingMap.BuildingID == BuildingId select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                }


                List<UserView> members = new List<UserView>();

                if (searchActiveType == null)
                {

                    IQueryable<UserView> userMembers = db.vw_UserList.Where(ul => ul.RoleID == student && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID) && ((searchUserId == null) || (ul.UserID == searchUserId.Value))).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, RoleID = u.RoleID }).GroupBy(u => u.UserID).Select(u => u.FirstOrDefault());

                    members = (from u in userMembers
                               join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                               where o.AdminID == submitter.UserID

                               select new UserView()
                               {
                                   UserID = u.UserID,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   RoleID = u.RoleID
                               }).Distinct().ToList();


                }
                else
                {

                    IQueryable<UserView> userMembers = db.vw_UserList.Where(ul => ul.RoleID == student && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID) && ((searchUserId == null) || (ul.UserID == searchUserId.Value)) && ((searchActiveType == null) || (ul.IsActive == searchActiveType.Value))).Select(u => new UserView() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, RoleID = u.RoleID }).GroupBy(u => u.UserID).Select(u => u.FirstOrDefault());

                    members = (from u in userMembers
                               join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                               where o.AdminID == submitter.UserID

                               select new UserView()
                               {
                                   UserID = u.UserID,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   RoleID = u.RoleID
                               }).Distinct().ToList();


                }

                NewPortalObject.Add("members", members);
                return Json(new { Result = "success", Message = NewPortalObject }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }


        // POST: Manage/RemoveFromTeacherList
        [HttpPost]
        public ActionResult RemoveFromTeacherList(int id, int teacherid)
        {
            tblUser teacher = db.tblUsers.FirstOrDefault(u => u.UserID == teacherid);

            tblOrganizationMapping userToRemove = db.tblOrganizationMappings.Where(u => u.AdminID == teacher.UserID && u.UserID == id).SingleOrDefault();
            if (userToRemove != null)
            {
                db.tblOrganizationMappings.Remove(userToRemove);
                db.SaveChanges();

                return Json(new { Result = "Success", Message = "User successfully removed from your list." });
            }

            return Json(new { Result = "Error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/AddDistrictContact
        [HttpPost]
        [Authorize]
        public ActionResult AddDistrictContact(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    string USD = collection["districtUSD"].ToString();

                    tblContact districtContact = db.tblContacts.Where(c => c.USD == USD).FirstOrDefault();
                    if (districtContact != null)
                    {
                        districtContact.ContactName = collection["districtContact"].ToString();
                        districtContact.Email = collection["districtEmail"].ToString();
                        districtContact.Address1 = collection["districtContactAddress1"].ToString();
                        districtContact.Address2 = collection["districtContactAddress2"].ToString();
                        districtContact.City = collection["districtCity"].ToString();
                        districtContact.State = collection["districtState"].ToString();
                        districtContact.Zip = collection["districtContactZipCode"].ToString();
                        districtContact.Phone = collection["districtContactPhone"].ToString();
                        districtContact.Active = 1;
                        districtContact.Update_Date = DateTime.Now;
                    }
                    else
                    {
                        districtContact = new tblContact();
                        tblDistrict district = db.tblDistricts.Where(d => d.USD == USD).FirstOrDefault();

                        districtContact.ContactName = collection["districtContact"].ToString();
                        districtContact.Email = collection["districtEmail"].ToString();
                        districtContact.Address1 = collection["districtContactAddress1"].ToString();
                        districtContact.Address2 = collection["districtContactAddress2"].ToString();
                        districtContact.City = collection["districtCity"].ToString();
                        districtContact.State = collection["districtState"].ToString();
                        districtContact.Zip = collection["districtContactZipCode"].ToString();
                        districtContact.Phone = collection["districtContactPhone"].ToString();
                        districtContact.Active = 1;
                        districtContact.USD = district.USD;
                        districtContact.Create_Date = DateTime.Now;
                        districtContact.Update_Date = DateTime.Now;

                        db.tblContacts.Add(districtContact);
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "Your district contact has been updated." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit/add the district contact. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while saving the district contact. Please try again or contact your administrator." }, JsonRequestBehavior.AllowGet);
        }

        // POST : Manage/ContactsInDistricts
        [HttpPost]
        [Authorize]
        public ActionResult ContactsInDistricts(string USD)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                tblContact districtContact = (from contact in db.tblContacts where contact.Active == 1 && contact.USD == USD select contact).FirstOrDefault();
                if (districtContact != null)
                {
                    return Json(new { Result = "success", Message = new { ContactID = districtContact.ContactID, USD = districtContact.USD, ContactName = districtContact.ContactName, Email = districtContact.Email, Phone = districtContact.Phone, Address1 = districtContact.Address1, Address2 = districtContact.Address2, City = districtContact.City, State = districtContact.State, Zip = districtContact.Zip } }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "success", Message = districtContact }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "There was an error while getting contacts in the district" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveDistrictContact(string USD)
        {
            tblContact districtContact = db.tblContacts.Where(c => c.USD == USD).FirstOrDefault();
            if (districtContact != null)
            {
                districtContact.Active = 0;

                db.SaveChanges();

                return Json(new { Result = "success", Message = "The district contact was removed." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to edit contacts. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDistricts(int id)
        {
            try
            {
                tblUser user = db.tblUsers.FirstOrDefault(u => u.UserID == id);
                //List<tblDistrict> districts = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where user.UserID == bm.UserID select d).Distinct().ToList();
                var districts = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where user.UserID == bm.UserID select new { USD = d.USD, DistrictName = d.DistrictName, }).Distinct().ToList();
                if (districts != null)
                {
                    return Json(new { Result = "success", Message = districts }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get districts. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateIEPAnnual(int Stid, int Iepid)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                db.uspCopyIEP(Iepid, user.UserID, false);

                tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == Stid).FirstOrDefault();

                tblIEP annual = db.tblIEPs.Where(i => i.UserID == Stid && i.Amendment == false && i.IepStatus.ToUpper() == IEPStatus.DRAFT && i.IsActive).FirstOrDefault();
                int AnnualId = annual.IEPid;

                if (studentDetails != null)
                {
                    annual.StatusCode = studentDetails.StatusCode;
                    db.SaveChanges();

                }

                return Json(new { Result = "success", Message = AnnualId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened :" + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateIEPAmendment(int Stid, int IepId, bool? amend = true)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                uspCopyIEP_Result results = db.uspCopyIEP(IepId, user.UserID, amend).ToList().First();

                tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == Stid).FirstOrDefault();
                tblIEP amendment = db.tblIEPs.Where(i => i.UserID == Stid && i.Amendment == true && i.AmendingIEPid == IepId && i.IEPid == results.ToIEP).FirstOrDefault();

                int AmendmentId = amendment.IEPid;

                if (studentDetails != null)
                {
                    amendment.StatusCode = studentDetails.StatusCode;
                    db.SaveChanges();
                }

                return Json(new { Result = "success", Message = AmendmentId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened :" + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetAllStudentsInBuildings(int id, string filterStudentName)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(u => u.UserID == id);

            try
            {
                List<string> teacherBuildings = (from bm in db.tblBuildingMappings where bm.UserID == teacher.UserID select bm.BuildingID).Distinct().ToList();
                List<int> studentsInTheBuildings = (from bm in db.tblBuildingMappings
                                                    join user in db.tblUsers on bm.UserID
                  equals user.UserID
                                                    where user.RoleID == "5"
                                                    && ((filterStudentName == null) || (user.FirstName.Contains(filterStudentName) || user.LastName.Contains(filterStudentName)))
                                                    && teacherBuildings.Contains(bm.BuildingID)
                                                    select bm.UserID).ToList();
                List<int> alreadyAssignedStudents = (from o in db.tblOrganizationMappings where o.AdminID == teacher.UserID select o.UserID).Distinct().ToList();

                // Get all users that are students NOT archive, NOT already in the teachers list and in the Teachers's building!!!!
                //var students = db.tblUsers.Where(u => u.Archive != true && studentsInTheBuildings.Contains(u.UserID) && !alreadyAssignedStudents.Contains(u.UserID)).ToList();

                List<Student> students = (from u in db.tblUsers
                                          join su in db.tblStudentInfoes on u.UserID equals su.UserID
                                          join b in db.tblBuildings on su.BuildingID equals b.BuildingID
                                          where
                                          u.RoleID == "5"
                                          && !(u.Archive ?? false)
                                          && studentsInTheBuildings.Contains(u.UserID)
                                          && !alreadyAssignedStudents.Contains(u.UserID)
                                          && teacherBuildings.Contains(b.BuildingID)
                                          select new Student
                                          {
                                              UserID = u.UserID,
                                              FirstName = u.FirstName,
                                              LastName = u.LastName,
                                              ImageURL = u.ImageURL,
                                              BuildingName = b.BuildingName
                                          }).Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();


                return Json(new { Result = "success", Message = students }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        [Authorize]
        public ActionResult AddStudentsToTeacher(int id, int[] students)
        {
            try
            {
                tblUser teacher = db.tblUsers.Where(u => u.UserID == id).SingleOrDefault();
                foreach (int student in students)
                {
                    tblUser studentUser = db.tblUsers.Where(u => u.UserID == student).SingleOrDefault();
                    tblOrganizationMapping newRelation = new tblOrganizationMapping()
                    {
                        AdminID = teacher.UserID,
                        UserID = student,
                        USD = (from bm in db.tblBuildingMappings where bm.UserID == studentUser.UserID select bm.USD).FirstOrDefault(),
                        Create_Date = DateTime.Now,
                    };
                    db.tblOrganizationMappings.Add(newRelation);
                    db.SaveChanges();
                }

                return Json(new { Result = "Success", Message = "Wooty Woot" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetBuildings(string id, int userId)
        {
            try
            {
                // A list of all the building ids the current user has joined.
                IQueryable<string> mappings = from m in db.tblBuildingMappings
                                              where m.UserID == userId
                                              select m.BuildingID;

                // Give me the list of all the buildings in the current district that are user is NOT already in.
                IOrderedQueryable<tblBuilding> listOfBuildings = from b in db.tblBuildings
                                                                 where !(mappings).Contains(b.BuildingID)
                                                                 && b.USD == id && b.Active == 1
                                                                 orderby b.BuildingName
                                                                 select b;

                if (listOfBuildings != null)
                {
                    return Json(new { Result = "success", Message = listOfBuildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditReevalSigned(int studentId)
        {
            int id = Convert.ToInt32(Request.QueryString["userId"]);
            int dateId = Convert.ToInt32(Request.QueryString["ourDateId"]);
            string editDate = Request.QueryString["dateValue"].ToString();

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPastSignedReEvaluationDates(int studentId)
        {
            List<tblArchiveEvaluationDateSigned> signedDates = db.tblArchiveEvaluationDateSigneds.Where(a => a.userID == studentId).OrderBy(a => a.evaluationDateSigned).ToList();
            if (signedDates.Count() > 0)
            {
                return Json(new { Result = "success", Dates = signedDates }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "success", Message = "There are no past Re-Evaluation Signed dates for this student" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPastCompletedReEvaluationDates(int studentId)
        {
            List<tblArchiveEvaluationDate> signedDates = db.tblArchiveEvaluationDates.Where(a => a.userID == studentId).OrderBy(a => a.evalutationDate).ToList();
            if (signedDates.Count() > 0)
            {
                return Json(new { Result = "success", Dates = signedDates }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "success", Message = "There are no past Re-Evaluation Signed dates for this student" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult editReevalSignedDates(int studentId, int dateId, string newDateValue)
        {
            DateTime newDate = Convert.ToDateTime(newDateValue);

            if (newDate != null)
            {
                tblArchiveEvaluationDateSigned signedDate = db.tblArchiveEvaluationDateSigneds.Where(d => d.archiveEvaluationDateSignedID == dateId && d.userID == studentId).SingleOrDefault();
                if (signedDate != null)
                {
                    tblUser modifier = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                    signedDate.evaluationDateSigned = newDate;
                    signedDate.ModifiedBy = modifier.UserID;

                    db.SaveChanges();

                    return Json(new { Result = "success" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "There was an error while tryring to edit the date" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult editReevalCompletDates(int studentId, int dateId, string newDateValue)
        {
            DateTime newDate = Convert.ToDateTime(newDateValue);

            if (newDate != null)
            {
                tblArchiveEvaluationDate signedDate = db.tblArchiveEvaluationDates.Where(d => d.archiveEvaluationDateID == dateId && d.userID == studentId).SingleOrDefault();
                if (signedDate != null)
                {
                    tblUser modifier = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                    signedDate.evalutationDate = newDate;
                    signedDate.ModifiedBy = modifier.UserID;

                    db.SaveChanges();

                    return Json(new { Result = "success" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "There was an error while tryring to edit the date" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult deleteReEvaluationDates(int dateId, bool Completed)
        {
            if (Completed)
            {
                tblArchiveEvaluationDate Date = db.tblArchiveEvaluationDates.Where(d => d.archiveEvaluationDateID == dateId).FirstOrDefault();
                if (Date != null)
                {
                    db.tblArchiveEvaluationDates.Remove(Date);
                }
            }
            else
            {
                tblArchiveEvaluationDateSigned Date = db.tblArchiveEvaluationDateSigneds.Where(d => d.archiveEvaluationDateSignedID == dateId).FirstOrDefault();
                if (Date != null)
                {
                    db.tblArchiveEvaluationDateSigneds.Remove(Date);
                }
            }

            try { db.SaveChanges(); }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "success" }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetBuildingsByDistrictId(string districtId)
        {
            try
            {
                if (!string.IsNullOrEmpty(districtId))
                {
                    List<vw_BuildingList> buildings = db.vw_BuildingList.Where(b => b.USD == districtId).ToList();
                    return Json(new { Result = "success", DistrictBuildings = buildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllBuilingsByDistrictIds(string districtIds)
        {
            try
            {
                string[] countrycode = null;
                countrycode = districtIds.Split(',');

                if (districtIds.Length > 0)
                {
                    List<vw_BuildingList> buildings = db.vw_BuildingList.Where(b => countrycode.Contains(b.USD)).ToList();
                    return Json(new { Result = "success", DistrictBuildings = buildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult SaveBuildingsToUser(string USD, int userId, string[] buildings)
        {
            try
            {
                foreach (string building in buildings)
                {
                    tblBuildingMapping mapping = new tblBuildingMapping() { USD = USD, UserID = userId, BuildingID = building, Create_Date = DateTime.Now };
                    db.tblBuildingMappings.Add(mapping);
                    db.SaveChanges();
                }

                return Json(new { Result = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Home");
        }

        protected void CreateReevalSignedArchive(int studentId, DateTime reEvaldate)
        {
            if (reEvaldate != null)
            {
                IQueryable<tblArchiveEvaluationDateSigned> archives = db.tblArchiveEvaluationDateSigneds.Where(i => i.userID == studentId && DbFunctions.TruncateTime(i.evaluationDateSigned) == reEvaldate.Date).AsQueryable();
                if (archives.Count() == 0)
                {
                    int createBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault().UserID;
                    db.tblArchiveEvaluationDateSigneds.Add(new tblArchiveEvaluationDateSigned { evaluationDateSigned = reEvaldate.Date, Create_Date = DateTime.Now, Update_Date = DateTime.Now, userID = studentId, CreatedBy = createBy });
                    db.SaveChanges();
                }
            }
        }

        protected void CreateReevalCompletedArchive(int studentId, DateTime reCompleted)
        {
            if (reCompleted != null)
            {
                IQueryable<tblArchiveEvaluationDate> archives = db.tblArchiveEvaluationDates.Where(i => i.userID == studentId && DbFunctions.TruncateTime(i.evalutationDate) == reCompleted.Date).AsQueryable();
                if (archives.Count() == 0)
                {
                    int createBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault().UserID;
                    db.tblArchiveEvaluationDates.Add(new tblArchiveEvaluationDate { evalutationDate = reCompleted.Date, Create_Date = DateTime.Now, Update_Date = DateTime.Now, userID = studentId, CreatedBy = createBy });
                    db.SaveChanges();
                }
            }
        }


        protected void SendExitEmail(string assignedUSD, string studentName, string exitDate, string exitCode, string exitNotes)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            int userDistrictId = 0;
            string usdName = assignedUSD;
            string misRole = "2"; //level 4

            List<tblUser> list = (from org in db.tblOrganizationMappings
                                  join user in db.tblUsers
                                      on org.UserID equals user.UserID
                                  where !(user.Archive ?? false) && (user.RoleID == misRole) && org.USD == assignedUSD
                                  select user).Distinct().ToList();

            tblDistrict usd = db.tblDistricts.Where(o => o.USD == assignedUSD).FirstOrDefault();

            if (list != null && list.Any())
            {

                SmtpClient smtpClient = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress("GreenbushIEP@greenbush.org"));

                foreach (tblUser misUser in list)
                {
                    if (userDistrictId == 0)
                    {
                        userDistrictId = misUser.UserID;
                    }

                    if (!string.IsNullOrEmpty(misUser.Email))
                    {
                        mailMessage.To.Add(misUser.Email);
                    }


                }


                StringBuilder sb = new StringBuilder();
                sb.Append("The following student was updated with an Exit status: ");
                sb.AppendFormat("\n\nStudent Name: {0}", studentName);
                sb.AppendFormat("\nAssigned USD: {0}", usd == null ? assignedUSD : usd.DistrictName);
                sb.AppendFormat("\nCode: {0}", exitCode);
                sb.AppendFormat("\nExit Date: {0}", exitDate);
                sb.AppendFormat("\nExit Notes: {0}", exitNotes);
                sb.AppendFormat("\nSubmitted By: {0}", string.Format("{0} {1}", submitter.FirstName, submitter.LastName));

                sb.Append("\n\nContact melanie.johnson@greenbush.org or (620) 724 - 6281 if you need any assistance.");
                sb.Append("\n\nURL: https://greenbushbackpack.org");

                mailMessage.Subject = "Student Exit";

                mailMessage.Body = sb.ToString();

                smtpClient.Send(mailMessage);

            }
        }


        protected List<TeacherView> GetTeacherByBuilding(string buildingId, string usd)
        {
            List<TeacherView> list = new List<TeacherView>();

            try
            {

                List<string> myRoles = new List<string>() { "2", "3", "4", "6" };
                List<vw_UserList> teachers = new List<vw_UserList>();

                teachers = db.vw_UserList
                        .Where(ul => myRoles.Contains(ul.RoleID) && ul.BuildingID == buildingId && ul.USD.Contains(usd))
                        .GroupBy(u => u.UserID)
                        .Select(u => u.FirstOrDefault()).OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

                if (teachers != null && teachers.Count > 0)
                {
                    foreach (vw_UserList teacher in teachers)
                    {
                        TeacherView tv = new TeacherView() { Name = string.Format("{0}, {1}", teacher.LastName, teacher.FirstName), UserID = teacher.UserID };
                        list.Add(tv);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }

            return list;

        }

        #region ReportFilters

        [HttpGet]
        public ActionResult ReportFilterDistrict()
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                var districtList = (from org in db.tblOrganizationMappings
                                    join district in db.tblDistricts on org.USD equals district.USD
                                    where org.UserID == user.UserID
                                    orderby district.DistrictName
                                    select new DistrictViewModel { USD = district.USD, Name = district.DistrictName }).Distinct().ToList();

                List<DistrictViewModel> districts = new List<DistrictViewModel>();

                districts.AddRange(districtList);


                return Json(new { Result = "success", Districts = districts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ReportFilterProviderByDistrictId(string districtId)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                var providerList = new List<ProviderViewModel>();

                //if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
                //{
                //	var providerId = 0;

                //	if (user.TeacherID != null)
                //	{
                //		var teacherProvider = GetProviderByProviderCode(user.TeacherID);
                //		providerList.Add(teacherProvider);

                //	}
                //	else
                //	{
                //		providerList.Add(new ProviderViewModel { Name = string.Format("{0}, {1}", user.LastName, user.FirstName), ProviderID = providerId });
                //	}

                //}

                if (!string.IsNullOrEmpty(districtId))
                {


                    providerList.Add(new ProviderViewModel { Name = "All", ProviderID = -1 });

                    var providers = db.uspServiceProviders(districtId)
                        .Select(u => new ProviderViewModel() { ProviderID = u.ProviderID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
                        .Distinct()
                        .OrderBy(o => o.Name)
                        .ToList();

                    providerList.AddRange(providers);

                    return Json(new { Result = "success", Providers = providerList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ReportFilterBuildingsByDistrictId(string districtId)
        {
            try
            {
                if (!string.IsNullOrEmpty(districtId))
                {

                    tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                    var buildings = new List<BuildingsViewModel>();

                    var buildingList = (from bm in db.tblBuildingMappings
                                        join b in db.tblBuildings on bm.USD equals b.USD
                                        where b.Active == 1
                                        && bm.BuildingID == b.BuildingID
                                        && bm.UserID == user.UserID
                                        && b.USD == districtId
                                        orderby b.BuildingName
                                        select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

                    if (buildingList.Count() > 1 || districtId == "-1")
                    {
                        var allOption = new BuildingsViewModel() { BuildingName = "All", BuildingID = "-1", BuildingUSD = "-1" };
                        buildings.Add(allOption);
                    }


                    buildings.AddRange(buildingList);

                    return Json(new { Result = "success", DistrictBuildings = buildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReportFilterTeacher(string selectedDistrict, string selectedBuilding)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedDistrict))
                {
                    tblUser user = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
                    {
                        //just add themselves to the list				
                        List<TeacherView> teacherList = new List<TeacherView>();
                        TeacherView tv = new TeacherView() { Name = string.Format("{0}, {1}", user.LastName, user.FirstName), UserID = user.UserID };
                        teacherList.Add(tv);

                        return Json(new { Result = "success", TeacherList = teacherList }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        List<string> myRoles = new List<string>() { "3", "4", "6" };

                        if (user.RoleID == mis || user.RoleID == owner)
                            myRoles.Add("2");

                        if (selectedBuilding == "-1")
                            selectedBuilding = null;

                        var teachers = db.uspUserList(user.UserID, selectedDistrict, selectedBuilding, null, null)
                            .Where(ul => myRoles.Contains(ul.RoleID))
                            .Select(u => new TeacherView() { UserID = u.UserID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
                            .OrderBy(o => o.Name).ToList();

                        if (teachers.Count() > 1)
                        {
                            var allOption = new TeacherView() { Name = "All", UserID = -1 };
                            teachers.Insert(0, allOption);
                        }

                        return Json(new { Result = "success", TeacherList = teachers }, JsonRequestBehavior.AllowGet);
                    }


                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReportFilterStudent(string selectedDistrict, string selectedBuilding, string selectedTeacher)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedDistrict))
                {
                    tblUser user = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    List<string> myRoles = new List<string>() { "5" }; //students

                    if (selectedBuilding == "-1")
                        selectedBuilding = null;

                    if (selectedTeacher == "-1")
                        selectedTeacher = null;


                    if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
                    {
                        //can only see their own students
                        var students = db.uspUserList(user.UserID, selectedDistrict, selectedBuilding, null, null)
                                .Where(ul => myRoles.Contains(ul.RoleID))
                                .Select(u => new TeacherView() { UserID = u.UserID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
                                .OrderBy(o => o.Name);


                        var teacherStudents = (from student in students
                                               join o in db.tblOrganizationMappings on student.UserID equals o.UserID
                                               where o.AdminID == user.UserID
                                               select new TeacherView()
                                               {
                                                   UserID = student.UserID,
                                                   Name = student.Name
                                               }).Distinct().ToList();

                        teacherStudents.Insert(0, new TeacherView() { Name = "All", UserID = -1 });

                        return Json(new { Result = "success", StudentList = teacherStudents }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(selectedTeacher))
                        {
                            //get based on user id and district and building
                            var students = db.uspUserList(user.UserID, selectedDistrict, selectedBuilding, null, null)
                            .Where(ul => myRoles.Contains(ul.RoleID))
                            .Select(u => new TeacherView() { UserID = u.UserID, Name = u.LastName + ", " + u.FirstName })
                            .OrderBy(o => o.Name).ToList();

                            students.Insert(0, new TeacherView() { Name = "All", UserID = -1 });

                            return Json(new { Result = "success", StudentList = students }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //get based on selected teachers
                            selectedTeacher = selectedTeacher.TrimEnd(',');
                            var studentList = new List<TeacherView>();

                            List<string> myTeachers = string.IsNullOrEmpty(selectedTeacher) ? new List<string>() : selectedTeacher.Split(',').ToList();

                            foreach (var teacher in myTeachers)
                            {
                                Int32.TryParse(teacher, out int teacherId);

                                var students = db.uspUserList(teacherId, selectedDistrict, selectedBuilding, null, null)
                                .Where(ul => myRoles.Contains(ul.RoleID))
                                .Select(u => new TeacherView() { UserID = u.UserID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
                                .OrderBy(o => o.Name);

                                var teacherStudents = (from student in students
                                                       join o in db.tblOrganizationMappings on student.UserID equals o.UserID
                                                       where o.AdminID == teacherId
                                                       select new TeacherView()
                                                       {
                                                           UserID = student.UserID,
                                                           Name = student.Name
                                                       }).Distinct().ToList();


                                studentList.AddRange(teacherStudents);
                            }

                            studentList.Insert(0, new TeacherView() { Name = "All", UserID = -1 });

                            return Json(new { Result = "success", StudentList = studentList.Distinct().OrderBy(o => o.Name).ToList() }, JsonRequestBehavior.AllowGet);

                        }
                    }

                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReportFilterStudentByProvider(string selectedDistrict, string selectedBuilding, string selectedProvider)
        {
            try
            {
                tblUser user = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                //if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
                //{
                //	selectedProvider = "0";

                //	if (user.TeacherID != null)
                //	{
                //		var teacherProvider = GetProviderByProviderCode(user.TeacherID);
                //		selectedProvider = teacherProvider.ProviderID.ToString();

                //	}					
                //}

                if (!string.IsNullOrEmpty(selectedDistrict))
                {

                    List<string> myRoles = new List<string>() { "5" }; //students

                    if (selectedBuilding == "-1")
                        selectedBuilding = null;

                    if (selectedProvider == "-1")
                        selectedProvider = null;

                    if (selectedDistrict == "-1")
                        selectedDistrict = null;

                    if (string.IsNullOrEmpty(selectedProvider))
                    {
                        //get based on user id and district and building
                        var students = db.uspUserListByProvider(user.UserID, selectedDistrict, selectedBuilding, null)
                        .Select(u => new TeacherView() { UserID = u.UserID, Name = u.LastName + ", " + u.FirstName })
                        .OrderBy(o => o.Name).ToList();

                        //students.Insert(0, new TeacherView() { Name = "All", UserID = -1 });

                        return Json(new { Result = "success", StudentList = students }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //get based on selected teachers
                        selectedProvider = selectedProvider.TrimEnd(',');
                        var studentList = new List<TeacherView>();

                        List<string> myProviders = string.IsNullOrEmpty(selectedProvider) ? new List<string>() : selectedProvider.Split(',').ToList();

                        foreach (var provider in myProviders)
                        {
                            var providerId = 0;
                            Int32.TryParse(provider, out providerId);

                            var students = db.uspUserListByProvider(user.UserID, selectedDistrict, selectedBuilding, providerId)
                            .Select(u => new TeacherView() { UserID = u.UserID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
                            .OrderBy(o => o.Name);

                            studentList.AddRange(students);
                        }

                        //studentList.Insert(0, new TeacherView() { Name = "All", UserID = -1 });

                        return Json(new { Result = "success", StudentList = studentList.Distinct().OrderBy(o => o.Name).ToList() }, JsonRequestBehavior.AllowGet);

                    }


                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region helpers

        [NonAction]
        public string ConvertToRoleName(string roleId)
        {
            switch (roleId)
            {
                case "1":
                    return "Level 5";
                case "2":
                    return "Level 4";
                case "3":
                    return "Level 3";
                case "4":
                    return "Level 2";
                case "5":
                    return "Student";
                case "6":
                    return "Nurse";
                default:
                    return "";
            }
        }

        [NonAction]
        public string ConvertToILPRoleName(int UserId)
        {
            int roleId = db.tblUserRoles.Where(u => u.UserID == UserId && u.BookID == "_ILP_").FirstOrDefault().RoleID;

            switch (roleId)
            {
                case 7:
                    return "Administrator";
                case 8:
                    return "Instructor";
                case 9:
                    return "Viewer";
                case 10:
                    return "Learner";
                default:
                    return "";
            }
        }

        private ProviderViewModel GetProviderByProviderCode(string providerCode)
        {
            var providerObj = new ProviderViewModel();
            var provider = (from p in db.tblProviders
                            select p).FirstOrDefault();

            if (provider != null)
            {
                providerObj.Name = string.Format("{0}, {1}", provider.LastName, provider.FirstName);
                providerObj.ProviderID = provider.ProviderID;
                providerObj.ProviderCode = provider.ProviderCode;
            }

            return providerObj;
        }

        private string GetDisability(string value)
        {
            string fullName = "";
            tblDisability disablity = db.tblDisabilities.Where(o => o.DisabilityCode == value).FirstOrDefault();
            if (disablity != null)
            {
                fullName = string.Format("({0}) {1}", disablity.DisabilityCode, disablity.DisabilityDescription);
            }

            return fullName;
        }

        private string GetLanguage(string value)
        {
            string fullName = "";
            switch (value)
            {

                case "EN":
                    fullName = "EN - English";
                    break;

                case "ES": fullName = "ES - Spanish"; break;

                case "DE": fullName = "DE - German"; break;

                case "FR": fullName = "FR - French"; break;

                case "RU": fullName = "RU - Russian"; break;

                case "A": fullName = "A - Augmentative Communication"; break;

                case "AR": fullName = "AR - Arabic"; break;

                case "DIN": fullName = "DIN - Dinka(Sudanese)"; break;

                case "HMN": fullName = "HMN - Hmong"; break;

                case "IRA": fullName = "IRA - Farsi(Iranian)"; break;

                case "KHMR": fullName = "KHMR - Khmen / Cambodian"; break;

                case "KO": fullName = "KO - Korean"; break;

                case "LO": fullName = "LO - Lao"; break;

                case "M": fullName = "M - Mode of Communication"; break;

                case "NAT-AM": fullName = "NAT - A, -Navtive America(Kickapoo, Pottawatomie, etc.)"; break;

                case "N": fullName = "N - Non - Verbal and Non-Sign"; break;

                case "O": fullName = "O - Other"; break;

                case "PH": fullName = "PH - Phllippine - Tagalog"; break;

                case "SO": fullName = "SO - Somali"; break;

                case "TH": fullName = "TH - Thai"; break;

                case "VI": fullName = "VI - Vietnamese"; break;

                case "YU": fullName = "YU - Yugoslavian, Croatian, Bosnian, Serb"; break;

                case "ZH-ZH-CMN": fullName = "ZH - ZH - CMN - Mandarin"; break;

                case "ZH-YUE": fullName = "ZH - YUE - Cantonese"; break;

                case "ZH-WUU": fullName = "ZH - WUU - Wu"; break;
            }

            return fullName;

        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}