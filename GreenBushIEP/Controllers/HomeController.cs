using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace GreenbushIep.Controllers
{
    public class HomeController : Controller
    {
        private const string owner = "1";
        private const string mis = "2";
        private const string admin = "3";
        private const string teacher = "4";
        private const string student = "5";

        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: Home
        [AllowAnonymous]
        public ActionResult Index()
        {
            // Invalidate the Cache on the Client Side
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return View();
        }

        [AllowAnonymous]
        public ActionResult Portal()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(owner))
                {
                    return RedirectToAction("OwnerPortal");
                }
                else if (User.IsInRole(mis))
                {
                    return RedirectToAction("MISPortal");
                }
                else if (User.IsInRole(admin))
                {
                    return RedirectToAction("AdminPortal");
                }
                else if (User.IsInRole(teacher))
                {
                    return RedirectToAction("TeacherPortal");
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = owner)]
        public ActionResult OwnerPortal()
        {
            tblUser owner = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (owner != null)
            {
                // get all the admins and orphans in the database.
                var mis = (from org in db.tblOrganizationMappings
                           join user in db.tblUsers
                               on org.UserID equals user.UserID
                           where (org.AdminID == owner.UserID || user.RoleID == "2") && !(user.Archive ?? false)
                           select user).Distinct().OrderBy(u => u.RoleID).ToList();

                UserOrganizationViewModel model = new UserOrganizationViewModel();
                model.user = owner;
                model.staff = mis.ToList();

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = mis)]
        public ActionResult MISPortal()
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                // get all the admins in the database that are active and under this MIS user.
                var admin = (from org in db.tblOrganizationMappings
                             join user in db.tblUsers
                                 on org.UserID equals user.UserID
                             where (org.AdminID == MIS.UserID) && !(user.Archive ?? false)
                             select user).Distinct().OrderBy(u => u.RoleID).ToList();

                UserOrganizationViewModel model = new UserOrganizationViewModel();
                model.user = MIS;
                model.staff = admin.ToList();

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize(Roles = mis)]
        public ActionResult LoadMISSection(string view)
        {
            if (view == "CalendarModule")
            {
                tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                if (MIS != null)
                {
                    int SchoolYear = (DateTime.Now.Month > 7) ? DateTime.Now.AddYears(1).Year : DateTime.Now.Year;
                    List<tblCalendarTemplate> temp = db.tblCalendarTemplates.Where(t => t.NoService == true).ToList();

                    var MISDistrictList = (from buildingMaps in db.tblBuildingMappings
                                           join districts in db.tblDistricts
                                                on buildingMaps.USD equals districts.USD
                                           where buildingMaps.UserID == MIS.UserID
                                           select districts).Distinct().ToList();

                    var MISBuildingList = (from buildingMaps in db.tblBuildingMappings
                                           join buildings in db.tblBuildings
                                              on buildingMaps.BuildingID equals buildings.BuildingID
                                           where buildingMaps.UserID == MIS.UserID
                                           select buildings).Distinct().OrderBy(b => b.BuildingID).ToList();

                    List<tblCalendar> defaultCalendar = new List<tblCalendar>();
                    foreach (var day in temp)
                    {
                        tblCalendar calendar = new tblCalendar();
                        calendar.canHaveClass = day.canHaveClass;
                        calendar.Day = day.Day;
                        calendar.Month = day.Month;
                        calendar.Year = day.Year;
                        calendar.NoService = day.NoService;
                        calendar.SchoolYear = day.SchoolYear;

                        defaultCalendar.Add(calendar);
                    }

                    MISCalendarViewModel model = new MISCalendarViewModel();
                    model.districts = MISDistrictList;
                    model.buildings = MISBuildingList;
                    model.calendarDays = defaultCalendar;

                    return PartialView("_ModuleCalendarSection", model);
                }
            }

            if (view == "ServiceProviderModule")
            {
                List<tblProvider> listOfProviders = new List<tblProvider>();
                tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                if (MIS != null)
                {
                    listOfProviders = db.tblProviders.Where(p => p.UserID == MIS.UserID).ToList();
                }

                return PartialView("_ModuleServiceProviders", listOfProviders);
            }

            // Unknow user or view.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpPost]
        [Authorize(Roles = mis)]
        public ActionResult UpdateProvidersList(string name, int pk, string value)
        {
            tblUser owner = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (owner != null)
            {
                tblProvider provider = db.tblProviders.Where(p => p.ProviderID == pk).SingleOrDefault();
                if (provider != null)
                {
                    provider.Name = value.ToString();

                    db.SaveChanges();

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tblProvider providerName = new tblProvider();
                    providerName.Name = value.ToString();
                    providerName.UserID = owner.UserID;

                    db.tblProviders.Add(providerName);

                    db.SaveChanges();

                    return Json(new { Result = "success", id = providerName.ProviderID, errors = "" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", id = pk, errors = "Unknown database error." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = mis)]
        public ActionResult DeleteProviderName(int providerId)
        {
            tblUser owner = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (owner != null)
            {
                tblProvider provider = db.tblProviders.Where(p => p.ProviderID == providerId).SingleOrDefault();
                if (provider != null)
                {
                    db.tblProviders.Remove(provider);
                    db.SaveChanges();

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "error", id = providerId, errors = "Unknown Provider Name." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", id = providerId, errors = "Unknown error." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult HasSchool(int year, int month, int day, bool hasSchool, string usd, string bId)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                tblCalendar calendar = db.tblCalendars.Where(c => c.UserID == MIS.UserID && c.Year == year && c.Month == month && c.Day == day && c.USD == usd && c.BuildingID == bId).FirstOrDefault();

                if (calendar == null)
                {
                    CopyCalendar(usd, bId, MIS);

                    calendar = db.tblCalendars.Where(c => c.UserID == MIS.UserID && c.Year == year && c.Month == month && c.Day == day && c.USD == usd && c.BuildingID == bId).FirstOrDefault();
                }

                if (!calendar.NoService)
                {
                    calendar.canHaveClass = hasSchool;
                    db.SaveChanges();

                    return Json(new { Result = "success", HasClass = hasSchool, Message = "successfully saved to the dababase." }, JsonRequestBehavior.AllowGet);
                }

                //calendar = db.tblCalendars.Where(c => c.UserID == MIS.UserID && c.Year == year && c.Month == month && c.Day == day && c.USD == usd && c.BuildingID == bId).FirstOrDefault();

                return Json(new { Result = "success", HasClass = false, Message = "successfully saved to the dababase." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }

        private void CopyCalendar(string usd, string bId, tblUser MIS)
        {
            using (SqlConnection SQLConn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndividualizedEducationProgramConnectionString"].ConnectionString))
            {
                if (SQLConn.State != ConnectionState.Open) { SQLConn.Open(); }

                String saveStuff = "INSERT INTO [tblCalendar] ([UserID], [USD], [BuildingID], [Year], [Month], [Day], [NoService], [canHaveClass]) SELECT @userID, @USD, @BuildingID, [Year], [Month], [Day], [NoService], [canHaveClass] FROM [dbo].[tblCalendarTemplate]";
                using (SqlCommand querySaveStuff = new SqlCommand(saveStuff))
                {
                    querySaveStuff.Connection = SQLConn;
                    querySaveStuff.Parameters.Clear();
                    querySaveStuff.Parameters.AddWithValue("@userID", MIS.UserID);
                    querySaveStuff.Parameters.AddWithValue("@USD", usd);
                    querySaveStuff.Parameters.AddWithValue("@BuildingID", bId);
                    querySaveStuff.ExecuteNonQuery();
                }

                String saveMoreStuff = "INSERT INTO [tblCalendarReporting] ([UserID], [USD], [BuildingID], [SchoolYear]) SELECT DISTINCT @userID, @USD, @BuildingID, SchoolYear FROM [dbo].[tblCalendarTemplate] ORDER BY SchoolYear";
                using (SqlCommand querySaveMoreStuff = new SqlCommand(saveMoreStuff))
                {
                    querySaveMoreStuff.Connection = SQLConn;
                    querySaveMoreStuff.Parameters.Clear();
                    querySaveMoreStuff.Parameters.AddWithValue("@userID", MIS.UserID);
                    querySaveMoreStuff.Parameters.AddWithValue("@USD", usd);
                    querySaveMoreStuff.Parameters.AddWithValue("@BuildingID", bId);
                    querySaveMoreStuff.ExecuteNonQuery();
                }
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetCalendarViewByBuilding(int SchoolYear, string usd, string bId)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                List<tblCalendar> CalendarView = db.tblCalendars.Where(c => c.USD == usd && c.BuildingID == bId && c.UserID == MIS.UserID && (c.NoService == true || (c.NoService == false && c.canHaveClass == false))).ToList();
                if (CalendarView != null && CalendarView.Count > 0)
                {
                    List<tblCalendarReporting> reports = db.tblCalendarReportings.Where(r => r.UserID == MIS.UserID && r.USD == usd && r.BuildingID == bId).ToList();
                    return Json(new { Result = "success", calendarEvents = CalendarView, calendarReports = reports, Message = "calendar exisit!" }, JsonRequestBehavior.AllowGet);
                }

                CopyCalendar(usd, bId, MIS);
                CalendarView = db.tblCalendars.Where(c => c.USD == usd && c.BuildingID == bId && c.UserID == MIS.UserID && (c.NoService == true || (c.NoService == false && c.canHaveClass == false))).ToList();

                return Json(new { Result = "success", calendarEvents = CalendarView, Message = "Calendar Created" }, JsonRequestBehavior.AllowGet);
            }

            // Unknow user or view.
            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CopyOverToCalendars(FormCollection collection)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                string district = collection["district"];
                string building = collection["building"];

                var selectedDistricts = collection["selectedDistrict[]"].Split(',');
                var selectedBuildings = collection["selectedBuilding[]"].Split(',');

                for (int i = 0; i < selectedDistricts.Length; i++)
                {
                    using (SqlConnection SQLConn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndividualizedEducationProgramConnectionString"].ConnectionString))
                    {
                        if (SQLConn.State != ConnectionState.Open) { SQLConn.Open(); }

                        String saveStuff = "UPDATE Cal_Upd SET Cal_Upd.[NoService] = Cal_Orig.[NoService], Cal_Upd.[canHaveClass] = Cal_Orig.[canHaveClass] FROM tblCalendar Cal_Upd JOIN tblCalendar Cal_Orig ON Cal_Orig.UserID = Cal_Upd.UserID AND Cal_Orig.calendarDate = Cal_Upd.calendarDate WHERE Cal_Orig.UserID = @UserID_Orig AND Cal_Orig.USD = @USD_Orig AND Cal_Orig.BuildingID = @BuildingID_Orig AND Cal_Upd.USD = @USD_Upd AND Cal_Upd.BuildingID = @BuildingID_Upd AND(Cal_Orig.canHaveClass != Cal_Upd.canHaveClass OR Cal_Orig.NoService != Cal_Upd.NoService)";
                        using (SqlCommand querySaveStuff = new SqlCommand(saveStuff))
                        {
                            querySaveStuff.Connection = SQLConn;
                            querySaveStuff.Parameters.Clear();
                            querySaveStuff.Parameters.AddWithValue("@UserID_Orig", MIS.UserID);
                            querySaveStuff.Parameters.AddWithValue("@USD_Orig", district);
                            querySaveStuff.Parameters.AddWithValue("@BuildingID_Orig", building);
                            querySaveStuff.Parameters.AddWithValue("@USD_Upd", selectedDistricts[i]);
                            querySaveStuff.Parameters.AddWithValue("@BuildingID_Upd", selectedBuildings[i]);
                            querySaveStuff.ExecuteNonQuery();
                        }

                        String saveMoreStuff = "UPDATE CalR_Upd SET CalR_Upd.DaysPerWeek = CalR_Orig.DaysPerWeek, CalR_Upd.TotalDays = CalR_Orig.TotalDays, CalR_Upd.TotalWeeks = CalR_Orig.TotalWeeks FROM tblCalendarReporting CalR_Upd JOIN tblCalendarReporting CalR_Orig ON CalR_Orig.UserID = CalR_Upd.UserID AND CalR_Orig.SchoolYear = CalR_Upd.SchoolYear WHERE CalR_Orig.UserID = @UserID_Orig AND CalR_Orig.USD = @USD_Orig AND CalR_Orig.BuildingID = @BuildingID_Orig AND CalR_Upd.USD = @USD_Upd AND CalR_Upd.BuildingID = @BuildingID_Upd";
                        using (SqlCommand querySaveMoreStuff = new SqlCommand(saveMoreStuff))
                        {
                            querySaveMoreStuff.Connection = SQLConn;
                            querySaveMoreStuff.Parameters.Clear();
                            querySaveMoreStuff.Parameters.AddWithValue("@UserID_Orig", MIS.UserID);
                            querySaveMoreStuff.Parameters.AddWithValue("@USD_Orig", district);
                            querySaveMoreStuff.Parameters.AddWithValue("@BuildingID_Orig", building);
                            querySaveMoreStuff.Parameters.AddWithValue("@USD_Upd", selectedDistricts[i]);
                            querySaveMoreStuff.Parameters.AddWithValue("@BuildingID_Upd", selectedBuildings[i]);
                            querySaveMoreStuff.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new { Result = "success", Message = "Calendars Copied" }, JsonRequestBehavior.AllowGet);
            }

            // Unknow user or view.
            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = mis)]
        public ActionResult SaveCalendarReports(int schoolYear, string usd, string building, int daysPerWeek, int totalDays, int totalWeeks)
        {

            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                tblCalendarReporting reports = db.tblCalendarReportings.Where(r => r.UserID == MIS.UserID && r.SchoolYear == schoolYear && r.USD == usd && r.BuildingID == building).FirstOrDefault();
                if (reports != null)
                {
                    reports.DaysPerWeek = daysPerWeek;
                    reports.TotalDays = totalDays;
                    reports.TotalWeeks = totalWeeks;
                }
                else
                {
                    reports = new tblCalendarReporting();

                    reports.UserID = MIS.UserID;
                    reports.USD = usd;
                    reports.BuildingID = building;
                    reports.SchoolYear = schoolYear;
                    reports.DaysPerWeek = daysPerWeek;
                    reports.TotalDays = totalDays;
                    reports.TotalWeeks = totalWeeks;

                    db.tblCalendarReportings.Add(reports);
                }

                db.SaveChanges();

                // Unknow user or view.
                return Json(new { Result = "success", Message = "Data has successfully been saved." }, JsonRequestBehavior.AllowGet);
            }

            // Unknow user or view.
            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = admin)]
        public ActionResult AdminPortal(int? userId)
        {
            tblUser admin = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (admin != null)
            {
                // get all the admins in the database that are active and under this MIS user.
                var teachers = (from org in db.tblOrganizationMappings
                                join user in db.tblUsers
                                    on org.UserID equals user.UserID
                                where (org.AdminID == admin.UserID) && !(user.Archive ?? false)
                                select user).Distinct().OrderBy(u => u.RoleID).ToList();

                UserOrganizationViewModel model = new UserOrganizationViewModel();
                model.user = admin;
                model.staff = teachers.ToList();

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = teacher)]
        public ActionResult TeacherPortal(int? userId, bool hasSeenAgreement = false)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (teacher != null)
            {
                var users = (from u in db.tblUsers
                             join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                             where o.AdminID == teacher.UserID
                             select new Student()
                             {
                                 UserID = u.UserID,
                                 FirstName = u.FirstName,
                                 MiddleName = u.MiddleName,
                                 LastName = u.LastName,
                                 City = u.City,
                                 State = u.State,
                                 Email = u.Email,
                                 Password = u.Password,
                                 ImageURL = u.ImageURL,
                             }).Distinct().ToList();

                var info = (from i in db.tblStudentInfoes
                            join o in db.tblOrganizationMappings on i.UserID equals o.UserID
                            where o.AdminID == teacher.UserID
                            select i).Distinct().ToList();

                var students = (from user in users
                                join i in info
                                on user.UserID equals i.UserID
                                where !(user.Archive ?? false)
                                select new Student()
                                {
                                    UserID = user.UserID,
                                    FirstName = user.FirstName,
                                    MiddleName = user.MiddleName,
                                    LastName = user.LastName,
                                    City = user.City,
                                    State = user.State,
                                    Email = user.Email,
                                    Password = user.Password,
                                    USD = user.USD,
                                    BuildingID = user.BuildingID,
                                    ImageURL = user.ImageURL,
                                    KidsID = i.KIDSID,
                                    DateOfBirth = i.DateOfBirth
                                }).Distinct().ToList();


                var model = new StudentViewModel();
                model.Teacher = teacher;
                model.Students = students.ToList();

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetOrganization(string id)
        {
            int userId = Convert.ToInt32(id);
            tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == userId);

            if (user != null)
            {
                UserOrganizationViewModel viewModel = new UserOrganizationViewModel();

                var query = (from u in db.tblUsers
                             join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                             where o.AdminID == userId
                             select u).Distinct().OrderBy(u => u.RoleID).ToList();

                viewModel.user = user;
                viewModel.staff = query;

                if (user.RoleID == mis)
                {
                    ViewBag.Icon = "fa fa-cog";
                    ViewBag.SubIcon = "fa fa-user-o";
                    ViewBag.IndentClass = "indentAdmin";
                }
                else if (user.RoleID == admin)
                {
                    ViewBag.Icon = "fa fa-user-o";
                    ViewBag.SubIcon = "fa fa-graduation-cap";
                    ViewBag.IndentClass = "indentTeacher";
                }
                else if (user.RoleID == teacher)
                {
                    ViewBag.Icon = "fa fa-graduation-cap";
                    ViewBag.SubIcon = "fa fa-child";
                    ViewBag.IndentClass = "indentStudent";
                }

                return PartialView("_TreeHierarchyView", viewModel);
            }

            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult LoadModuleSection(int studentId, string view)
        {
            var iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();

            try
            {
                switch (view)
                {
                    case "HealthModule":
                        var healthModel = new tblIEPHealth();
                        if (iep != null)
                        {
                            healthModel = db.tblIEPHealths.Where(h => h.IEPid == iep.IEPHealthID).FirstOrDefault();
                        }

                        return PartialView("_ModuleHealthSection", healthModel);
                    case "AcademicModule":
                        var academicModel = new ModuleAcademicViewModel();
                        if (iep != null)
                        {
                            academicModel.Academic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == iep.IEPAcademicID).FirstOrDefault();
                            academicModel.Reading = db.tblIEPReadings.Where(r => r.IEPReadingID == iep.IEPReadingID).FirstOrDefault();
                            academicModel.Math = db.tblIEPMaths.Where(m => m.IEPMathID == iep.IEPMathID).FirstOrDefault();
                            academicModel.Written = db.tblIEPWrittens.Where(w => w.IEPWrittenID == iep.IEPWrittenID).FirstOrDefault();
                        }

                        return PartialView("_ModuleAcademicSection", academicModel);
                    case "MotorModule":
                        var motorModel = new tblIEPMotor();
                        if (iep != null)
                        {
                            motorModel = db.tblIEPMotors.Where(m => m.IEPid == iep.IEPMotorID).FirstOrDefault();
                        }

                        return PartialView("_ModuleMotorSection", motorModel);
                    case "CommunicationModule":
                        var communicationModel = new tblIEPCommunication();
                        if (iep != null)
                        {
                            communicationModel = db.tblIEPCommunications.Where(c => c.IEPid == iep.IEPCommunicationID).FirstOrDefault();
                        }

                        return PartialView("_ModuleCommunicationSection", communicationModel);
                    case "SocialModule":
                        var socialModel = new tblIEPSocial();
                        if (iep != null)
                        {
                            socialModel = db.tblIEPSocials.Where(s => s.IEPSocialID == iep.IEPSocialID).FirstOrDefault();
                        }

                        return PartialView("_ModuleSocialSection", socialModel);
                    case "GeneralIntelligenceModule":
                        var intelligenceModel = new tblIEPIntelligence();
                        if (iep != null)
                        {
                            intelligenceModel = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == iep.IEPIntelligenceID).FirstOrDefault();
                        }

                        return PartialView("_ModuleGeneralIntelligenceSection", intelligenceModel);
                    default:
                        return Json(new { Result = "error", Message = "Unknown View" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult AvailableTeachers(int id)
        {
            try
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                tblOrganizationMapping boss = db.tblOrganizationMappings.Where(u => u.UserID == submitter.UserID).Distinct().FirstOrDefault();

                var teachers = (from org in db.tblOrganizationMappings
                                join user in db.tblUsers
                                    on org.UserID equals user.UserID
                                where (org.AdminID == boss.AdminID) && (user.RoleID == "4") && !(user.Archive ?? false)
                                select user).Distinct();

                return PartialView("_AvailableTeachers", teachers);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddTeachers(int[] teacherIds)
        {
            try
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                tblOrganizationMapping boss = db.tblOrganizationMappings.Where(u => u.UserID == submitter.UserID).Distinct().FirstOrDefault();

                foreach (int teacherId in teacherIds)
                {
                    List<tblOrganizationMapping> oldRelations = db.tblOrganizationMappings.Where(u => u.AdminID == boss.AdminID && u.UserID == teacherId).ToList();
                    db.tblOrganizationMappings.RemoveRange(oldRelations);
                    db.SaveChanges();

                    oldRelations.Select(o => { o.AdminID = submitter.UserID; return o; }).ToList();

                    db.tblOrganizationMappings.AddRange(oldRelations);
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
        [Authorize]
        public ActionResult TeacherStudentsRole(int id)
        {
            StudentViewModel model = new StudentViewModel();
            tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();

            if (user != null)
            {
                model.Teacher = user;
                model.Students = (from o in db.tblOrganizationMappings
                                  join u in db.tblUsers on o.UserID equals u.UserID
                                  where o.AdminID == id && u.RoleID == "5" && !(u.Archive ?? false)
                                  select new Student
                                  {
                                      UserID = u.UserID,
                                      FirstName = u.FirstName,
                                      MiddleName = u.MiddleName,
                                      LastName = u.LastName,
                                      City = u.City,
                                      State = u.State,
                                      Email = u.Email,
                                      Password = u.Password,
                                      RoleID = u.RoleID,
                                      ImageURL = u.ImageURL,
                                      Archive = u.Archive,
                                  }).ToList();
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult StudentProcedures(int stid)
        {
            StudentProcedureViewModel model = new StudentProcedureViewModel();

            tblUser student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();

            if (student != null)
            {
                model.student = student;
                IEP theIEP = new IEP(student.UserID);

                if (theIEP.draft != null)
                {
                    model.hasplan = true;
                    model.studentIEP = theIEP;
                    model.studentPlan = new StudentPlan(student.UserID);
                }
                else
                {
                    model.hasplan = false;
                    model.studentIEP = theIEP.CreateNewIEP(stid);
                    model.studentPlan = new StudentPlan();
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdateIEPDates(int stId, string iepDate, bool start = true)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == stId).FirstOrDefault();

            if (iep != null)
            {
                if (start)
                {
                    iep.begin_date = Convert.ToDateTime(iepDate);
                }
                else
                {
                    iep.end_date = Convert.ToDateTime(iepDate);
                }

                db.SaveChanges();

                return Json(new { Result = "success", Message = "IEP Beginning Date was updated" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Error saving to the database." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult AgreementPrint(int id)
        {
            tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (user != null)
            {
                user.Agreement = true;
            }

            db.SaveChanges();

            return RedirectToAction("StudentProcedures", new { stid = id });
        }

        [HttpGet]
        [Authorize]
        public ActionResult StudentGoals(int studentId)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                StudentGoalsViewModel model = new StudentGoalsViewModel();
                model.studentId = studentId;
                model.iepId = iep.IEPid;

                List<tblGoal> goals = db.tblGoals.Where(g => g.IEPid == iep.IEPid).ToList();
                foreach (tblGoal goal in goals)
                {

                    StudentGoal test = new StudentGoal(goal.goalID);

                    model.studentGoals.Add(test);
                }

                return PartialView("_ModuleStudentGoals", model);
            }

            //return RedirectToAction("StudentProcedures", new { stid = studentId });
            return PartialView("_ModuleStudentGoals", new StudentGoalsViewModel());
        }

        [HttpGet]
        [Authorize(Roles = teacher)]
        public ActionResult StudentServices(int studentId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            StudentServiceViewModel model = new StudentServiceViewModel();

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                List<tblService> services = db.tblServices.Where(s => s.IEPid == iep.IEPid).ToList();

                // Get the MIS id of the logged in teacher.
                tblOrganizationMapping admin = db.tblOrganizationMappings.Where(o => o.UserID == teacher.UserID).First();
                tblOrganizationMapping mis = db.tblOrganizationMappings.Where(o => o.UserID == admin.AdminID).First();

                if (services != null)
                {
                    model.studentId = studentId;
                    model.studentService = services;
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.AdminID).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = db.tblCalendars.Where(c => c.UserID == mis.AdminID && c.NoService == true && c.canHaveClass == false && c.Year >= DateTime.Now.Year).ToList();
                }
                else
                {
                    model.studentId = studentId;
                    model.studentService.Add(new tblService() { IEPid = iep.IEPid });
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.AdminID).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = db.tblCalendars.Where(c => c.UserID == mis.UserID && c.NoService == true && c.canHaveClass == false && c.Year >= DateTime.Now.Year).ToList();
                }

                return PartialView("_ModuleStudentServices", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [HttpPost]
        [Authorize(Roles = teacher)]
        public ActionResult SaveStudentService(FormCollection collection)
        {
            int StudentSerivceId = Convert.ToInt32(collection["StudentSerivceId"]);
            int studentId = Convert.ToInt32(collection["StudentId"]);

            DateTime temp;
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if(iep != null)
            {
                if (StudentSerivceId == 0) // new service
                {
                    tblService service = new tblService();
                    service.IEPid = iep.IEPid;
                    service.SchoolYear = Convert.ToInt32(collection["fiscalYear"]);
                    service.StartDate = DateTime.TryParse((collection["serviceStartDate"]), out temp) ? temp : DateTime.Now;
                    service.EndDate = DateTime.TryParse((collection["serviceEndDate"]), out temp) ? temp : DateTime.Now;
                    service.ServiceCode = collection["ServiceType"].ToString();
                    service.Frequency = Convert.ToInt32(collection["Frequency"]);
                    service.DaysPerWeek = Convert.ToByte(collection["serviceDaysPerWeek"]);
                    service.Minutes = Convert.ToInt16(collection["serviceMinutesPerDay"]);
                    service.ProviderID = Convert.ToInt32(collection["serviceProvider"]);
                    service.LocationCode = collection["location"];
                    service.Create_Date = DateTime.Now;
                    service.Update_Date = DateTime.Now;
          
                    for (int i = 11; i < collection.Count; i++)
                    {
                        int goalId = Convert.ToInt32(collection[i]);
                        tblGoal goal = db.tblGoals.Where(g => g.goalID == goalId).First();
                        service.tblGoals.Add(goal);
                    }

                    db.tblServices.Add(service);
                }
                else // exsisting service
                {
                    tblService service = db.tblServices.Where(s => s.ServiceID == StudentSerivceId).FirstOrDefault();
                    service.SchoolYear = Convert.ToInt32(collection["fiscalYear"]);
                    service.StartDate = DateTime.TryParse((collection["serviceStartDate"]), out temp) ? temp : DateTime.Now;
                    service.EndDate = DateTime.TryParse((collection["serviceEndDate"]), out temp) ? temp : DateTime.Now;
                    service.ServiceCode = collection["ServiceType"].ToString();
                    service.Frequency = Convert.ToInt32(collection["Frequency"]);
                    service.DaysPerWeek = Convert.ToByte(collection["serviceDaysPerWeek"]);
                    service.Minutes = Convert.ToInt16(collection["serviceMinutesPerDay"]);
                    service.ProviderID = Convert.ToInt32(collection["serviceProvider"]);
                    service.LocationCode = collection["location"];
                    service.Update_Date = DateTime.Now;
                    service.tblGoals.Clear();

                    for (int i = 11; i < collection.Count; i++)
                    {
                        int goalId = Convert.ToInt32(collection[i]);
                        tblGoal goal = db.tblGoals.Where(g => g.goalID == goalId).First();
                        service.tblGoals.Add(goal);
                    }
                }

                //save the service
                db.SaveChanges();
            }

            //return Json Dummie.
            return Json(new { Result = "success", Message = "The service has been saved." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteStudentService(int studentServiceId)
        {
            tblService service = db.tblServices.Where(s => s.ServiceID == studentServiceId).FirstOrDefault();
            if(service != null)
            {
                db.tblServices.Remove(service);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "The Service has been delete." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Unknown Error Occured." }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = teacher)]
        public ActionResult Accommodations(int studentId)
        {
            var model = new AccomodationViewModel();
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();

            if (iep != null)
            {
                model.StudentId = studentId;
                model.IEPid = iep.IEPid;
                var accommodations = db.tblAccommodations.Where(i => i.IEPid == iep.IEPid);
                if (accommodations.Any())
                    model.AccomList = accommodations.OrderBy(o => o.AccomType).ToList();

                var locations = db.tblLocations.Where(o => o.Active == true);

                if (locations.Any())
                {
                    foreach (var loc in locations)
                    {
                        model.Locations.Add(new SelectListItem() { Text = loc.Name, Value = loc.LocationCode });
                    }
                }
            }

            return PartialView("_ModuleAccommodations", model);
        }

        [HttpPost]
        [Authorize(Roles = teacher)]
        public ActionResult DeleteAccommodation(int accomId)
        {
            try
            {
                var accomodation = db.tblAccommodations.FirstOrDefault(o => o.AccommodationID == accomId);
                if (accomodation != null)
                {
                    db.tblAccommodations.Remove(accomodation);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = false, error = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult StudentPlanning(FormCollection collection)
        {
            var studentId = Convert.ToInt32(collection["student.UserID"]);

            StudentPlan thePlan = new StudentPlan();

            // reset all the no concern flags
            thePlan.AcademicNoConcern = false;
            thePlan.CommunicationNoConcern = false;
            thePlan.HealthNoConcern = false;
            thePlan.MathNoConcern = false;
            thePlan.MotorNoConcern = false;
            thePlan.ReadingNoConcern = false;
            thePlan.SocialNoConcern = false;
            thePlan.WrittenNoConcern = false;

            if (thePlan != null)
            {
                int intValue;
                DateTime dateTimeValue;
                foreach (var key in collection.AllKeys.Skip(2))
                {
                    var value = collection[key];

                    if (value == "on")
                        thePlan[key] = true;
                    else if (DateTime.TryParse(value, out dateTimeValue))
                        thePlan[key] = dateTimeValue;
                    else if (int.TryParse(value, out intValue))
                        thePlan[key] = intValue;
                    else
                        thePlan[key] = (value == "1");
                }

                thePlan.Update(studentId);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [HttpGet]
        [Authorize]
        public ActionResult RequestConsent(int id)
        {
            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            tblUser teacher = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault(); //== tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            StudentLegalView viewModel = new StudentLegalView()
            {
                student = student,
                teacher = teacher,
                studentInfo = db.tblStudentInfoes.Where(u => u.UserID == student.UserID).FirstOrDefault(),
                contacts = db.tblStudentRelationships.Where(u => u.UserID == student.UserID).ToList(),
            };

            return View("RequestConsent", viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult NoticeOfMeeting(int id)
        {
            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            tblUser teacher = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault(); //== tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            StudentLegalView viewModel = new StudentLegalView()
            {
                student = student,
                teacher = teacher,
                studentInfo = db.tblStudentInfoes.Where(u => u.UserID == student.UserID).FirstOrDefault(),
                contacts = db.tblStudentRelationships.Where(u => u.UserID == student.UserID).ToList(),
            };

            return View("NoticeOfMeeting", viewModel);
        }

        [HttpGet]
        public ActionResult PrintIEP(int id, int status = 0)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(u => u.UserID == id);

            // check the passed value and change the status based on value.
            string iepStatus = IEPStatus.DRAFT;

            var query = (from iep in db.tblIEPs
                         join health in db.tblIEPHealths
                             on iep.IEPHealthID equals health.IEPHealthID
                         join motor in db.tblIEPMotors
                             on iep.IEPMotorID equals motor.IEPMotorID
                         join communication in db.tblIEPCommunications
                             on iep.IEPCommunicationID equals communication.IEPCommunicationID
                         join social in db.tblIEPSocials
                             on iep.IEPSocialID equals social.IEPSocialID
                         join intelligence in db.tblIEPIntelligences
                             on iep.IEPIntelligenceID equals intelligence.IEPIntelligenceID
                         join academics in db.tblIEPAcademics
                             on iep.IEPAcademicID equals academics.IEPAcademicID
                         join reading in db.tblIEPReadings
                             on iep.IEPReadingID equals reading.IEPReadingID
                         join math in db.tblIEPMaths
                             on iep.IEPMathID equals math.IEPMathID
                         join written in db.tblIEPWrittens
                             on iep.IEPWrittenID equals written.IEPWrittenID
                         where iep.UserID == student.UserID && iep.IepStatus == iepStatus
                         select new { iep, health, motor, communication, social, intelligence, academics, reading, math, written }).ToList();

            if (query.Count() == 1)
            {
                IEP theIEP = new IEP()
                {
                    draft = query.SingleOrDefault().iep,
                    studentHealth = query.SingleOrDefault().health,
                    studentMotor = query.SingleOrDefault().motor,
                    studentCommunication = query.SingleOrDefault().communication,
                    studentSocial = query.SingleOrDefault().social,
                    studentIntelligence = query.SingleOrDefault().intelligence,
                    studentAcademic = query.SingleOrDefault().academics,
                    studentReading = query.SingleOrDefault().reading,
                    studentMath = query.SingleOrDefault().math,
                    studentWritten = query.SingleOrDefault().written
                };

                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }


        public ActionResult EditStudentInformation()
        {
            return View();
        }

        public ActionResult EditTeamStatements()
        {
            return View();
        }

        public ActionResult EditStudentStrategy()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult MySettings()
        {
            return View();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Home");

            // OR set the result without redirection:
            //filterContext.Result = new ViewResult
            //{
            //    ViewName = "~/Views/Error/Index.cshtml"
            //};
        }

    }
}