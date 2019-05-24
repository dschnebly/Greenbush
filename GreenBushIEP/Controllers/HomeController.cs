using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GreenbushIep.Controllers
{
    public class HomeController : Controller
    {
        private const string owner = "1"; //level 5
        private const string mis = "2"; //level 4
        private const string admin = "3"; //level 3
        private const string teacher = "4"; //level 2
        private const string student = "5";
        private const string nurse = "6"; //level 1

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
                else if (User.IsInRole(nurse))
                {
                    return RedirectToAction("NursePortal");
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = owner)]
        public ActionResult OwnerPortal()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            tblUser OWNER = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (OWNER != null)
            {
                PortalViewModel model = new PortalViewModel();
                model.user = OWNER;
                model.districts = (from district in db.tblDistricts select district).Distinct().ToList();
                model.buildings = (from building in db.tblBuildings select building).Distinct().ToList();
                model.members = (from user in db.tblUsers where user.RoleID != owner && user.Archive != true select new StudentIEPViewModel { UserID = user.UserID, FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, RoleID = user.RoleID }).Distinct().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(OWNER);

                return View("OwnerPortal", model);
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
                PortalViewModel model = new PortalViewModel();
                model.user = MIS;
                model.districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == MIS.UserID select district).Distinct().ToList();
                model.buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList();

                List<String> myDistricts = model.districts.Select(d => d.USD).ToList();
                List<String> myBuildings = model.buildings.Select(b => b.BuildingID).ToList();
                myBuildings.Add("0");
                model.members = (from buildingMap in db.tblBuildingMappings join user in db.tblUsers on buildingMap.UserID equals user.UserID where (user.RoleID == admin || user.RoleID == teacher || user.RoleID == student || user.RoleID == nurse) && ((user.Archive ?? false) != true) && (myDistricts.Contains(buildingMap.USD) && myBuildings.Contains(buildingMap.BuildingID)) select new StudentIEPViewModel() { UserID = user.UserID, FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, RoleID = user.RoleID }).Distinct().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

                foreach (var student in model.members.Where(m => m.RoleID == student))
                {
                    student.hasIEP = db.tblIEPs.Where(i => i.UserID == student.UserID && i.IsActive).Any();
                }

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(MIS);

                return View("MISPortal", model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = admin)]
        public ActionResult AdminPortal(int? userId)
        {

            tblUser ADMIN = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (ADMIN != null)
            {
                PortalViewModel model = new PortalViewModel();
                model.user = ADMIN;
                model.districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == ADMIN.UserID select district).Distinct().ToList();
                model.buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == ADMIN.UserID select building).Distinct().ToList();

                List<String> myDistricts = model.districts.Select(d => d.USD).ToList();
                List<String> myBuildings = model.buildings.Select(b => b.BuildingID).ToList();
                myBuildings.Add("0");
                model.members = (from buildingMap in db.tblBuildingMappings join user in db.tblUsers on buildingMap.UserID equals user.UserID where (user.RoleID == teacher || user.RoleID == student || user.RoleID == nurse) && ((user.Archive ?? false) != true) && (myDistricts.Contains(buildingMap.USD) && myBuildings.Contains(buildingMap.BuildingID)) select new StudentIEPViewModel() { UserID = user.UserID, FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, RoleID = user.RoleID }).Distinct().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

                foreach (var student in model.members.Where(m => m.RoleID == student))
                {
                    student.hasIEP = db.tblIEPs.Where(i => i.UserID == student.UserID && i.IsActive).Any();
                }

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(ADMIN);

                return View("AdminPortal", model);
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
                                 Password = null,
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
                                    DateOfBirth = i.DateOfBirth,
                                    CreatedBy = i.CreatedBy
                                }).Distinct().OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

                //get IEP Date
                foreach (var student in students)
                {
                    IEP theIEP = new IEP(student.UserID);
                    student.hasIEP = theIEP.current.IepStatus != IEPStatus.PLAN;
                    student.IEPDate = DateTime.Now.ToString("MM-dd-yyyy");
                    if (theIEP != null && theIEP.current != null && theIEP.current.begin_date.HasValue)
                        student.IEPDate = theIEP.current.begin_date.Value.ToShortDateString();

                }
                var model = new StudentViewModel();
                model.Teacher = teacher;
                model.Students = students.ToList();

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(teacher);

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = nurse)]
        public ActionResult NursePortal(int? userId)
        {
            tblUser nurse = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (nurse != null)
            {
                var users = (from u in db.tblUsers
                             join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                             where o.AdminID == nurse.UserID
                             select new Student()
                             {
                                 UserID = u.UserID,
                                 FirstName = u.FirstName,
                                 MiddleName = u.MiddleName,
                                 LastName = u.LastName,
                                 City = u.City,
                                 State = u.State,
                                 Email = u.Email,
                                 Password = null,
                                 ImageURL = u.ImageURL,
                                 Archive = u.Archive,
                             }).Distinct().ToList();

                var info = (from i in db.tblStudentInfoes
                            join o in db.tblOrganizationMappings on i.UserID equals o.UserID
                            where o.AdminID == nurse.UserID
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
                                    DateOfBirth = i.DateOfBirth,
                                    CreatedBy = i.CreatedBy
                                }).Distinct().OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

                //get IEP Date
                foreach (var student in students)
                {
                    IEP theIEP = new IEP(student.UserID);
                    student.hasIEP = theIEP.current.IepStatus != IEPStatus.PLAN;
                    student.IEPDate = DateTime.Now.ToString("MM-dd-yyyy");
                    if (theIEP != null && theIEP.current != null && theIEP.current.begin_date.HasValue)
                        student.IEPDate = theIEP.current.begin_date.Value.ToShortDateString();
                }

                var model = new StudentViewModel();
                model.Teacher = nurse;
                model.Students = students.ToList();

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize(Roles = mis)]
        public ActionResult LoadMISSection(string view)
        {

            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (view == "CalendarModule" && MIS != null)
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

            if (view == "ServiceProviderModule" && MIS != null)
            {
                MISProviderViewModel model = new MISProviderViewModel();

                var MISDistrictList = (from buildingMaps in db.tblBuildingMappings
                                       join districts in db.tblDistricts
                                            on buildingMaps.USD equals districts.USD
                                       where buildingMaps.UserID == MIS.UserID
                                       select districts).Distinct().ToList();

                List<string> listOfUSD = MISDistrictList.Select(d => d.USD).ToList();

                List<tblProvider> listOfProviders = new List<tblProvider>();
                listOfProviders = (from providers in db.tblProviders
                                   join districts in db.tblProviderDistricts
                                        on providers.ProviderID equals districts.ProviderID
                                   where listOfUSD.Contains(districts.USD)
                                   select providers).Distinct().ToList();

                model.listOfProviders = listOfProviders;
                model.districts = MISDistrictList;

                return PartialView("_ModuleServiceProviders", model);
            }

            if (view == "ServiceDistrictContactModule" && MIS != null)
            {

                MISDistricContactViewModel model = new MISDistricContactViewModel();
                model.myDistricts = (from buildingMaps in db.tblBuildingMappings join districts in db.tblDistricts on buildingMaps.USD equals districts.USD where buildingMaps.UserID == MIS.UserID select districts).Distinct().ToList();
                model.currentDistrict = model.myDistricts.FirstOrDefault();
                model.districtContact = (from contact in db.tblContacts where contact.Active == 1 && contact.USD == model.currentDistrict.USD select contact).FirstOrDefault();

                return PartialView("_ModuleDistrictContact", model);
            }

            // Unknow user or view.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpPost]
        [Authorize(Roles = mis)]
        public ActionResult UpdateProvidersList(int pk, string providerFirstName, string providerLastName, string[] providerDistrict, string providerCode)
        {
            tblUser owner = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (owner != null)
            {
                tblProvider provider = db.tblProviders.Where(p => p.ProviderID == pk).SingleOrDefault();
                if (provider != null)
                {

                    provider.FirstName = providerFirstName.ToString();
                    provider.LastName = providerLastName.ToString();
                    provider.ProviderCode = providerCode.ToString();

                    // blows away all the districts
                    foreach (var existingPD in provider.tblProviderDistricts.ToList())
                    {
                        db.tblProviderDistricts.Remove(existingPD);
                    }

                    db.SaveChanges();

                    foreach (var district in providerDistrict)
                    {
                        db.tblProviderDistricts.Add(new tblProviderDistrict() { ProviderID = provider.ProviderID, USD = district });
                        db.SaveChanges();
                    }

                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.LastName).ThenBy(o => o.FirstName) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tblProvider newProvider = new tblProvider();
                    newProvider.FirstName = providerFirstName.ToString();
                    newProvider.LastName = providerLastName.ToString();
                    newProvider.ProviderCode = providerCode.ToString();
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
                                db.tblProviderDistricts.Add(new tblProviderDistrict() { ProviderID = newProvderId, USD = district.ToString() });
                                db.SaveChanges();
                            }
                        }

                    }
                    else
                    {
                        return Json(new { Result = "error", id = pk, errors = "Provider code already exists" }, JsonRequestBehavior.AllowGet);
                    }

                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    return Json(new { Result = "success", id = newProvider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.LastName).ThenBy(o => o.FirstName) }, JsonRequestBehavior.AllowGet);
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

                    var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.LastName).ThenBy(o => o.FirstName) }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "error", id = providerId, errors = "Unknown Provider Name." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", id = providerId, errors = "Unknown error." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetProviderDistrict(int providerId)
        {
            var districts = db.tblProviderDistricts.Where(p => p.ProviderID == providerId).Select(p => p.USD).ToList();

            return Json(new { Result = "success", districts = districts }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult HasSchool(int year, int month, int day, bool hasSchool, string usd, string bId)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {

                tblCalendar calendar = db.tblCalendars.Where(c => c.Year == year && c.Month == month && c.Day == day && c.USD == usd && c.BuildingID == bId).FirstOrDefault();

                if (calendar == null)
                {
                    CopyCalendar(usd, bId, MIS);

                    calendar = db.tblCalendars.Where(c => c.Year == year && c.Month == month && c.Day == day && c.USD == usd && c.BuildingID == bId).FirstOrDefault();
                }

                if (!calendar.NoService)
                {
                    calendar.canHaveClass = hasSchool;
                    db.SaveChanges();

                    return Json(new { Result = "success", HasClass = hasSchool, Message = "successfully saved to the dababase." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "success", HasClass = false, Message = "successfully saved to the dababase." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "The user doesn't have permission to access a resource, or sufficient privilege to perform a task initiated by the user." }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        private void CopyCalendar(string usd, string bId, tblUser MIS)
        {
            using (SqlConnection SQLConn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndividualizedEducationProgramConnectionString"].ConnectionString))
            {
                if (SQLConn.State != ConnectionState.Open) { SQLConn.Open(); }

                String saveStuff = "INSERT INTO [tblCalendar] ([USD], [BuildingID], [Year], [Month], [Day], [NoService], [canHaveClass]) SELECT @USD, @BuildingID, [Year], [Month], [Day], [NoService], [canHaveClass] FROM [dbo].[tblCalendarTemplate]";
                using (SqlCommand querySaveStuff = new SqlCommand(saveStuff))
                {
                    querySaveStuff.Connection = SQLConn;
                    querySaveStuff.Parameters.Clear();
                    querySaveStuff.Parameters.AddWithValue("@USD", usd);
                    querySaveStuff.Parameters.AddWithValue("@BuildingID", bId);
                    querySaveStuff.ExecuteNonQuery();
                }

                String saveMoreStuff = "INSERT INTO [tblCalendarReporting] ([USD], [BuildingID], [SchoolYear]) SELECT DISTINCT @USD, @BuildingID, SchoolYear FROM [dbo].[tblCalendarTemplate] ORDER BY SchoolYear";
                using (SqlCommand querySaveMoreStuff = new SqlCommand(saveMoreStuff))
                {
                    querySaveMoreStuff.Connection = SQLConn;
                    querySaveMoreStuff.Parameters.Clear();
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
                List<tblCalendar> CalendarView = db.tblCalendars.Where(c => c.USD == usd && c.BuildingID == bId && c.SchoolYear == SchoolYear && (c.NoService == true || (c.NoService == false && c.canHaveClass == false))).OrderBy(o => o.Month).ToList();

                if (CalendarView != null && CalendarView.Count > 0)
                {
                    List<tblCalendarReporting> reports = db.tblCalendarReportings.Where(r => r.USD == usd && r.BuildingID == bId).ToList();
                    return Json(new { Result = "success", calendarEvents = CalendarView, calendarReports = reports, Message = "calendar exisit!" }, JsonRequestBehavior.AllowGet);
                }

                CopyCalendar(usd, bId, MIS);
                CalendarView = db.tblCalendars.Where(c => c.USD == usd && c.BuildingID == bId && (c.NoService == true || (c.NoService == false && c.canHaveClass == false))).ToList();

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

                        String saveStuff = @"UPDATE Cal_Upd 
											SET 
											  Cal_Upd.[NoService] = Cal_Orig.[NoService]
											, Cal_Upd.[canHaveClass] = Cal_Orig.[canHaveClass] 
											FROM tblCalendar Cal_Upd
											CROSS JOIN tblCalendar Cal_Orig
											WHERE Cal_Orig.calendarDate = Cal_Upd.calendarDate 
											AND Cal_Orig.USD = @USD_Orig 
											AND Cal_Orig.BuildingID = @BuildingID_Orig 
											AND Cal_Upd.USD = @USD_Upd 
											AND Cal_Upd.BuildingID = @BuildingID_Upd 
											AND(Cal_Orig.canHaveClass != Cal_Upd.canHaveClass OR Cal_Orig.NoService != Cal_Upd.NoService)";
                        using (SqlCommand querySaveStuff = new SqlCommand(saveStuff))
                        {
                            querySaveStuff.Connection = SQLConn;
                            querySaveStuff.Parameters.Clear();
                            querySaveStuff.CommandTimeout = 180;
                            querySaveStuff.Parameters.AddWithValue("@USD_Orig", district);
                            querySaveStuff.Parameters.AddWithValue("@BuildingID_Orig", building);
                            querySaveStuff.Parameters.AddWithValue("@USD_Upd", selectedDistricts[i]);
                            querySaveStuff.Parameters.AddWithValue("@BuildingID_Upd", selectedBuildings[i]);
                            querySaveStuff.ExecuteNonQuery();
                        }

                        String saveMoreStuff = @"UPDATE CalR_Upd 
											SET CalR_Upd.DaysPerWeek = CalR_Orig.DaysPerWeek
											, CalR_Upd.TotalDays = CalR_Orig.TotalDays
											, CalR_Upd.TotalWeeks = CalR_Orig.TotalWeeks
											FROM tblCalendarReporting CalR_Upd
											CROSS JOIN tblCalendarReporting CalR_Orig
											WHERE
											CalR_Orig.SchoolYear = CalR_Upd.SchoolYear
											AND CalR_Orig.USD = @USD_Orig
											AND CalR_Orig.BuildingID = @BuildingID_Orig
											AND CalR_Upd.USD = @USD_Upd AND CalR_Upd.BuildingID = @BuildingID_Upd";
                        using (SqlCommand querySaveMoreStuff = new SqlCommand(saveMoreStuff))
                        {
                            querySaveMoreStuff.Connection = SQLConn;
                            querySaveMoreStuff.Parameters.Clear();
                            querySaveMoreStuff.CommandTimeout = 180;
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
                tblCalendarReporting reports = db.tblCalendarReportings.Where(r => r.SchoolYear == schoolYear && r.USD == usd && r.BuildingID == building).FirstOrDefault();
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
                             where o.AdminID == userId && !u.Archive.HasValue
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
        public ActionResult LoadModuleSection(int studentId, int iepId, string view)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(s => s.UserID == studentId);

            ViewBag.studentName = student.FirstName + " " + student.LastName;
            var iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            var isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

            try
            {
                switch (view)
                {
                    case "HealthModule":
                        tblIEPHealth healthModel = db.tblIEPHealths.Where(h => h.IEPHealthID == iep.IEPHealthID).FirstOrDefault();
                        if (healthModel == null)
                        {
                            healthModel = new tblIEPHealth();
                        }

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_HealthSection", (tblIEPHealth)healthModel);
                        else
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

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_AcademicSection", academicModel);
                        else
                            return PartialView("_ModuleAcademicSection", academicModel);

                    case "MotorModule":
                        tblIEPMotor motorModel = db.tblIEPMotors.Where(m => m.IEPMotorID == iep.IEPMotorID).FirstOrDefault();
                        if (motorModel == null)
                        {
                            motorModel = new tblIEPMotor();
                        }

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_MotorSection", motorModel);
                        else
                            return PartialView("_ModuleMotorSection", motorModel);

                    case "CommunicationModule":
                        tblIEPCommunication communicationModel = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == iep.IEPCommunicationID).FirstOrDefault();
                        if (communicationModel == null)
                        {
                            communicationModel = new tblIEPCommunication();
                        }

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_CommunicationSection", communicationModel);
                        else
                            return PartialView("_ModuleCommunicationSection", communicationModel);

                    case "SocialModule":
                        tblIEPSocial socialModel = db.tblIEPSocials.Where(s => s.IEPSocialID == iep.IEPSocialID).FirstOrDefault();
                        if (socialModel == null)
                        {
                            socialModel = new tblIEPSocial();
                        }

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_SocialSection", socialModel);
                        else
                            return PartialView("_ModuleSocialSection", socialModel);

                    case "GeneralIntelligenceModule":
                        tblIEPIntelligence intelligenceModel = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == iep.IEPIntelligenceID).FirstOrDefault();
                        if (intelligenceModel == null)
                        {
                            intelligenceModel = new tblIEPIntelligence();
                        }

                        if (isReadOnly)
                            return PartialView("ActiveIEP/_GeneralIntelligenceSection", intelligenceModel);
                        else
                            return PartialView("_ModuleGeneralIntelligenceSection", intelligenceModel);

                    case "ProgressModule":
                        StudentGoalsViewModel model = new StudentGoalsViewModel();
                        List<tblGoal> studentGoals = db.tblGoals.Where(g => g.IEPid == iepId).ToList();
                        foreach (tblGoal goal in studentGoals)
                        {
                            model.studentGoals.Add(new StudentGoal(goal.goalID));
                        }

                        return PartialView("ActiveIEP/_ProgressReport", model);

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
                                  join m in db.tblBuildingMappings on u.UserID equals m.UserID
                                  join b in db.tblBuildings on m.BuildingID equals b.BuildingID
                                  where o.AdminID == id && u.RoleID == "5" && !(u.Archive ?? false)
                                  select new Student
                                  {
                                      UserID = u.UserID,
                                      FirstName = u.FirstName,
                                      LastName = u.LastName,
                                      Email = u.Email,
                                      BuildingName = b.BuildingName
                                  }).Distinct().ToList();
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = mis + "," + owner)]
        public ActionResult UnlockStudentIEP(int stid)
        {
            tblUser student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();
            if (student != null)
            {
                tblIEP theIEP = db.tblIEPs.Where(i => i.UserID == stid).FirstOrDefault();
                if (theIEP != null)
                {
                    return RedirectToAction("StudentProcedures", new { stid, theIEP.IEPid });
                }
                else
                {
                    new IEP(student.UserID);

                    theIEP = db.tblIEPs.Where(i => i.UserID == stid).FirstOrDefault();
                    theIEP.IepStatus = IEPStatus.PLAN;
                    db.SaveChanges();
                }

                return Json(new { Result = "success", Message = "student IEP was unlocked." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Error unlocking the student IEP." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult StudentProcedures(int stid, int? iepID = null)
        {
            StudentProcedureViewModel model = new StudentProcedureViewModel();

            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            tblUser student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();
            tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();
            tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
            tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

            ViewBag.UserRoleId = currentUser.RoleID;

            if (student != null)
            {
                model.student = student;
                model.birthDate = info.DateOfBirth;
                model.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
                model.isDoc = district.DOC;
                model.isGiftedOnly = info.isGifted && info.Primary_DisabilityCode == "ND" && info.Secondary_DisabilityCode == "ND";
                model.isCreator = currentUser.UserID == info.CreatedBy;

                IEP theIEP = (iepID != null) ? new IEP(student.UserID, iepID) : new IEP(student.UserID);
                if (theIEP.current != null)
                {
                    model.hasplan = theIEP.hasPlan;
                    model.studentIEP = theIEP;
                    model.studentPlan = new StudentPlan(student.UserID);
                    model.hasAccommodations = theIEP.hasAccommodations;
                    model.needsBehaviorPlan = theIEP.hasBehavior;
                }
                else
                {
                    model.hasplan = false;
                    model.studentIEP = theIEP.CreateNewIEP(stid);
                    model.studentPlan = new StudentPlan();
                    model.hasAccommodations = false;
                    model.needsBehaviorPlan = false;
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdateIEPDates(int stId, int IepId, string IEPStartDate, string IEPMeetingDate)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == stId && i.IEPid == IepId).FirstOrDefault();

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
        [Authorize(Roles = mis + "," + admin + "," + teacher)]
        public ActionResult UpdateIEPAmendmentToActive(int stId, int IEPid)
        {
            // get the iep and make sure it's an amendment.
            tblIEP studentAmmendIEP = db.tblIEPs.Where(i => i.UserID == stId && i.IEPid == IEPid && i.Amendment).FirstOrDefault();
            tblIEP studentActiveIEP = db.tblIEPs.Where(i => i.IEPid == studentAmmendIEP.AmendingIEPid).FirstOrDefault();
            if (studentAmmendIEP != null && studentActiveIEP != null)
            {
                // find the current active iep and make it inactive and change its status to DELETED
                studentActiveIEP.IepStatus = IEPStatus.ARCHIVE;
                studentActiveIEP.IsActive = false;
                studentAmmendIEP.IepStatus = IEPStatus.ACTIVE;

                try
                {
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "IEP Amendment status changed to Active." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable to change the IEP status." }, JsonRequestBehavior.AllowGet);
        }

        // GET: Manage/UpdateIEPStatus/5
        [HttpGet]
        [Authorize(Roles = mis + ", " + admin + "," + teacher)]
        public ActionResult UpdateIEPStatusToActive(int stId, int IEPid)
        {
            // switch the flag
            tblIEP iepDraft = db.tblIEPs.Where(i => i.UserID == stId && !i.Amendment && i.IsActive && i.IEPid == IEPid).FirstOrDefault();
            if (iepDraft != null)
            {
                if (iepDraft.IepStatus != IEPStatus.ACTIVE)
                {
                    //create archive
                    try
                    {
                        var theIEP = GetIEPPrint(stId, IEPid);
                        var data = RenderRazorViewToString("~/Views/Home/_PrintPartial.cshtml", theIEP);

                        string result = System.Text.RegularExpressions.Regex.Replace(data, @"\r\n?|\n|\t", "");
                        HtmlDocument doc = new HtmlDocument();
                        doc.OptionWriteEmptyNodes = true;
                        doc.OptionFixNestedTags = true;
                        doc.LoadHtml(result);

                        var studentInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("studentInformationPage")).FirstOrDefault();
                        var moduleInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("module-page")).FirstOrDefault();
                        var mergedFile = CreateIEPPdf(studentInfo.InnerHtml, moduleInfo.InnerHtml, "", stId.ToString(), "1", theIEP.current.IEPid.ToString(), "1", "Draft");

                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }

                    // start switching the flag.
                    iepDraft.IepStatus = IEPStatus.ACTIVE;
                    //iepDraft.begin_date = DateTime.Now;
                    iepDraft.end_Date = (!iepDraft.Amendment) ? iepDraft.begin_date.Value.AddYears(1) : iepDraft.end_Date;

                    try
                    {
                        db.SaveChanges();

                        return Json(new { Result = "success", Message = "IEP Status changed to Active." }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // There is already an active iep in play
                    return Json(new { Result = "error", Message = "This user already has an active IEP." }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable to change the IEP status." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult UpdateIEPStatusToInActive(int Stid, int IepId)
        {
            tblIEP studehtIEP = db.tblIEPs.Where(i => i.UserID == Stid && i.IEPid == IepId).FirstOrDefault();
            if (studehtIEP != null)
            {
                studehtIEP.IsActive = false;
                db.SaveChanges();

                return Json(new { Result = "success", Message = "IEP is archived." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Result = "error", Message = "Unknown Error. Unable make the IEP Inactive." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult UpdateRevertIEPtoDraft(int Stid, int IepId)
        {
            List<tblIEP> studentIEPs = db.tblIEPs.Where(i => i.UserID == Stid && i.IsActive).ToList();
            tblIEP studentActiveIEP = studentIEPs.Where(i => i.UserID == Stid && i.IEPid == IepId).FirstOrDefault();
            if (studentActiveIEP != null)
            {
                // if ammended is in play then they can't revert.
                tblIEP studentAmmendedIEP = studentIEPs.Where(i => i.AmendingIEPid == IepId && i.IsActive).FirstOrDefault();
                if (studentAmmendedIEP != null)
                {
                    return Json(new { Result = "error", Message = "You cannot revert an IEP that has an amendment." }, JsonRequestBehavior.AllowGet);
                }

                // if annual is in play then they can't revert.
                tblIEP studentAnnualIEP = studentIEPs.Where(i => i.IsActive && i.IepStatus == IEPStatus.ACTIVE && i.IEPid != IepId && i.IsActive).FirstOrDefault();
                if (studentAnnualIEP != null)
                {
                    return Json(new { Result = "error", Message = "You cannot revert an IEP that has an annual." }, JsonRequestBehavior.AllowGet);
                }

                // make sure there isn't another draft iep in play.
                tblIEP studentDraftIep = studentIEPs.Where(i => i.IepStatus == IEPStatus.DRAFT && i.IsActive && !i.Amendment && i.IsActive).FirstOrDefault();
                if (studentDraftIep == null)
                {
                    studentActiveIEP.IepStatus = IEPStatus.DRAFT;
                    studentActiveIEP.begin_date = null;
                    studentActiveIEP.MeetingDate = null;
                    studentActiveIEP.Update_Date = DateTime.Now;
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "IEP is reverted." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "error", Message = "There is already another Draft in play. Unable make to revert this IEP" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable make to revert this IEP" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult StudentGoals(int studentId, int IEPid)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            var isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

            if (iep != null)
            {
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
                ViewBag.studentName = student.FirstName + " " + student.LastName;

                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                StudentGoalsViewModel model = new StudentGoalsViewModel();
                model.studentId = studentId;
                model.iepId = iep.IEPid;
                model.isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse) ? true : false;
                model.canAddProgress = (teacher != null && teacher.RoleID == nurse) ? false : true;

                List<vw_ModuleGoalFlags> GoalFlag = db.vw_ModuleGoalFlags.Where(vm => vm.IEPid == iep.IEPid).ToList();
                model.modulesNeedingGoals = GoalFlag.Where(vm => vm.Module == "Health").FirstOrDefault().NeedMetByGoal == 1 ? "Health " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Motor").FirstOrDefault().NeedMetByGoal == 1 ? "Motor " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Communication").FirstOrDefault().NeedMetByGoal == 1 ? "Communication " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Social").FirstOrDefault().NeedMetByGoal == 1 ? "Social-Emotional " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Math").FirstOrDefault().NeedMetByGoal == 1 ? "Math " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Reading").FirstOrDefault().NeedMetByGoal == 1 ? "Reading " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Written").FirstOrDefault().NeedMetByGoal == 1 ? "Written&nbsp;Language " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Academic").FirstOrDefault().NeedMetByGoal == 1 ? "Academic&nbsp;Performance" : string.Empty;

                List<tblGoal> goals = db.tblGoals.Where(g => g.IEPid == iep.IEPid).ToList();
                foreach (tblGoal goal in goals)
                {
                    model.studentGoals.Add(new StudentGoal(goal.goalID));
                }

                if (!isReadOnly)
                    return PartialView("_ModuleStudentGoals", model);
                else
                    return PartialView("ActiveIEP/_StudentGoals", model);
            }

            return PartialView("_ModuleStudentGoals", new StudentGoalsViewModel());
        }

        [HttpGet]
        //[Authorize(Roles = teacher)]
        [Authorize]
        public ActionResult DuplicateStudentServicesNextYear(int studentId, int? serviceId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                //get latest year
                int maxYear = DateTime.Now.AddYears(1).Year;
                int currentYear = DateTime.Now.Year;
                if (serviceId.HasValue)
                {
                    currentYear = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.ServiceID == serviceId).Max(o => o.SchoolYear);
                    maxYear = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.ServiceID == serviceId).Max(o => o.SchoolYear) + 1;
                }
                else
                {
                    currentYear = db.tblServices.Where(s => s.IEPid == iep.IEPid).Max(o => o.SchoolYear);
                    maxYear = db.tblServices.Where(s => s.IEPid == iep.IEPid).Max(o => o.SchoolYear) + 1;
                }

                if (maxYear > 0)
                {
                    tblUser mis = FindSupervisor.GetUSersMIS(teacher);
                    tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    int startMonth = 7; //july
                    int endMonth = 6; //june

                    List<tblCalendar> availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.AssignedUSD && c.canHaveClass == true && c.NoService == false && c.Year >= currentYear && c.Year <= maxYear).OrderBy(c => c.SchoolYear).ThenBy(c => c.Month).ThenBy(c => c.Day).ToList();
                    var whatever = availableCalendarDays.Where(c => c.Month >= startMonth).OrderBy(c => c.Month).ThenBy(c => c.Day);
                    tblCalendar firstDaySchoolYear = availableCalendarDays.Where(c => c.Month >= startMonth).OrderBy(c => c.Month).ThenBy(c => c.Day).First();
                    tblCalendar lastDaySchoolYear = availableCalendarDays.Where(c => c.Month <= endMonth).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();


                    List<tblService> services = null;
                    if (serviceId.HasValue)
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear && s.ServiceID == serviceId).ToList();
                    else
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear).ToList();

                    List<StudentServiceObject> serviceList = new List<StudentServiceObject>();
                    foreach (var service in services)
                    {
                        var item = new StudentServiceObject();
                        var meetingDate =
                        item.DaysPerWeek = service.DaysPerWeek;
                        item.StartDate = firstDaySchoolYear != null && firstDaySchoolYear.calendarDate.HasValue ? firstDaySchoolYear.calendarDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();

                        if (iep.MeetingDate.HasValue && (iep.MeetingDate.Value > lastDaySchoolYear.calendarDate))
                        {
                            item.EndDate = iep.MeetingDate.Value.ToShortDateString();
                        }
                        else
                        {
                            item.EndDate = lastDaySchoolYear.calendarDate.Value.ToShortDateString();
                        }

                        //item.EndDate = iep.MeetingDate.HasValue ? iep.MeetingDate.Value.ToShortDateString() : lastDaySchoolYear != null && lastDaySchoolYear.calendarDate.HasValue ? lastDaySchoolYear.calendarDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                        item.LocationCode = service.LocationCode;
                        item.Minutes = service.Minutes;
                        item.ProviderID = service.ProviderID.HasValue ? service.ProviderID.Value : -1;
                        item.SchoolYear = service.SchoolYear;
                        item.ServiceCode = service.ServiceCode;
                        item.Frequency = service.Frequency;
                        if (service.tblGoals.Any())
                        {
                            foreach (var goal in service.tblGoals)
                            {
                                item.Goals += goal.goalID + ",";
                            }

                            item.Goals = item.Goals.Trim(',');
                        }

                        serviceList.Add(item);
                    }

                    return Json(new { Result = "success", Data = serviceList }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "fail" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult CheckCalendar(int studentId, int IEPid)
        {
            int lastYear = DateTime.Now.AddYears(-1).Year;
            int thirdYear = DateTime.Now.AddYears(2).Year;
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
            List<tblCalendar> calendar = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.USD && c.Year >= lastYear && c.Year <= thirdYear).OrderBy(c => c.Year).ToList();

            if (calendar.Count == 0)
            {
                return Json(new { Result = "error", Message = "The calendar for this district has not been created. Please create the calendar before you procede." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "success",  Message = "Nicely Done" }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize]
        public ActionResult StudentServices(int studentId, int IEPid)
        {
            bool isReadOnly = false;

            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name); // current teacher-esque user.
            tblUser mis = FindSupervisor.GetUSersMIS(teacher); // get the mis of the teacher
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault(); // gimme the student's iep.

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse) ? true : false;

                StudentServiceViewModel model = new StudentServiceViewModel();
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
                tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

                ViewBag.studentName = student.FirstName + " " + student.LastName;
                ViewBag.isMIS = mis.UserID == teacher.UserID;
                int lastYear = DateTime.Now.AddYears(-1).Year;
                int thirdYear = DateTime.Now.AddYears(2).Year;

                List<tblCalendar> calendar = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.USD && c.Year >= lastYear && c.Year <= thirdYear).OrderBy(c => c.Year).ToList();

                var providers = (from p in db.tblProviders
                                 join d in db.tblProviderDistricts on p.ProviderID equals d.ProviderID
                                 where d.USD != null && d.USD == studentInfo.AssignedUSD
                                 select p).ToList();

                List<tblService> services = db.tblServices.Where(s => s.IEPid == iep.IEPid).ToList();

                JsonResult Holidays = Json(calendar.Where(c => c.NoService || !c.canHaveClass).Select(c => c.calendarDate.Value.ToString("d-M-yyyy")).ToList(), JsonRequestBehavior.AllowGet);
                tblCalendar isPossibleLastFiscalDay = calendar.Where(c => c.canHaveClass && c.Year == DateTime.Now.Year && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();
                ViewBag.LastFiscalDayofYear = (isPossibleLastFiscalDay.calendarDate > DateTime.Now) ? isPossibleLastFiscalDay : calendar.Where(c => c.canHaveClass && c.Year == DateTime.Now.AddYears(1).Year && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

                if (services != null)
                {
                    model.studentId = studentId;
                    model.studentServices = services;
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = providers;
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = Holidays;
                    model.IEPStartDate = iep.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                }
                else
                {
                    model.studentId = studentId;
                    model.studentServices.Add(new tblService() { IEPid = iep.IEPid });
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.UserID).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.calendar = Holidays;
                    model.IEPStartDate = iep.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                }

                if (isReadOnly)
                    return PartialView("ActiveIEP/_StudentServices", model);
                else
                    return PartialView("_ModuleStudentServices", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize]
        public ActionResult ValidateServiceDate(int fiscalYear, string calendarDay, int studentId)
        {
            bool isValid = false;
            bool isService = true;
            string validDates = "";
            IsValidDate(fiscalYear, calendarDay, studentId, out isValid, out isService, out validDates);

            return Json(new { IsValid = isValid, IsService = isService, ValidDates = validDates }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ValidateCalendarReporting(int fiscalYear, int studentId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser mis = FindSupervisor.GetUSersMIS(teacher);
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
            int minutesPerDay = 60;
            int daysPerWeek = 5;

            var reporting = db.tblCalendarReportings.Where(r => r.BuildingID == studentInfo.BuildingID && r.USD == studentInfo.AssignedUSD && r.SchoolYear == fiscalYear).FirstOrDefault();

            if (reporting != null)
            {
                minutesPerDay = reporting.MinutesPerDay;
                daysPerWeek = reporting.DaysPerWeek;
            }

            return Json(new { MinutesPerDay = minutesPerDay, DaysPerWeek = daysPerWeek }, JsonRequestBehavior.AllowGet);
        }

        private void IsValidDate(int fiscalYear, string calendarDay, int studentId, out bool isValid, out bool isService, out string validDates)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser mis = FindSupervisor.GetUSersMIS(teacher);
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

            int startMonth = 7; //july
            int endMonth = 6; //june

            DateTime searchDate = Convert.ToDateTime(calendarDay);
            isValid = false;
            isService = true;
            validDates = "";

            //start date must be within the school year
            var availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.AssignedUSD && (c.canHaveClass == true && c.NoService == false) && c.SchoolYear == fiscalYear);

            if (availableCalendarDays != null)
            {
                var firstDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == startMonth).FirstOrDefault();
                var lastDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == endMonth).OrderByDescending(o => o.Day).FirstOrDefault();

                //keep looking for first day
                if (firstDaySchoolYear == null)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        startMonth++;
                        firstDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == startMonth && o.Year == fiscalYear - 1).FirstOrDefault();
                        if (firstDaySchoolYear != null)
                            break;
                    }
                }

                //keep looking for last day
                if (lastDaySchoolYear == null)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        endMonth--;
                        lastDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == endMonth && o.Year == fiscalYear).OrderByDescending(o => o.Day).FirstOrDefault();
                        if (lastDaySchoolYear != null)
                            break;
                    }
                }

                if (availableCalendarDays.Where(o => o.calendarDate == searchDate).Count() == 0)
                {
                    isService = false;
                }

                if (firstDaySchoolYear != null && firstDaySchoolYear.calendarDate.HasValue && lastDaySchoolYear != null && lastDaySchoolYear.calendarDate.HasValue)
                {
                    validDates = string.Format("Start Date: {0} End Date: {1}.", firstDaySchoolYear.calendarDate.Value.ToShortDateString(), lastDaySchoolYear.calendarDate.Value.ToShortDateString());
                    if ((searchDate >= firstDaySchoolYear.calendarDate.Value) && (searchDate <= lastDaySchoolYear.calendarDate.Value))
                    {
                        isValid = true;
                    }
                }
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveStudentService(FormCollection collection)
        {
            int StudentSerivceId = Convert.ToInt32(collection["StudentSerivceId"]);
            int studentId = Convert.ToInt32(collection["StudentId"]);
            bool isCompleted = Convert.ToBoolean(collection["completed"]);
            tblService service;

            //check dates
            bool isValidStartDate = false;
            bool isValidServiceStartDate = true;
            bool isValidEndDate = false;
            bool isValidServiceEndDate = true;
            bool isSuccess = false;
            string validDates = "";
            string errorMessage = "There was a problem saving the service";

            DateTime temp;
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId).FirstOrDefault();
            if (iep != null)
            {
                if (StudentSerivceId == 0) // new service
                {
                    service = new tblService();
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
                    service.Completed = isCompleted;
                    service.Create_Date = DateTime.Now;
                    service.Update_Date = DateTime.Now;

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

                    db.tblServices.Add(service);

                    //check dates
                    IsValidDate(service.SchoolYear, service.StartDate.ToShortDateString(), studentId, out isValidStartDate, out isValidServiceStartDate, out validDates);
                    IsValidDate(service.SchoolYear, service.EndDate.ToShortDateString(), studentId, out isValidEndDate, out isValidServiceEndDate, out validDates);
                }
                else // exsisting service
                {
                    service = db.tblServices.Where(s => s.ServiceID == StudentSerivceId).FirstOrDefault();
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
                    service.Completed = isCompleted;
                    service.FiledOn = null; //need to clear so it can be pickedup by spedpro export
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

                    //check dates
                    IsValidDate(service.SchoolYear, service.StartDate.ToShortDateString(), studentId, out isValidStartDate, out isValidServiceStartDate, out validDates);
                    IsValidDate(service.SchoolYear, service.EndDate.ToShortDateString(), studentId, out isValidEndDate, out isValidServiceEndDate, out validDates);
                }


                if (isValidStartDate && isValidServiceStartDate && isValidEndDate && isValidServiceEndDate)
                {
                    //save the service
                    db.SaveChanges();
                    StudentSerivceId = service.ServiceID;
                    isSuccess = true;
                }
                else
                {
                    errorMessage = "";

                    if (!isValidStartDate || !isValidServiceStartDate)
                    {
                        errorMessage += "The Initiation Date must be a valid date within the selected Fiscal Year. " + validDates + "<br/>";
                    }
                    if (!isValidEndDate || !isValidServiceEndDate)
                    {
                        errorMessage += "The End Date must be a valid date within the selected Fiscal Year. " + validDates + "<br/>";
                    }

                }
            }

            if (isSuccess)
            {
                //return Json Dummie.
                return Json(new { Result = "success", Message = "The service has been saved.", key = StudentSerivceId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Result = "false", Message = errorMessage }, JsonRequestBehavior.AllowGet);
            }
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
        [Authorize]
        public ActionResult StudentTransition(int studentId, int IEPid)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            bool isReadOnly = false;
            if (iep != null)
            {
                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();

                string studentFirstName = string.Format("{0}", student.FirstName);
                string studentLastName = string.Format("{0}", student.LastName);
                int studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse) ? true : false;

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
                model.isRequired = (studentAge > 13 || (model.isDOC && studentAge <= 21)) ? true : false;
                model.gender = info.Gender;
                model.careers = db.tblCareerPaths.Where(o => o.Active == true).ToList();

                var hasEmploymentGoal = model.goals.Any(o => o.GoalType == "employment");
                var hasEducationGoal = model.goals.Any(o => o.GoalType == "education");
                if (hasEmploymentGoal && hasEducationGoal)
                    model.canComplete = true;

                ViewBag.studentFirstName = studentFirstName;
                ViewBag.studentLastName = studentLastName;
                ViewBag.studentAge = studentAge;

                //isReadOnly = true;

                if (isReadOnly)
                    return PartialView("ActiveIEP/_StudentTransition", model);
                else
                    return PartialView("_ModuleStudentTransition", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize]
        public ActionResult BehaviorPlan(int studentId, int iepID)
        {
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepID).FirstOrDefault();
            List<SelectListItem> locationList = new List<SelectListItem>();
            bool isReadOnly = false;

            if (iep != null)
            {
                tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

                var model = GetBehaviorModel(studentId, iep.IEPid);

                if (isReadOnly)
                    return PartialView("ActiveIEP/_Behavior", model);
                else
                    return PartialView("_ModuleBehavior", model);
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize]
        public ActionResult Accommodations(int studentId, int IEPid)
        {
            var model = new AccomodationViewModel();
            bool isReadOnly = false;

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            List<SelectListItem> locationList = new List<SelectListItem>();

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

                model.StudentId = studentId;
                model.IEPid = iep.IEPid;

                var accommodations = db.tblAccommodations.Where(i => i.IEPid == iep.IEPid);
                if (accommodations.Any())
                {
                    model.AccomList = accommodations.OrderBy(o => o.AccomType).ToList();
                }

                var locations = db.tblLocations.Where(o => o.Active == true);
                if (locations.Any())
                {
                    foreach (var loc in locations)
                    {
                        locationList.Add(new SelectListItem() { Text = loc.Name, Value = loc.LocationCode });
                    }

                    model.Locations = locationList;
                }

                model.DefaultStartDate = iep.begin_date.HasValue ? iep.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                model.DefaultEndDate = String.Empty;
            }

            ViewBag.Locations = locationList;

            if (isReadOnly)
                return PartialView("ActiveIEP/_Accommodations", model);
            else
                return PartialView("_ModuleAccommodations", model);
        }

        [Authorize]
        public ActionResult OtherConsiderations(int studentId, int IEPid)
        {
            var model = new tblOtherConsideration();
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            bool isReadOnly = false;
            ViewBag.vehicleType = 0;
            ViewBag.minutes = "25";
            ViewBag.begin = "";
            ViewBag.end = "";
            if (iep != null)
            {
                tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

                model.IEPid = iep.IEPid;
                var oc = db.tblOtherConsiderations.Where(i => i.IEPid == iep.IEPid);
                if (oc.Any())
                {
                    model = oc.FirstOrDefault();
                }
                else
                {
                    //default value
                    model.DistrictAssessment_GradeNotAssessed = true;
                    model.StateAssessment_RequiredCompleted = true;
                }
            }

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            string studentName = "";
            if (student != null)
                studentName = string.Format("{0}", student.FirstName);

            ViewBag.StudentName = studentName;
            ViewBag.StudentId = studentId;
            ViewBag.FullName = string.Format("{0} {1}", student.FirstName, student.LastName);

            if (isReadOnly)
                return PartialView("ActiveIEP/_OtherConsiderations", model);
            else
                return PartialView("_ModuleOtherConsiderations", model);
        }

        [HttpPost]
        [Authorize]
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

        [HttpGet]
        [Authorize]
        public ActionResult dismissPlanning(int studentId, int iepId)
        {
            tblIEP studentIEP = db.tblIEPs.Where(i => i.IEPid == iepId && i.UserID == studentId).FirstOrDefault();
            if (studentIEP != null)
            {
                studentIEP.IepStatus = (studentIEP.IepStatus.ToUpper() == IEPStatus.PLAN) ? IEPStatus.DRAFT : studentIEP.IepStatus.ToUpper();
                db.SaveChanges();

                return Json(new { result = "success", message = studentId }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { result = "error", message = "Unable to change the IEP status from plan to draft." }, JsonRequestBehavior.AllowGet);
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
            thePlan.IntelligenceNoConcern = false;
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

            return Json(new { result = "success", message = studentId }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult IEPFormModule(int studentId)
        {
            IEPFormViewModel viewModel = new IEPFormViewModel();

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                viewModel.IEPForms = GetForms();
                viewModel.StudentId = studentId;
                viewModel.StudentName = string.Format("{0} {1}", !string.IsNullOrEmpty(student.FirstName) ? student.FirstName : "", !string.IsNullOrEmpty(student.LastName) ? student.LastName : "");
                viewModel.Archives = db.tblFormArchives.Where(u => u.Student_UserID == studentId).OrderByDescending(o => o.ArchiveDate).ToList();
            }

            return PartialView("_IEPFormModule", viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult IEPFormFile(int id, string fileName)
        {
            IEPFormFileViewModel viewModel = new IEPFormFileViewModel();
            viewModel.studentId = id;
            viewModel.fileName = fileName;

            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            tblUser teacher = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            //tblIEP iep = db.tblIEPs.Where(u => u.UserID == id).FirstOrDefault();
            var forms = GetForms();

            var form = forms.Where(o => o.Value == fileName).FirstOrDefault();
            if (form != null)
                viewModel.fileDesc = form.Text;

            //if (iep != null)
            //viewModel.iepId = iep.IEPid;

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
                fileViewModel.building = building != null ? building.BuildingName : "";

                tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                if (MIS != null)
                {
                    fileViewModel.districtContact = (from contact in db.tblContacts where contact.Active == 1 && contact.USD == fileViewModel.studentInfo.AssignedUSD select contact).FirstOrDefault();
                }

            }

            var lastReEval = db.tblArchiveEvaluationDates.Where(c => c.userID == id).OrderByDescending(o => o.evalutationDate).FirstOrDefault();
            if (lastReEval != null)
            {
                fileViewModel.lastReEvalDate = lastReEval.evalutationDate.ToShortDateString();
            }

			if (fileViewModel.studentInfo != null && fileViewModel.studentInfo.AssignedUSD != null)
			{
				var district = db.tblDistricts.Where(c => c.USD == fileViewModel.studentInfo.AssignedUSD).FirstOrDefault();
				fileViewModel.districtName = district != null ? district.DistrictName : "";
			}
						

			viewModel.fileModel = fileViewModel;

            return View("_IEPFormsFile", viewModel);
        }

        [Authorize]
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

        [Authorize]
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
            forms.Add(new SelectListItem { Text = "Re-Evaluation Not Needed Agreement Form", Value = "IEPReEvalNotNeeded" });
            forms.Add(new SelectListItem { Text = "Manifestation Determination Review Form", Value = "ManiDetermReview" });
            forms.Add(new SelectListItem { Text = "Summary of Performance Example", Value = "SOPExample" });
            forms.Add(new SelectListItem { Text = "IEP Team Considerations", Value = "IEPTeamConsider" });
            forms.Add(new SelectListItem { Text = "Parent Consent for Release of Information and Medicaid Reimbursement", Value = "ParentConsentMedicaid" });
            forms.Add(new SelectListItem { Text = "Physician Script", Value = "PhysicianScript" });
			forms.Add(new SelectListItem { Text = "Team Evaluation Report", Value = "TeamEvaluation" });
			forms.Add(new SelectListItem { Text = "Conference Summary", Value = "ConferenceSummary" });



			return forms.OrderBy(x => x.Text).ToList();
        }

        [Authorize]
        public ActionResult Reports()
        {
            return View("~/Views/Home/Reports.cshtml");
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintIEP(int stid, int iepId)
        {
            var theIEP = GetIEPPrint(stid, iepId);
            if (theIEP != null)
            {
                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintStudentInfo(int stid, int iepId)
        {
            var theIEP = GetIEPPrint(stid, iepId);
            ViewBag.IsStudentInfo = 1;
            if (theIEP != null)
            {
                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        private IEP GetIEPPrint(int stid, int iepId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(u => u.UserID == stid);
            iepId = (iepId == 0) ? db.tblIEPs.Where(i => i.UserID == stid && i.IEPid == iepId).OrderBy(i => i.IepStatus).FirstOrDefault().IEPid : iepId;
            var studentDetails = new StudentDetailsPrintViewModel();

            List<tblStudentRelationship> contacts = db.tblStudentRelationships.Where(i => i.UserID == stid).ToList();

            // Get the MIS id of the logged in teacher.
            tblUser mis = FindSupervisor.GetUSersMIS(teacher);

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
                         where iep.UserID == student.UserID && iep.IEPid == iepId
                         select new { iep, health, motor, communication, social, intelligence, academics, reading, math, written }).ToList();

            if (query.Count() == 1)
            {

                IEP theIEP = new IEP()
                {
                    current = query.SingleOrDefault().iep,
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
                    serviceProviders = db.tblProviders.Where(p => p.UserID == mis.UserID).ToList(),
                    studentFirstName = string.Format("{0}", student.FirstName),
                    studentLastName = string.Format("{0}", student.LastName),

                };


                //student goalds
                if (theIEP != null && theIEP.current != null)
                {
                    theIEP.studentGoals = db.tblGoals.Where(g => g.IEPid == theIEP.current.IEPid).ToList();
                    foreach (var goal in theIEP.studentGoals)
                    {
                        theIEP.studentGoalBenchmarks.AddRange(db.tblGoalBenchmarks.Where(g => g.goalID == goal.goalID).ToList());
                        theIEP.studentGoalEvalProcs.AddRange(db.tblGoalEvaluationProcedures.Where(g => g.goalID == goal.goalID).ToList());
                    }

                    theIEP.studentServices = db.tblServices.Where(g => g.IEPid == theIEP.current.IEPid).ToList();
                    theIEP.accommodations = db.tblAccommodations.Where(g => g.IEPid == theIEP.current.IEPid).ToList();
                    var studentBehavior = db.tblBehaviors.Where(g => g.IEPid == theIEP.current.IEPid).FirstOrDefault();
                    theIEP.studentBehavior = GetBehaviorModel(student.UserID, theIEP.current.IEPid);
                    theIEP.studentOtherConsiderations = db.tblOtherConsiderations.Where(o => o.IEPid == theIEP.current.IEPid).FirstOrDefault();

                    StudentTransitionViewModel stvw = new StudentTransitionViewModel();
                    stvw.studentId = student.UserID;
                    stvw.student = student;
                    stvw.assessments = db.tblTransitionAssessments.Where(a => a.IEPid == theIEP.current.IEPid).ToList();
                    stvw.services = db.tblTransitionServices.Where(s => s.IEPid == theIEP.current.IEPid).ToList();
                    stvw.goals = db.tblTransitionGoals.Where(g => g.IEPid == theIEP.current.IEPid).ToList();
                    stvw.transition = db.tblTransitions.Where(t => t.IEPid == theIEP.current.IEPid).FirstOrDefault() ?? new tblTransition();
                    if (stvw.transition != null)
                    {
                        stvw.careers = db.tblCareerPaths.Where(o => o.CareerPathID == stvw.transition.CareerPathID).ToList();
                    }
                    theIEP.studentTransition = stvw;
                    tblStudentInfo info = null;
                    if (student != null)
                    {

                        info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();
                        tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
                        tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

                        theIEP.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
                        stvw.isGiftedOnly = info.isGifted && info.Primary_DisabilityCode == "ND" && info.Secondary_DisabilityCode == "ND";
                        stvw.isDOC = district.DOC;
                        studentDetails.isDOC = district.DOC;

                    }

                    if (info != null && theIEP.current != null)
                    {
                        var studentBuilding = db.tblBuildings.Where(c => c.BuildingID == info.BuildingID).Take(1).FirstOrDefault();
                        var studentNeighborhoodBuilding = db.tblBuildings.Where(c => c.BuildingID == info.NeighborhoodBuildingID).Take(1).FirstOrDefault();
                        var studentCounty = db.tblCounties.Where(c => c.CountyCode == info.County).FirstOrDefault();
                        var studentUSD = db.tblDistricts.Where(c => c.USD == info.AssignedUSD).FirstOrDefault();

                        studentDetails.student = info;
                        studentDetails.teacher = teacher;
                        studentDetails.ethnicity = info.Ethicity == "Y" ? "Hispanic" : "Not Hispanic or Latino";
                        studentDetails.gender = info.Gender == "F" ? "Female" : "Male";
                        studentDetails.contacts = contacts;
                        studentDetails.building = studentBuilding;
                        studentDetails.neighborhoodBuilding = studentNeighborhoodBuilding;
                        studentDetails.studentCounty = studentCounty != null ? studentCounty.CountyName : "";
                        studentDetails.parentLang = GetLanguage(info.ParentLanguage);
                        studentDetails.studentLang = GetLanguage(info.StudentLanguage);
                        studentDetails.primaryDisability = GetDisability(info.Primary_DisabilityCode);
                        studentDetails.secondaryDisability = GetDisability(info.Secondary_DisabilityCode);
                        studentDetails.studentAgeAtIEP = (theIEP.current.begin_date.HasValue ? (theIEP.current.begin_date.Value.Year - info.DateOfBirth.Year - 1) + (((theIEP.current.begin_date.Value.Month > info.DateOfBirth.Month) || ((theIEP.current.begin_date.Value.Month == info.DateOfBirth.Month) && (theIEP.current.begin_date.Value.Day >= info.DateOfBirth.Day))) ? 1 : 0) : 0);
                        studentDetails.studentAgeAtAnnualMeeting = (theIEP.current.MeetingDate.HasValue ? (theIEP.current.MeetingDate.Value.Year - info.DateOfBirth.Year - 1) + (((theIEP.current.MeetingDate.Value.Month > info.DateOfBirth.Month) || ((theIEP.current.MeetingDate.Value.Month == info.DateOfBirth.Month) && (theIEP.current.MeetingDate.Value.Day >= info.DateOfBirth.Day))) ? 1 : 0) : 0);
                        studentDetails.inititationDate = theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : "";
                        studentDetails.assignChildCount = studentUSD != null ? studentUSD.DistrictName : "";
                        studentDetails.placementCodeDesc = info != null ? db.tblPlacementCodes.Where(c => c.PlacementCode == info.PlacementCode).FirstOrDefault().PlacementDescription : "";
                        studentDetails.edStatusCodeDesc = info != null && db.tblStatusCodes.Where(c => c.StatusCode == info.StatusCode).Any() ? db.tblStatusCodes.Where(c => c.StatusCode == info.StatusCode).FirstOrDefault().Description : "";
                        studentDetails.reevalDates = db.tblArchiveEvaluationDates.Where(c => c.userID == stid).OrderByDescending(o => o.evalutationDate).ToList();
                    }

                    theIEP.studentDetails = studentDetails;

                    return theIEP;
                }
            }

            return null;
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        [Authorize]
        public ActionResult EditStudentInformation()
        {
            return View();
        }

        [Authorize]
        public ActionResult EditTeamStatements()
        {
            return View();
        }

        [Authorize]
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

        public ActionResult ContactUs(FormCollection collection)
        {
            try
            {
                // email this user the password
                SmtpClient smtpClient = new SmtpClient();

                MailMessage mailMessage = new MailMessage();
                mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress("GreenbushIEP@greenbush.org"));
                mailMessage.To.Add("melanie.johnson@greenbush.org");
                mailMessage.Subject = "IEP Greenbush Contact. Message from Backpack!";
                mailMessage.Body = String.Format("{0} has contacted you from email {1} with this message {2}", collection["Name"], collection["email"], collection["Message"]);
                smtpClient.Send(mailMessage);
            }
            catch (Exception e)
            {
                throw new EmailException("There was a problem when emailing the new user password.", e);
            }

            return View("Home");
        }

        public ActionResult MySettings()
        {
            return View();
        }

        [Authorize]
        public ActionResult Updates()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            ViewBag.fileVersion = version;

            tblUser user = db.tblUsers.SingleOrDefault(u => u.Email == User.Identity.Name);
            user.LastVersionNumberSeen = version;
            db.SaveChanges();

            var model = new List<tblVersionLog>();
            model = db.tblVersionLogs.Where(u => u.VersionNumber == version).ToList();

            return View(model);
        }

        [Authorize]
        public ActionResult DownloadArchive(int id)
        {
            //TODO: Check if user has permissions to update permissions
            var document = db.tblFormArchives.Where(o => o.FormArchiveID == id).FirstOrDefault();
            if (document != null)
            {
                new System.Net.Mime.ContentDisposition
                {
                    FileName = document.FormName + ".pdf",
                    Inline = false
                };
                Response.AppendHeader("Content-Disposition", "attachment;filename=" + ScrubDocumentName(document.FormName + ".pdf"));

                return File(document.FormFile, "application/pdf");
            }
            else
                return null;
        }

        [HttpPost]
        public ActionResult UploadStudentFile(HttpPostedFileBase myFile, int studentId)
        {
            try
            {
                using (var binaryReader = new BinaryReader(myFile.InputStream))
                {
                    var fileName = Path.GetFileName(myFile.FileName);
                    string fileNameExt = Path.GetExtension(fileName);

                    if (fileNameExt.ToLower() != ".pdf")
                        return Json(new { result = false, message = "Please select a valid PDF" }, "text/plain");

                    byte[] fileData = binaryReader.ReadBytes(myFile.ContentLength);

                    tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                    //int iepId = db.tblIEPs.Where(i => i.UserID == studentId).OrderBy(i => i.IepStatus).FirstOrDefault().IEPid;

                    var archive = new tblFormArchive();
                    archive.Creator_UserID = teacher.UserID;
                    archive.Student_UserID = studentId;
                    archive.FormName = string.IsNullOrEmpty(fileName) ? "Upload" : fileName;
                    archive.FormFile = fileData;
                    archive.ArchiveDate = DateTime.Now;
                    archive.isUpload = true;

                    db.tblFormArchives.Add(archive);
                    db.SaveChanges();
                }

                var archives = db.tblFormArchives.Where(u => u.Student_UserID == studentId && u.isUpload).OrderByDescending(o => o.ArchiveDate).ToList();

                var archiveList = new List<IEPFormFileViewModel>();
                foreach (var archive in archives)
                {
                    archiveList.Add(new IEPFormFileViewModel() { fileDate = string.Format("{0} {1}", archive.ArchiveDate.ToShortDateString(), archive.ArchiveDate.ToShortTimeString()), fileName = archive.FormName, id = archive.FormArchiveID });
                }

                return Json(new { result = true, message = "File uploaded successfully.", archives = archiveList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message }, "text/plain");
            }
        }

        public ActionResult SpedProReport()
        {
            return View("~/Reports/SpedPro/Index.cshtml");
        }

        [Authorize]
        public ActionResult DownloadSpedPro(FormCollection collection)
        {
            string fiscalYearStr = collection["fiscalYear"];
            int fiscalYear = 0;
            Int32.TryParse(fiscalYearStr, out fiscalYear);

            string iepStatus = IEPStatus.ACTIVE;
            var exportErrors = new List<ExportErrorView>();

            var query = (from iep in db.tblIEPs
                         join student in db.tblUsers
                             on iep.UserID equals student.UserID
                         join services in db.tblServices
                             on iep.IEPid equals services.IEPid
                         where
                         iep.IepStatus == iepStatus
                         && services.SchoolYear == fiscalYear
                         && (services.FiledOn == null || iep.FiledOn == null)
                         select new { iep, student }).Distinct().ToList();

            if (query.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var item in query)
                {
                    IEP theIEP = new IEP()
                    {
                        current = item.iep,
                        studentFirstName = string.Format("{0}", item.student.FirstName),
                        studentLastName = string.Format("{0}", item.student.LastName),
                    };

                    if (theIEP != null && theIEP.current != null)
                    {
                        var studentDetails = new StudentDetailsPrintViewModel();
                        theIEP.studentServices = db.tblServices.Where(g => g.IEPid == theIEP.current.IEPid && g.SchoolYear == fiscalYear).ToList();
                        theIEP.studentOtherConsiderations = db.tblOtherConsiderations.Where(o => o.IEPid == theIEP.current.IEPid).FirstOrDefault();

                        tblStudentInfo info = null;
                        if (student != null)
                        {
                            info = db.tblStudentInfoes.Where(i => i.UserID == item.iep.UserID).FirstOrDefault();
                            tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
                            tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();
                        }

                        if (info != null && theIEP.current != null)
                        {
                            var studentBuilding = db.tblBuildings.Where(c => c.BuildingID == info.BuildingID).Take(1).FirstOrDefault();
                            var studentNeighborhoodBuilding = db.tblBuildings.Where(c => c.BuildingID == info.NeighborhoodBuildingID).Take(1).FirstOrDefault();
                            var studentCounty = db.tblCounties.Where(c => c.CountyCode == info.County).FirstOrDefault();
                            var studentUSD = db.tblDistricts.Where(c => c.USD == info.AssignedUSD).FirstOrDefault();

                            studentDetails.student = info;
                            studentDetails.gender = info.Gender;
                            studentDetails.building = studentBuilding;
                            studentDetails.neighborhoodBuilding = studentNeighborhoodBuilding;
                            studentDetails.studentCounty = studentCounty != null ? studentCounty.CountyCode : "";
                            studentDetails.parentLang = (string.IsNullOrEmpty(info.ParentLanguage)) ? "EN" : info.ParentLanguage;
                            studentDetails.primaryDisability = (info.Primary_DisabilityCode == "ND") ? string.Empty : info.Primary_DisabilityCode;
                            studentDetails.secondaryDisability = (info.Secondary_DisabilityCode == "ND") ? string.Empty : info.Secondary_DisabilityCode;
                            studentDetails.inititationDate = theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : "";
                            studentDetails.assignChildCount = studentUSD.KSDECode;
                        }

                        theIEP.current.FiledOn = DateTime.Now;
                        theIEP.studentDetails = studentDetails;
                    }

                    var errors = CreateSpedProExport(theIEP, fiscalYear, sb);

                    if (errors.Count > 0)
                    {
                        exportErrors.AddRange(errors);
                    }
                    else
                    {
                        db.SaveChanges();
                    }
                }//end foreach


                if (exportErrors.Count == 0)
                {
                    Response.Clear();
                    Response.ClearHeaders();

                    Response.AppendHeader("Content-Length", sb.Length.ToString());
                    Response.ContentType = "text/plain";
                    Response.AppendHeader("Content-Disposition", "attachment;filename=\"SpedProExport.txt\"");

                    Response.Write(sb);
                    Response.End();
                }
                else
                {
                    ViewBag.errors = exportErrors;
                }
            }
            else
            {
                exportErrors.Add(new ExportErrorView()
                {
                    UserID = "",
                    Description = "No data found to export."
                });

                ViewBag.errors = exportErrors;
            }

            return View("~/Reports/SpedPro/Index.cshtml");
        }

        private List<ExportErrorView> CreateSpedProExport(IEP studentIEP, int schoolYear, StringBuilder sb)
        {
            var errors = new List<ExportErrorView>();

            //1 KidsID Req
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.KIDSID);

            //2 Last Name, Student’s Legal Req less < 60 characters
            sb.AppendFormat("{0}\t", studentIEP.studentLastName.Length > 60 ? studentIEP.studentLastName.Substring(0, 60) : studentIEP.studentLastName);

            //3 Student’s Gender
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.Gender == "M" ? 1 : 0);

            //4 DOB MM/DD/YYYY
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.DateOfBirth.ToShortDateString());

            //5 School Year YYYY Req
            sb.AppendFormat("{0}\t", schoolYear);

            //6 Assign Child Count Req
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.assignChildCount);

            //7 Neighborhood Building Identifier Req
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.neighborhoodBuilding.BuildingID);

            //8 grade level req
            var gradeCode = "";
            switch (studentIEP.studentDetails.student.Grade)
            {
                case 0:
                    gradeCode = "05";
                    break;

                case 1:
                    gradeCode = "06";
                    break;

                case 2:
                    gradeCode = "07";
                    break;

                case 3:
                    gradeCode = "08";
                    break;

                case 4:
                    gradeCode = "09";
                    break;

                case 5:
                    gradeCode = "10";
                    break;

                case 6:
                    gradeCode = "11";
                    break;

                case 7:
                    gradeCode = "12";
                    break;

                case 8:
                    gradeCode = "13";
                    break;

                case 9:
                    gradeCode = "14";
                    break;

                case 10:
                    gradeCode = "15";
                    break;

                case 11:
                    gradeCode = "16";
                    break;

                case 12:
                    gradeCode = "17";
                    break;
            }
            if (gradeCode == "")
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: 8 - Grade")
                });
            }
            else
            {
                sb.AppendFormat("{0}\t", gradeCode);
            }

            //9 status code req
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.StatusCode);

            //10 exit date
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.ExitDate.HasValue ? studentIEP.studentDetails.student.ExitDate.Value.ToShortDateString() : "");

            //11 School Psychologist Provider ID
            sb.AppendFormat("{0}\t", "");

            //12 slp provider id
            sb.AppendFormat("{0}\t", "");

            //13 case manager provider id
            sb.AppendFormat("{0}\t", "");

            //14 extended school year
            sb.AppendFormat("{0}\t", studentIEP.studentOtherConsiderations != null ? studentIEP.studentOtherConsiderations.ExtendedSchoolYear_Necessary : "");

            //15 sped transportation
            sb.AppendFormat("{0}\t", studentIEP.studentOtherConsiderations != null ? studentIEP.studentOtherConsiderations.Transporation_Required.HasValue && studentIEP.studentOtherConsiderations.Transporation_Required.Value ? "1" : "0" : "");

            //16 All Day Kindergarten
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.FullDayKG == null ? "" : studentIEP.studentDetails.student.FullDayKG.Value == true ? "1" : "");

            //17 Behavior Intervention Plan - BIP BehaviorInterventionPlan
            sb.AppendFormat("{0}\t", studentIEP.studentSocial != null && studentIEP.studentSocial.BehaviorInterventionPlan ? "1" : "");

            //18 Claiming Code req
            sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.ClaimingCode ? "1" : "");

            //19 Placed By KDCF/JJA/LEA/Parent req
            if (string.IsNullOrEmpty(studentIEP.studentDetails.student.PlacementCode))
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: 19 - Placed By KDCF/JJA/LEA/Parent")
                });
            }
            else
            {
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.PlacementCode);
            }

            //20 County of Residence  req
            if (string.IsNullOrEmpty(studentIEP.studentDetails.studentCounty))
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: 20 County of Residence")
                });
            }
            else
            {
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.studentCounty);
            }

            //21 Language of Parent  req
            if (string.IsNullOrEmpty(studentIEP.studentDetails.parentLang))
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: 21 Language of Parent")
                });
            }
            else
            {
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.parentLang);
            }

            int count = 1;
            foreach (var service in studentIEP.studentServices)
            {
                if (count == 25)
                    break;

                service.FiledOn = DateTime.Now;
                //1 IEP date req
                if (!studentIEP.current.begin_date.HasValue)
                {
                    errors.Add(new ExportErrorView()
                    {
                        UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                        Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: R1 IEP date")
                    });
                }
                else
                {
                    sb.AppendFormat("{0}\t", studentIEP.current.begin_date.Value.ToShortDateString());
                }

                //2 gap allow
                sb.AppendFormat("{0}\t", "");

                //3 Responsible School req
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.neighborhoodBuilding.BuildingID);

                //4 primary disablity
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.primaryDisability);

                //5 secondary disablity
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.secondaryDisability);

                //6 gifted
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.student.isGifted ? "1" : "0");

                //7 service location
                sb.AppendFormat("{0}\t", studentIEP.studentDetails.building.BuildingID);

                //8 Primary Service Location Indicator
                sb.AppendFormat("{0}\t", "");

                //9 setting code
                sb.AppendFormat("{0}\t", service.LocationCode);

                //10 service code
                sb.AppendFormat("{0}\t", service.ServiceCode);

                //11 provider id
                sb.AppendFormat("{0}\t", service.tblProvider != null ? service.tblProvider.ProviderCode.Length > 10 ? service.tblProvider.ProviderCode.Substring(0, 10) : service.tblProvider.ProviderCode : "");

                //12 Primary Provider Indicator
                sb.AppendFormat("{0}\t", "");

                //13 Service Start Date
                sb.AppendFormat("{0}\t", service.StartDate.ToShortDateString());

                //14 Service end Date
                sb.AppendFormat("{0}\t", service.EndDate.ToShortDateString());

                //15 minutes
                sb.AppendFormat("{0}\t", service.Minutes);

                //16 days per
                sb.AppendFormat("{0}\t", service.DaysPerWeek);

                //17 freq
                sb.AppendFormat("{0}\t", service.Frequency);

                //18 total days
                sb.AppendFormat("{0}", "");

                count++;
            }

            sb.Append(Environment.NewLine);

            return errors;
        }

        [Authorize]
        public static string ScrubDocumentName(string documentName)
        {
            return documentName.Replace(',', ' ');
        }

        [Authorize]
        [ValidateInput(false)]
        public FileResult DownloadPDF(FormCollection collection)
        {

            string StudentHTMLContent = collection["studentText"];
            string HTMLContent = collection["printText"];
            string studentName = collection["studentName"];
            string studentId = collection["studentId"];
            string isArchive = collection["isArchive"];
            string iepIDStr = collection["iepID"];
            string isIEP = collection["isIEP"];
            string formName = collection["formName"];

            var mergedFile = this.CreateIEPPdf(StudentHTMLContent, HTMLContent, studentName, studentId, isArchive, iepIDStr, isIEP, formName);
            if (mergedFile != null)
            {
                string downloadFileName = string.IsNullOrEmpty(HTMLContent) ? "StudentInformation.pdf" : "IEP.pdf";
                return File(mergedFile, "application/pdf", downloadFileName);
            }
            else
                return null;

        }

        private byte[] CreateIEPPdf(string StudentHTMLContent, string HTMLContent, string studentName, string studentId,
        string isArchive, string iepIDStr, string isIEP, string formName)
        {
            if (!string.IsNullOrEmpty(HTMLContent) || !string.IsNullOrEmpty(StudentHTMLContent))
            {
                string logoImage = Server.MapPath("../Content/IEPBackpacklogo_black2.png");
                iTextSharp.text.Image imgfoot = iTextSharp.text.Image.GetInstance(logoImage);


                int id = 0;
                Int32.TryParse(studentId, out id);

                int iepId = 0;
                Int32.TryParse(iepIDStr, out iepId);

                tblUser user = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
                if (user != null && isIEP == "1")
                {
                    //update only if user it printing IEP
                    user.Agreement = true;
                    db.SaveChanges();
                }

                if (string.IsNullOrEmpty(studentName))
                {
                    studentName = string.Format("{0} {1}", user.FirstName, user.LastName);
                }


                bool isDraft = false;

                var iepObj = db.tblIEPs.Where(o => o.IEPid == iepId).FirstOrDefault();
                if (iepObj != null)
                {
                    isDraft = iepObj.IepStatus != null && iepObj.IepStatus.ToUpper() == "DRAFT" ? true : false;
                }


                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                var cssText = @"<style>hr{color:whitesmoke}h5{font-weight:500}.module-page{font-size:9pt;}.header{color:white;}img{margin-top:-10px;}.input-group-addon, .transitionGoalLabel, .transitionServiceLabel {font-weight:600;}.transitionServiceLabel, .underline{ text-decoration: underline;}.transition-break{page-break-before:always;}td { padding: 10px;}th {font-weight:600;}table {width:600px;border-spacing: 0px;border:none;font-size:9pt}.module-page, span {font-size:10pt;}label{font-weight:600;font-size:9pt}.text-center{text-align:center} h3 {font-weight:400;font-size:11pt;width:100%;text-align:center;padding:8px;}p {padding-top:5px;padding-bottom:5px;font-size:9pt}.section-break {page-break-after:always;color:white;background-color:white}.funkyradio {padding-bottom:15px;}.radio-inline {font-weight:normal;}div{padding-top:10px;}.form-check {padding-left:5px;}.dont-break {margin-top:10px;page-break-inside: avoid;} .form-group{margin-bottom:8px;} div.form-group-label{padding:0;padding-top:3px;padding-bottom:3px;} .checkbox{margin:0;padding:0} .timesfont{font-size:12pt;font-family:'Times New Roman',serif}</style>";
                string result = "";
                if (!string.IsNullOrEmpty(HTMLContent))
                {
                    result = System.Text.RegularExpressions.Regex.Replace(HTMLContent, @"\r\n?|\n", "");
                    result = System.Text.RegularExpressions.Regex.Replace(HTMLContent, @"</?textarea>", "");
                }

                string cssTextResult = System.Text.RegularExpressions.Regex.Replace(cssText, @"\r\n?|\n", "");
                byte[] studentFile = null;

                if (!string.IsNullOrEmpty(StudentHTMLContent))
                {
                    string result2 = System.Text.RegularExpressions.Regex.Replace(StudentHTMLContent, @"\r\n?|\n", "");
                    result2 = System.Text.RegularExpressions.Regex.Replace(StudentHTMLContent, @"textarea", "p");
                    studentFile = CreatePDFBytes(cssTextResult, result2, "studentInformationPage", imgfoot, "", isDraft, false);
                }

                byte[] iepFile = null;
                if (!string.IsNullOrEmpty(result))
                    iepFile = CreatePDFBytes(cssTextResult, result, "module-page", imgfoot, studentName, isDraft, true);

                List<byte[]> pdfByteContent = new List<byte[]>();

                if (studentFile != null)
                    pdfByteContent.Add(studentFile);

                if (iepFile != null)
                    pdfByteContent.Add(iepFile);
                else
                    formName = "Student Information";//this is just the student info page print

                var mergedFile = concatAndAddContent(pdfByteContent);

                if (isArchive == "1")
                {
                    try
                    {
                        var archive = new tblFormArchive();
                        archive.Creator_UserID = teacher.UserID;
                        archive.Student_UserID = id;
                        archive.FormName = string.IsNullOrEmpty(formName) ? "IEP" : formName;
                        archive.FormFile = mergedFile;//fileIn;
                                                      //archive.IEPid = iepId;
                        archive.ArchiveDate = DateTime.Now;

                        db.tblFormArchives.Add(archive);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "";

                        if (ex is DbEntityValidationException)
                        {

                            if (((DbEntityValidationException)(ex)).EntityValidationErrors.Any())
                            {
                                var errors = ((DbEntityValidationException)(ex)).EntityValidationErrors;
                                foreach (var failure in errors)
                                {
                                    foreach (var error in failure.ValidationErrors)
                                    {
                                        string propertyName = error.PropertyName;

                                        errorMessage += propertyName + " " + error.ErrorMessage;

                                    }
                                }
                            }
                        }
                        else
                        {
                            errorMessage = ex.Message;
                        }
                    }
                }


                return mergedFile;
            }

            return null;
        }

        private byte[] CreatePDFBytes(string cssTextResult, string result2, string className, iTextSharp.text.Image imgfoot, string studentName, bool isDraft, bool skipSignatureDraft)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionWriteEmptyNodes = true;
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(cssTextResult + "<div class='" + className + "'>" + result2 + "</div>");

            string cleanHTML2 = doc.DocumentNode.OuterHtml;

            byte[] fileIn = null;
            byte[] printFile = null;
            using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssTextResult)))
            {
                using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cleanHTML2)))
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

                            fileIn = stream.ToArray();

                            printFile = AddPageNumber(fileIn, studentName, imgfoot, isDraft, skipSignatureDraft);

                        }
                    }
                }
            }//end css stream

            return printFile;
        }

        private static byte[] concatAndAddContent(List<byte[]> pdfByteContent)
        {

            using (var ms = new MemoryStream())
            {
                using (var doc = new Document())
                {
                    using (var copy = new PdfSmartCopy(doc, ms))
                    {
                        doc.Open();

                        //Loop through each byte array
                        foreach (var p in pdfByteContent)
                        {

                            //Create a PdfReader bound to that byte array
                            using (var reader = new PdfReader(p))
                            {

                                //Add the entire document instead of page-by-page
                                copy.AddDocument(reader);
                            }
                        }

                        doc.Close();
                    }
                }

                //Return just before disposing
                return ms.ToArray();
            }
        }

        byte[] AddPageNumber(byte[] fileIn, string studentName, iTextSharp.text.Image imgfoot, bool isDraft, bool skipSignatureDraft)
        {
            byte[] bytes = fileIn;
            byte[] fileOut = null;
            Font blackFont = FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK);
            Font grayFont = FontFactory.GetFont("Arial", 75, Font.NORMAL, new BaseColor(218, 218, 218));
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(bytes);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    int pages = reader.NumberOfPages;

                    for (int i = 1; i <= pages; i++)
                    {

                        if (isDraft)
                        {
                            if (skipSignatureDraft && i == 1)
                            {
                                //continue;
                            }
                            else
                            {
                                ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_CENTER, new Phrase("DRAFT", grayFont), 300f, 400f, 0);
                            }
                        }

                        if (studentName != string.Empty)
                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_LEFT, new Phrase(studentName, blackFont), 25f, 750f, 0);

                        //Footer
                        //Phrase logoPhrase = new Phrase(string.Format("{0}", "IEP Backpack"), blackFont);
                        imgfoot.SetAbsolutePosition(250f, 10f);
                        imgfoot.ScalePercent(30);
                        stamper.GetOverContent(i).AddImage(imgfoot);

                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_LEFT, new Phrase(string.Format("Page {0} of {1}", i.ToString(), pages.ToString()), blackFont), 25f, 15f, 0);
                        //ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, logoPhrase, 365f, 15f, 0);
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(string.Format("Date Printed: {0}", DateTime.Now.ToShortDateString()), blackFont), 568f, 15f, 0);
                    }
                }
                fileOut = stream.ToArray();
            }

            return fileOut;
        }

        private static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        protected override void OnException(ExceptionContext filterContext)
        {

            TempData["Error"] = filterContext.Exception.Message;
            filterContext.ExceptionHandled = true;

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Error");
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
                model.Completed = BehaviorIEP.Completed;
                model.BehaviorConcern = BehaviorIEP.BehaviorConcern;
                model.StrengthMotivator = BehaviorIEP.StrengthMotivator;
                model.Crisis_Description = BehaviorIEP.Crisis_Description;
                model.Crisis_Escalation = BehaviorIEP.Crisis_Escalation;
                model.Crisis_Implementation = BehaviorIEP.Crisis_Implementation;
                model.Crisis_Other = BehaviorIEP.Crisis_Other;
                model.ReviewedBy = BehaviorIEP.ReviewedBy;
                model.isBehaviorPlanInSocialModuleChecked = db.tblIEPSocials.Where(b => b.IEPid == BehaviorIEP.IEPid).FirstOrDefault().BehaviorInterventionPlan;
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

        private string GetDisability(string value)
        {
            string fullName = "";
            var disablity = db.tblDisabilities.Where(o => o.DisabilityCode == value).FirstOrDefault();
            if (disablity != null)
            {
                fullName = string.Format("({0}) {1}", disablity.DisabilityCode, disablity.DisabilityDescription);
            }

            return fullName;

        }
    }
}