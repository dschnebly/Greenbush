using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using HtmlAgilityPack;

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
                MISProviderViewModel model = new MISProviderViewModel();
                List<tblProvider> listOfProviders = new List<tblProvider>();
                tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);


                if (MIS != null)
                {
                    listOfProviders = db.tblProviders.Where(p => p.UserID == MIS.UserID).OrderBy(o => o.Name).ToList();

					var MISDistrictList = (from buildingMaps in db.tblBuildingMappings
										   join districts in db.tblDistricts
												on buildingMaps.USD equals districts.USD
										   where buildingMaps.UserID == MIS.UserID
										   select districts).Distinct().ToList();

					model.listOfProviders = listOfProviders;
                    model.districts = MISDistrictList;
				}

				return PartialView("_ModuleServiceProviders", model);
            }

            // Unknow user or view.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpPost]
        [Authorize(Roles = mis)]
        public ActionResult UpdateProvidersList(int pk, string providerName, int[] providerDistrict, string providerCode)
        {
            tblUser owner = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            
            if (owner != null)
            {
                tblProvider provider = db.tblProviders.Where(p => p.ProviderID == pk).SingleOrDefault();
                if (provider != null)
                {

                    provider.Name = providerName;
                    provider.ProviderCode = providerCode;

                    db.SaveChanges();

                    foreach (var existingPD in provider.tblProviderDistricts.ToList())
                        db.tblProviderDistricts.Remove(existingPD);

                    db.SaveChanges();

                    foreach (var district in providerDistrict)
                    {
                        tblProviderDistrict pd = new tblProviderDistrict();
                        pd.ProviderID = provider.ProviderID;
                        pd.USD = district.ToString();

                        db.tblProviderDistricts.Add(pd);
                        db.SaveChanges();
                    }
                    
                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, Name = o.Name });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.Name) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tblProvider newProvider = new tblProvider();
                    newProvider.Name = providerName;
                    newProvider.ProviderCode = providerCode;
                    newProvider.UserID = owner.UserID;

                    

                    //can't have duplicate provider code
                    tblProvider dup = db.tblProviders.Where(p => p.ProviderCode == providerCode).SingleOrDefault();

                    if (dup == null)
                    {
                        db.tblProviders.Add(newProvider);
                        db.SaveChanges();

                        int newProvderId = newProvider.ProviderID;

                        //add to tblProviderDistricts
                        if (newProvderId > 0)
                        {
                            foreach (var district in providerDistrict)
                            {
                                tblProviderDistrict pd = new tblProviderDistrict();
                                pd.ProviderID = newProvderId;
                                pd.USD = district.ToString();

                                db.tblProviderDistricts.Add(pd);
                                db.SaveChanges();
                            }

                        }
                        
                    }
                    else
                    {
                        return Json(new { Result = "error", id = pk, errors = "Provider code already exists"}, JsonRequestBehavior.AllowGet);
                    }

                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, Name = o.Name });

                    return Json(new { Result = "success", id = newProvider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.Name) }, JsonRequestBehavior.AllowGet);
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
                    foreach (var existingPD in provider.tblProviderDistricts.ToList())
                        db.tblProviderDistricts.Remove(existingPD);
                    
                    db.tblProviders.Remove(provider);
                    db.SaveChanges();

                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, Name = o.Name });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.Name) }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "error", id = providerId, errors = "Unknown Provider Name." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", id = providerId, errors = "Unknown error." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetProviderDistrict(int providerId)
        {
            var districts = db.tblProviderDistricts.Where(o => o.ProviderID == providerId).Select(o => o.USD).ToList();

            return Json(new { Result = "success", districts = districts }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveCalendarReports(int schoolYear, string usd, string building, int daysPerWeek, int totalDays, int totalWeeks, int minutesPerDay)
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
                    reports.MinutesPerDay = minutesPerDay;
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
                    reports.MinutesPerDay = minutesPerDay;

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
                                 Archive = u.Archive,
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
                        tblIEPHealth healthModel = db.tblIEPHealths.Where(h => h.IEPid == iep.IEPHealthID).FirstOrDefault();
                        if (healthModel == null)
                        {
                            healthModel = new tblIEPHealth();
                        }

                        return PartialView("_ModuleHealthSection", (tblIEPHealth)healthModel);
                    case "AcademicModule":
                        ModuleAcademicViewModel academicModel = new ModuleAcademicViewModel();
                        academicModel.Academic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == iep.IEPAcademicID).FirstOrDefault();
                        academicModel.Reading = db.tblIEPReadings.Where(r => r.IEPReadingID == iep.IEPReadingID).FirstOrDefault();
                        academicModel.Math = db.tblIEPMaths.Where(m => m.IEPMathID == iep.IEPMathID).FirstOrDefault();
                        academicModel.Written = db.tblIEPWrittens.Where(w => w.IEPWrittenID == iep.IEPWrittenID).FirstOrDefault();

                        if (academicModel.Academic == null) { academicModel.Academic = new tblIEPAcademic(); }
                        if (academicModel.Reading == null) { academicModel.Reading = new tblIEPReading(); }
                        if (academicModel.Math == null) { academicModel.Math = new tblIEPMath(); }
                        if (academicModel.Written == null) { academicModel.Written = new tblIEPWritten(); }

                        return PartialView("_ModuleAcademicSection", academicModel);
                    case "MotorModule":
                        tblIEPMotor motorModel = db.tblIEPMotors.Where(m => m.IEPid == iep.IEPMotorID).FirstOrDefault();
                        if (motorModel == null)
                        {
                            motorModel = new tblIEPMotor();
                        }

                        return PartialView("_ModuleMotorSection", motorModel);
                    case "CommunicationModule":
                        tblIEPCommunication communicationModel = db.tblIEPCommunications.Where(c => c.IEPid == iep.IEPCommunicationID).FirstOrDefault();
                        if (communicationModel == null)
                        {
                            communicationModel = new tblIEPCommunication();
                        }

                        return PartialView("_ModuleCommunicationSection", communicationModel);
                    case "SocialModule":
                        tblIEPSocial socialModel = db.tblIEPSocials.Where(s => s.IEPSocialID == iep.IEPSocialID).FirstOrDefault();
                        if (socialModel == null)
                        {
                            socialModel = new tblIEPSocial();
                        }

                        return PartialView("_ModuleSocialSection", socialModel);
                    case "GeneralIntelligenceModule":
                        tblIEPIntelligence intelligenceModel = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == iep.IEPIntelligenceID).FirstOrDefault();
                        if (intelligenceModel == null)
                        {
                            intelligenceModel = new tblIEPIntelligence();
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
            tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();
            tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
            tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();
            bool enableAccommodations = false;
            bool enableBehaviorPlan = false;

            if (student != null)
            {
                model.student = student;
                model.birthDate = info.DateOfBirth;
                model.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
                model.isDoc = district.DOC;

                IEP theIEP = new IEP(student.UserID);

                if (theIEP.draft != null)
                {
                    model.hasplan = true;
                    model.studentIEP = theIEP;
                    model.studentPlan = new StudentPlan(student.UserID);

                    //check if any module has accommodations checked or behavior plan
                    if (db.tblIEPAcademics.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPCommunications.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPHealths.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPIntelligences.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPMotors.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPReadings.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPSocials.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPSocials.Where(o => o.IEPid == theIEP.draft.IEPid && o.BehaviorInterventionPlan).Any())
                        enableBehaviorPlan = true;
                    
                    if (db.tblIEPWrittens.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;

                    if (db.tblIEPMaths.Where(o => o.IEPid == theIEP.draft.IEPid && (o.NeedMetByAccommodation ?? true)).Any())
                        enableAccommodations = true;


                    model.hasAccommodations = enableAccommodations;
                    model.needsBehaviorPlan = enableBehaviorPlan;
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
        public ActionResult UpdateIEPDates(int stId, string IEPStartDate, string IEPMeetingDate)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == stId).FirstOrDefault();

            if (iep != null)
            {
                DateTime startDate;
                DateTime meetingDate;
                if (DateTime.TryParseExact(IEPStartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                {
                    if (DateTime.TryParseExact(IEPMeetingDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out meetingDate))
                    {
                        iep.begin_date = startDate;
                        iep.MeetingDate = meetingDate;

                        db.SaveChanges();
                    }

                    return Json(new { Result = "success", Message = "IEP dates were updated" }, JsonRequestBehavior.AllowGet);
                }
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
                    model.studentGoals.Add(new StudentGoal(goal.goalID));
                }

                return PartialView("_ModuleStudentGoals", model);
            }

            return PartialView("_ModuleStudentGoals", new StudentGoalsViewModel());
        }

        [HttpGet]
        [Authorize(Roles = teacher)]
        public ActionResult StudentServices(int studentId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                StudentServiceViewModel model = new StudentServiceViewModel();

                List<tblService> services = db.tblServices.Where(s => s.IEPid == iep.IEPid).ToList();
                tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

                // Get the MIS id of the logged in teacher.
                tblOrganizationMapping admin = db.tblOrganizationMappings.Where(o => o.UserID == teacher.UserID).First();
                tblOrganizationMapping mis = db.tblOrganizationMappings.Where(o => o.UserID == admin.AdminID).First();

                if (services != null)
                {
                    model.studentId = studentId;
                    model.studentServices = services;
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.AdminID).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = db.tblCalendars.Where(c => c.UserID == mis.AdminID && c.BuildingID == studentInfo.BuildingID && c.canHaveClass == false || c.NoService == true && c.Year >= DateTime.Now.Year && c.Year <= DateTime.Now.Year + 5).ToList();
                    model.availableCalendarDays = db.tblCalendars.Where(c => c.UserID == mis.AdminID && c.BuildingID == studentInfo.BuildingID && (c.canHaveClass == true || c.NoService == false) && c.Year >= DateTime.Now.Year && c.Year <= DateTime.Now.Year + 5).ToList();
                    model.calendarReportings = db.tblCalendarReportings.Where(r => r.UserID == mis.AdminID && r.BuildingID == studentInfo.BuildingID && r.SchoolYear <= DateTime.Now.Year + 5).ToList();
                    model.IEPStartDate = iep.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                    

                    //model.IEPEndDate = iep.end_date ?? DateTime.Now;
                }
                else
                {
                    model.studentId = studentId;
                    model.studentServices.Add(new tblService() { IEPid = iep.IEPid });
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.AdminID).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = db.tblCalendars.Where(c => c.UserID == mis.AdminID && c.BuildingID == studentInfo.BuildingID && c.canHaveClass == false || c.NoService == true && c.Year >= DateTime.Now.Year && c.Year <= DateTime.Now.Year + 5).ToList();
                    model.availableCalendarDays = db.tblCalendars.Where(c => c.UserID == mis.AdminID && c.BuildingID == studentInfo.BuildingID && (c.canHaveClass == true || c.NoService == false) && c.Year >= DateTime.Now.Year && c.Year <= DateTime.Now.Year + 5).ToList();
                    model.calendarReportings = db.tblCalendarReportings.Where(r => r.UserID == mis.AdminID && r.BuildingID == studentInfo.BuildingID && r.SchoolYear <= DateTime.Now.Year + 5).ToList();
                    model.IEPStartDate = iep.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                    //model.IEPEndDate = iep.end_date ?? DateTime.Now;
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
            if (iep != null)
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

                    // nullable serviceId
                    service.ProviderID = service.ProviderID == -1 ? null : service.ProviderID ;

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

                    // nullable serviceId
                    service.ProviderID = service.ProviderID == -1 ? null : service.ProviderID;

                    string selectedGoals = collection["studentGoalsSelect"];

                    if (!string.IsNullOrEmpty(selectedGoals))
                    {
                        string[] goalsArr = selectedGoals.Split(',');
                        
                        for (int i = 0; i < goalsArr.Count(); i++)
                        {
                            int goalId = 0;
                            Int32.TryParse(goalsArr[i], out goalId);

                            if (goalId > 0)
                            {
                                tblGoal goal = db.tblGoals.Where(g => g.goalID == goalId).First();
                                service.tblGoals.Add(goal);
                            }
                        }
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
            if (service != null)
            {
                db.tblServices.Remove(service);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "The Service has been delete." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Unknown Error Occured." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = teacher)]
        public ActionResult StudentTransition(int studentId)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();

                tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
                tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

                StudentTransitionViewModel model = new StudentTransitionViewModel();
                model.studentId = studentId;
                model.student = student;
                model.isDOC = district.DOC;
                model.iepId = iep.IEPid;
                model.assessments = db.tblTransitionAssessments.Where(a => a.IEPid == iep.IEPid).ToList();
                model.services = db.tblTransitionServices.Where(s => s.IEPid == iep.IEPid).ToList();
                model.goals = db.tblTransitionGoals.Where(g => g.IEPid == iep.IEPid).ToList();
                model.transition = db.tblTransitions.Where(t => t.IEPid == iep.IEPid).FirstOrDefault() ?? new tblTransition();

                return PartialView("_ModuleStudentTransition", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize(Roles = teacher)]
        public ActionResult BehaviorPlan(int studentId)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            List<SelectListItem> locationList = new List<SelectListItem>();
            if (iep != null)
            {
                var model = GetBehaviorModel(studentId, iep.IEPid);
                //model.StudentId = studentId;
                //model.IEPid = iep.IEPid;

                //tblBehavior BehaviorIEP = db.tblBehaviors.Where(c => c.IEPid == iep.IEPid).FirstOrDefault();
                //if (BehaviorIEP != null)
                //{
                //    model.BehaviorID = BehaviorIEP.BehaviorID;
                //    model.BehaviorConcern = BehaviorIEP.BehaviorConcern;
                //    model.StrengthMotivator = BehaviorIEP.StrengthMotivator;
                //    model.Crisis_Description = BehaviorIEP.Crisis_Description;
                //    model.Crisis_Escalation = BehaviorIEP.Crisis_Escalation;
                //    model.Crisis_Implementation = BehaviorIEP.Crisis_Implementation;
                //    model.Crisis_Other = BehaviorIEP.Crisis_Other;
                //    model.ReviewedBy = BehaviorIEP.ReviewedBy;
                //    model.SelectedTriggers = db.tblBehaviorTriggers.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorTriggerTypeID).ToList();
                //    var triggerOther = db.tblBehaviorTriggers.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                //    if (triggerOther != null)
                //        model.TriggerOther = triggerOther.OtherDescription;

                //    model.SelectedStrategies = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorStrategyTypeID).ToList();
                //    var stratOther = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                //    if (stratOther != null)
                //        model.StrategiesOther = stratOther.OtherDescription;

                //    model.SelectedHypothesis = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorHypothesisTypeID).ToList();
                //    var hypoOther = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                //    if (hypoOther != null)
                //        model.HypothesisOther = hypoOther.OtherDescription;

                //    var targetedBehaviors = db.tblBehaviorBaselines.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).ToList();
                //    if (targetedBehaviors.Any())
                //    {
                //        if (targetedBehaviors[0] != null)
                //            model.targetedBehavior1 = targetedBehaviors[0];
                //        if (targetedBehaviors[1] != null)
                //            model.targetedBehavior2 = targetedBehaviors[1];
                //        if (targetedBehaviors[2] != null)
                //            model.targetedBehavior3 = targetedBehaviors[2];
                //    }
                //}

                //model.Triggers = db.tblBehaviorTriggerTypes.ToList();
                //model.HypothesisList = db.tblBehaviorHypothesisTypes.ToList();
                //model.Strategies = db.tblBehaviorStrategyTypes.ToList();

                return PartialView("_ModuleBehavior", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize(Roles = teacher)]
        public ActionResult Accommodations(int studentId)
        {
            var model = new AccomodationViewModel();
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            List<SelectListItem> locationList = new List<SelectListItem>();
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
                        locationList.Add(new SelectListItem() { Text = loc.Name, Value = loc.LocationCode });
                    }

                    model.Locations = locationList;
                }
            }


            ViewBag.Locations = locationList;

            return PartialView("_ModuleAccommodations", model);
        }

        [Authorize(Roles = teacher)]
        public ActionResult OtherConsiderations(int studentId)
        {
            var model = new tblOtherConsideration();
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();

            if (iep != null)
            {
                
                model.IEPid = iep.IEPid;
                var oc = db.tblOtherConsiderations.Where(i => i.IEPid == iep.IEPid);
                if (oc.Any())
                    model = oc.FirstOrDefault();
                else
                {
                    //default value
                    model.DistrictAssessment_GradeNotAssessed = true;
                    model.StateAssessment_RequiredCompleted = true;
                    model.Transporation_Other_flag = true;
                    model.Transporation_Disability_flag = true;
                }
            }

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            string studentName = "";
            if (student != null)
                studentName = string.Format("{0}", student.FirstName);

            ViewBag.StudentName = studentName;
            ViewBag.StudentId = studentId;

            return PartialView("_ModuleOtherConsiderations", model);
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
            thePlan.RequireAssistiveTechnology = false;

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
        [Authorize(Roles = teacher)]
        public ActionResult IEPFormModule(int studentId)
        {

            IEPFormViewModel viewModel = new IEPFormViewModel();

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                viewModel.IEPForms = GetForms();
                viewModel.StudentId = studentId;
                viewModel.StudentName = string.Format("{0} {1}", !string.IsNullOrEmpty(student.FirstName) ? student.FirstName : "", !string.IsNullOrEmpty(student.LastName) ? student.LastName : "");
            }

            return PartialView("_IEPFormModule", viewModel);
        }

        [HttpGet]
        [Authorize(Roles = teacher)]
        public ActionResult IEPFormFile(int id, string fileName)
        {
            IEPFormFileViewModel viewModel = new IEPFormFileViewModel();
            viewModel.studentId = id;
            viewModel.fileName = fileName;

            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            tblUser teacher = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();


            StudentLegalView fileViewModel = new StudentLegalView()
            {
                student = student,
                teacher = teacher,
                studentInfo = db.tblStudentInfoes.Where(u => u.UserID == student.UserID).FirstOrDefault(),
                contacts = db.tblStudentRelationships.Where(u => u.UserID == student.UserID).ToList()
            };

            if (fileViewModel.studentInfo != null)
            {
                tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == fileViewModel.studentInfo.BuildingID).FirstOrDefault();
                fileViewModel.building = building != null ? building.BuildingName: "";
            }

            viewModel.fileModel = fileViewModel;

            return View("_IEPFormsFile", viewModel);
        }

        public ActionResult IEPForms(int stid)
        {
            IEPFormViewModel model = new IEPFormViewModel();

            tblUser student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();
            if (student != null)
            {
                model.IEPForms = GetForms();
                model.StudentId = stid;
                model.StudentName = string.Format("{0} {1}", !string.IsNullOrEmpty(student.FirstName) ? student.FirstName : "", !string.IsNullOrEmpty(student.LastName) ? student.LastName : "");
                
            }

            return View(model);
        }

        private List<SelectListItem> GetForms()
        {
            List<SelectListItem> forms = new List<SelectListItem>();

            forms.Add(new SelectListItem { Text = "Parents Rights-English", Value = "ParentsRights" });
            forms.Add(new SelectListItem { Text = "Notice Of Meeting", Value = "NoticeOfMeeting" });
            forms.Add(new SelectListItem { Text = "Prior Written Notice - Evaluation -English", Value = "RequestConsent" });
            forms.Add(new SelectListItem { Text = "Prior Written Notice - Identification", Value = "PriorWrittenNoticeId" });

            forms.Add(new SelectListItem { Text = "Revocation of Consent-Particular Services", Value = "RevPartSvscConsent" });
            forms.Add(new SelectListItem { Text = "Prior Written Notice-Revocation of Particular Services", Value = "RevPartSvscPWN" });
            forms.Add(new SelectListItem { Text = "Revocation of Consent-All Services", Value = "RevAllSvscConsent" });
            forms.Add(new SelectListItem { Text = "Prior Written Notice-Revocation of All Services", Value = "RevAllSvscPWN" });

            forms.Add(new SelectListItem { Text = "Sample Public Notice (Child Find)", Value = "SamplePublicNotice" });
            forms.Add(new SelectListItem { Text = "IEP Meeting-Consent to Invite Representative of Non-Educational Agency", Value = "IEPMtgConsent" });
            forms.Add(new SelectListItem { Text = "IEP Meeting-Excusal from Attendance Form", Value = "IEPMtgExcusal" });
            forms.Add(new SelectListItem { Text = "IEP Amendment Form", Value = "IEPAmendment" });
            forms.Add(new SelectListItem { Text = "Manifestation Determination Review Form", Value = "ManiDetermReview" });
            forms.Add(new SelectListItem { Text = "Summary of Performance Example", Value = "SOPExample" });
            forms.Add(new SelectListItem { Text = "IEP Team Considerations", Value = "IEPTeamConsider" });
            forms.Add(new SelectListItem { Text = "Parent Consent for Release of Information and Medicaid Reimbursement", Value = "ParentConsentMedicaid" });
            forms.Add(new SelectListItem { Text = "Physcian Script", Value = "PhysicianScript" });

            return forms.OrderBy(x => x.Text).ToList();
        }

        [HttpGet]
        public ActionResult PrintIEP(int id, int status = 0)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(u => u.UserID == id);


            // Get the MIS id of the logged in teacher.
            tblOrganizationMapping admin = db.tblOrganizationMappings.Where(o => o.UserID == teacher.UserID).First();
            tblOrganizationMapping mis = db.tblOrganizationMappings.Where(o => o.UserID == admin.AdminID).First();

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
                    studentWritten = query.SingleOrDefault().written,
                    locations = db.tblLocations.ToList(),
                    serviceTypes = db.tblServiceTypes.ToList(),
                    serviceProviders = db.tblProviders.Where(p => p.UserID == mis.AdminID).ToList(),
                    studentFirstName = string.Format("{0}", student.FirstName),
                    studentLastName = string.Format("{0}", student.LastName)



                };

                //student goalds
                if (theIEP != null && theIEP.draft != null)
                {
                    theIEP.studentGoals = db.tblGoals.Where(g => g.IEPid == theIEP.draft.IEPid).ToList();
                    foreach (var goal in theIEP.studentGoals)
                    { 
                        theIEP.studentGoalBenchmarks.AddRange( db.tblGoalBenchmarks.Where(g => g.goalID == goal.goalID).ToList());
                    }

                    theIEP.studentServices = db.tblServices.Where(g => g.IEPid == theIEP.draft.IEPid).ToList();
                    theIEP.accommodations = db.tblAccommodations.Where(g => g.IEPid == theIEP.draft.IEPid).ToList();
                    var studentBehavior = db.tblBehaviors.Where(g => g.IEPid == theIEP.draft.IEPid).FirstOrDefault();
                    theIEP.studentBehavior = GetBehaviorModel(student.UserID, theIEP.draft.IEPid);
                    theIEP.studentOtherConsiderations = db.tblOtherConsiderations.Where(o => o.IEPid == theIEP.draft.IEPid).FirstOrDefault();

                    StudentTransitionViewModel stvw = new StudentTransitionViewModel();
                    stvw.studentId = student.UserID;
                    stvw.student = student;
                    stvw.assessments = db.tblTransitionAssessments.Where(a => a.IEPid == theIEP.draft.IEPid).ToList();
                    stvw.services = db.tblTransitionServices.Where(s => s.IEPid == theIEP.draft.IEPid).ToList();
                    stvw.goals = db.tblTransitionGoals.Where(g => g.IEPid == theIEP.draft.IEPid).ToList();
                    stvw.transition = db.tblTransitions.Where(t => t.IEPid == theIEP.draft.IEPid).FirstOrDefault() ?? new tblTransition();

                    theIEP.studentTransition = stvw;
                    if (student != null)
                    {

                        tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();
                        tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
                        tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

                        theIEP.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);

                        stvw.isDOC = district.DOC;
                    }
                }

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

        [Authorize]
        [ValidateInput(false)]
        public FileResult DownloadPDF(FormCollection collection)
        {
            
            string HTMLContent = collection["printText"];
            string studentName = collection["studentName"];
            string studentId = collection["studentId"];

            if (!string.IsNullOrEmpty(HTMLContent))
            {
                int id = 0;
                Int32.TryParse(studentId, out id);
                
                tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
                if (user != null)
                {
                    user.Agreement = true;
                }

                db.SaveChanges();


                var cssText = @"<style>
                                .header{color:white;}
                                .input-group-addon, .transitionGoalLabel, .transitionServiceLabel {font-weight:600;}
                                .transitionServiceLabel, .underline{ text-decoration: underline;}
                                .transition-break{page-break-before:always;}
                                td { padding: 10px;}th{font-weight:600;}
                                table{width:600px;border-spacing: 10px;}
                                .module-page {font-size:11pt;}label{font-weight:800;}
                                h3{font-weight:600;font-size:14pt;width:100%;text-align:center;padding:15px;}
                                p{padding:10px}
                                .section-break{page-break-after:always;color:white;background-color:white}
                                .funkyradio{padding-bottom:15px;}
                                .radio-inline{font-weight:normal;}
                                .form-group{margin-top:5px;margin-bottom:5px}
                                .voffset{padding-bottom:8px;}
                                .form-check{margin-left:10px;}
                                .dont-break{margin-top:10px;page-break-inside: avoid;}
                                </style>";
                string result = System.Text.RegularExpressions.Regex.Replace(HTMLContent, @"\r\n?|\n", "");
                
                HtmlDocument doc = new HtmlDocument();
                doc.OptionWriteEmptyNodes = true;
                doc.OptionFixNestedTags = true;
                doc.LoadHtml(cssText + "<div class='module-page'>" + result + "</div>" );
                string cleanHTML = doc.DocumentNode.OuterHtml;

                using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssText)))
                {
                    using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cleanHTML)))
                    {
                        using (MemoryStream stream = new System.IO.MemoryStream())
                        {
                            using (MemoryStream stream2 = new System.IO.MemoryStream())
                            {       
                                    Document pdfDoc = new Document(PageSize.LETTER, 36, 36, 50, 50);
                                    

                                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                                    pdfDoc.Open();

                                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, htmlMemoryStream, cssMemoryStream);
                                    pdfDoc.Close();

                                    byte[] fileIn = stream.ToArray();
                                    var printFile = AddPageNumber(fileIn, studentName);

                                    return File(printFile, "application/pdf", "IEP.pdf");
                                
                            }
                        }
                    }
                }
            }

            return null;

        }

        byte[] AddPageNumber(byte[] fileIn, string studentName)
        {
            byte[] bytes = fileIn; 
            byte[] fileOut = null;
            Font blackFont = FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK);
            Font grayFont = FontFactory.GetFont("Arial", 75, Font.NORMAL, new BaseColor(245, 245, 245));
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(bytes);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    int pages = reader.NumberOfPages;
                    
                    for (int i = 1; i <= pages; i++)
                    {
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_CENTER, new Phrase("DRAFT", grayFont), 300f, 400f, 0);
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_LEFT, new Phrase(studentName, blackFont), 85f, 750f, 0);
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_LEFT, new Phrase(string.Format("{0}", DateTime.Now.ToShortDateString()), blackFont), 85f, 15f, 0);
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(string.Format("Page {0} of {1}",i.ToString(), pages.ToString()), blackFont), 568f, 15f, 0);
                    }
                }
                fileOut = stream.ToArray();
            }
            
            return fileOut;
        }

        protected override void OnException(ExceptionContext filterContext)
        {

            TempData["Error"] = filterContext.Exception.Message;
            filterContext.ExceptionHandled = true;

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Error");

            // OR set the result without redirection:
            //filterContext.Result = new ViewResult
            //{
              //  ViewName = "~/Views/Error/Index.cshtml"
                
            //};

            
        }
        
        private BehaviorViewModel GetBehaviorModel(int studentId, int iepId)
        {
            var model = new BehaviorViewModel();
            model.StudentId = studentId;
            model.IEPid = iepId;

            tblBehavior BehaviorIEP = db.tblBehaviors.Where(c => c.IEPid == iepId).FirstOrDefault();
            if (BehaviorIEP != null)
            {
                model.BehaviorID = BehaviorIEP.BehaviorID;
                model.BehaviorConcern = BehaviorIEP.BehaviorConcern;
                model.StrengthMotivator = BehaviorIEP.StrengthMotivator;
                model.Crisis_Description = BehaviorIEP.Crisis_Description;
                model.Crisis_Escalation = BehaviorIEP.Crisis_Escalation;
                model.Crisis_Implementation = BehaviorIEP.Crisis_Implementation;
                model.Crisis_Other = BehaviorIEP.Crisis_Other;
                model.ReviewedBy = BehaviorIEP.ReviewedBy;
                model.SelectedTriggers = db.tblBehaviorTriggers.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorTriggerTypeID).ToList();
                var triggerOther = db.tblBehaviorTriggers.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (triggerOther != null)
                    model.TriggerOther = triggerOther.OtherDescription;

                model.SelectedStrategies = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorStrategyTypeID).ToList();
                var stratOther = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (stratOther != null)
                    model.StrategiesOther = stratOther.OtherDescription;

                model.SelectedHypothesis = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorHypothesisTypeID).ToList();
                var hypoOther = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (hypoOther != null)
                    model.HypothesisOther = hypoOther.OtherDescription;

                var targetedBehaviors = db.tblBehaviorBaselines.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).ToList();
                if (targetedBehaviors.Any())
                {
                    if (targetedBehaviors[0] != null)
                        model.targetedBehavior1 = targetedBehaviors[0];
                    if (targetedBehaviors[1] != null)
                        model.targetedBehavior2 = targetedBehaviors[1];
                    if (targetedBehaviors[2] != null)
                        model.targetedBehavior3 = targetedBehaviors[2];
                }
            }

            model.Triggers = db.tblBehaviorTriggerTypes.ToList();
            model.HypothesisList = db.tblBehaviorHypothesisTypes.ToList();
            model.Strategies = db.tblBehaviorStrategyTypes.ToList();

            return model;
        }

    }
}