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
        [OutputCache(Duration = 15, VaryByParam = "none", Location = System.Web.UI.OutputCacheLocation.Client)]
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

                model.members = db.uspUserList(OWNER.UserID, null, null, null).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), hasIEP = u.hasIEP ?? false }).ToList();

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

                model.members = db.uspUserListByUserID(MIS.UserID, null).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), hasIEP = u.hasIEP ?? false }).ToList();

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

                //model.members = db.vw_UserList.Where(ul => (ul.RoleID == teacher || ul.RoleID == student || ul.RoleID == nurse) && (myBuildings.Contains(ul.BuildingID) && myDistricts.Contains(ul.USD))).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false }).Distinct().OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
                model.members = db.uspUserListByUserID(ADMIN.UserID, null).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), hasIEP = u.hasIEP ?? false }).ToList();

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
                                }).Distinct().ToList();

                //get IEP Date
                foreach (var student in students)
                {
                    IEP theIEP = new IEP(student.UserID);
                    student.hasIEP = (theIEP.current != null) ? theIEP.current.IEPid != 0 : false; //theIEP.current == null ? false : theIEP.current.IepStatus != IEPStatus.PLAN || theIEP.current.IepStatus != IEPStatus.ARCHIVE;
                    student.IEPDate = DateTime.Now.ToString("MM-dd-yyyy");
                    if (theIEP != null && theIEP.current != null && theIEP.current.begin_date.HasValue)
                        student.IEPDate = theIEP.current.begin_date.Value.ToShortDateString();
                }

                var model = new StudentViewModel();
                model.Teacher = teacher;
                model.Students = students.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
                model.districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == teacher.UserID select district).Distinct().ToList();
                model.buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == teacher.UserID select building).Distinct().ToList();


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
                                }).Distinct().ToList();

                //get IEP Date
                foreach (var student in students)
                {
                    IEP theIEP = new IEP(student.UserID);
                    student.hasIEP = (theIEP.current.IepStatus != IEPStatus.PLAN || theIEP.current.IepStatus != IEPStatus.ARCHIVE) && (theIEP.current.IsActive);
                    if (theIEP != null && theIEP.current != null && theIEP.current.begin_date.HasValue)
                        student.IEPDate = theIEP.current.begin_date.Value.ToShortDateString();
                }

                var model = new StudentViewModel();
                model.Teacher = nurse;
                model.Students = students.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
                model.districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == nurse.UserID select district).Distinct().ToList();
                model.buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == nurse.UserID select building).Distinct().ToList();

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
                                   select providers).Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

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
                        db.tblProviderDistricts.Add(new tblProviderDistrict() { ProviderID = provider.ProviderID, USD = district, CreatedBy = owner.UserID, Create_Date = DateTime.Now });
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
					newProvider.Create_Date = DateTime.Now;
					newProvider.Update_Date = DateTime.Now;

                    //can't have duplicate provider code
                    tblProvider dup = db.tblProviders.Where(p => p.ProviderCode == providerCode).SingleOrDefault();

                    if (dup == null)
                    {
						try
						{
							db.tblProviders.Add(newProvider);
							db.SaveChanges();

							int newProvderId = newProvider.ProviderID;

							//add to tblProviderDistricts
							if (newProvderId > 0)
							{
								foreach (var district in providerDistrict)
								{
									db.tblProviderDistricts.Add(new tblProviderDistrict() { ProviderID = newProvderId, USD = district.ToString(), CreatedBy = owner.UserID, Create_Date = DateTime.Now });
									db.SaveChanges();
								}
							}
						}
						catch
						{
							return Json(new { Result = "error", id = pk, errors = "There was a problem creating the provider. Please ask a sysadmin for help." }, JsonRequestBehavior.AllowGet);
						}

                    }
                    else
                    {
                        return Json(new { Result = "error", id = pk, errors = "Provider code already exists" }, JsonRequestBehavior.AllowGet);
                    }

                    //var listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    var MISDistrictList = (from buildingMaps in db.tblBuildingMappings
                                           join districts in db.tblDistricts
                                                on buildingMaps.USD equals districts.USD
                                           where buildingMaps.UserID == owner.UserID
                                           select districts).Distinct().ToList();

                    List<string> listOfUSD = MISDistrictList.Select(d => d.USD).ToList();

                    List<ProviderViewModel> listOfProviders = new List<ProviderViewModel>();
                    listOfProviders = (from providers in db.tblProviders
                                       join districts in db.tblProviderDistricts
                                            on providers.ProviderID equals districts.ProviderID
                                       where listOfUSD.Contains(districts.USD)
                                       select new ProviderViewModel
                                       {
                                           ProviderID = providers.ProviderID,
                                           ProviderCode = providers.ProviderCode,
                                           FirstName = providers.FirstName,
                                           LastName = providers.LastName
                                       }
                                       ).Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

                    return Json(new { Result = "success", id = newProvider.ProviderID, errors = "", providerList = listOfProviders }, JsonRequestBehavior.AllowGet);
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

                var selectedDistricts = collection["selectedDistrict[]"].Split(',').Distinct().ToArray();
                var selectedBuildings = collection["selectedBuilding[]"].Split(',');

                for (int i = 0; i < selectedDistricts.Length; i++)
                {

                    foreach (var selectedBuilding in selectedBuildings)
                    {
                        var districtUSD = selectedDistricts[i].ToString();
                        var calendarExists = db.tblCalendars.Count(c => c.USD == districtUSD && c.BuildingID == selectedBuilding);

                        if (calendarExists == 0)
                        {
                            //if calendar does not exist, first create calendar from template, the update
                            CopyCalendar(districtUSD, selectedBuilding, MIS);
                        }

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
                                querySaveStuff.Parameters.AddWithValue("@BuildingID_Upd", selectedBuilding);
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
                                querySaveMoreStuff.Parameters.AddWithValue("@BuildingID_Upd", selectedBuilding);
                                querySaveMoreStuff.ExecuteNonQuery();
                            }
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

            ViewBag.modifiedByFullName = string.Empty;
            ViewBag.studentName = student.FirstName + " " + student.LastName;
            var iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            var isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

            try
            {
                tblUser modifier = db.tblUsers.FirstOrDefault();
                switch (view)
                {
                    case "HealthModule":
                        tblIEPHealth healthModel = db.tblIEPHealths.Where(h => h.IEPHealthID == iep.IEPHealthID).FirstOrDefault();
                        if (healthModel == null)
                        {
                            healthModel = new tblIEPHealth();
                        }
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == healthModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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

                        if (academicModel.Academic == null) { academicModel.Academic = new tblIEPAcademic(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Academic.ModifiedBy).SingleOrDefault(); ViewBag.academicModifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Reading == null) { academicModel.Reading = new tblIEPReading(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Reading.ModifiedBy).SingleOrDefault(); ViewBag.readingModifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Math == null) { academicModel.Math = new tblIEPMath(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Math.ModifiedBy).SingleOrDefault(); ViewBag.mathModifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Written == null) { academicModel.Written = new tblIEPWritten(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Written.ModifiedBy).SingleOrDefault(); ViewBag.writtenModifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }

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
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == motorModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == communicationModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == socialModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == intelligenceModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
                                  }).Distinct().OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
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
                tblIEP theIEP = db.tblIEPs.Where(i => i.UserID == stid && i.IsActive && i.IepStatus != IEPStatus.ARCHIVE).FirstOrDefault();
                if (theIEP != null)
                {
                    return RedirectToAction("StudentProcedures", new { stid, theIEP.IEPid });
                }
                else
                {
                    new IEP(student.UserID, null, 1);

                    theIEP = db.tblIEPs.Where(i => i.UserID == stid).OrderByDescending(o => o.IEPid).FirstOrDefault();
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
                model.isDoc = district.DOC;
                model.isGiftedOnly = info.isGifted && info.Primary_DisabilityCode == "ND" && info.Secondary_DisabilityCode == "ND";

                IEP theIEP = (iepID != null) ? new IEP(student.UserID, iepID) : new IEP(student.UserID);
                if (theIEP.current != null)
                {
                    model.hasplan = theIEP.hasPlan;
                    model.studentIEP = theIEP;
                    model.studentPlan = new StudentPlan(student.UserID, iepID);
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

				model.studentAge = theIEP.GetCalculatedAge(info.DateOfBirth, model.isDoc);
				
				//need to check if transition plan is required and completed
				if ((model.studentAge > 13 || (model.isDoc && model.studentAge <= 21))  && !model.isGiftedOnly && (theIEP.iepStatusType == "DRAFT" || theIEP.iepStatusType == "AMENDMENT"))
				{
					if (theIEP.isTransitionCompleted == false && theIEP.isAllCompleted)
					{
						//transition plan must be completed
						theIEP.isAllCompleted = false;
					}
				}
			}

            switch (model.studentIEP.iepStatusType)
            {
                case IEPStatus.PLAN:
                    return View(model); //PLAN
                case IEPStatus.ACTIVE:
                    return View("~/Views/Home/ActiveIEP/index.cshtml", model); //ACTIVE
                case IEPStatus.AMENDMENT:
                    return View("~/Views/Home/AmmendmentIEP/index.cshtml", model); //AMMENDMENT
                case IEPStatus.DRAFT:
                    if (model.studentIEP.anyStudentIEPActive && !model.studentIEP.current.Amendment) //ANNUAL
                    {
                        return View("~/Views/Home/AnnualIEP/index.cshtml", model);
                    }
                    return View("~/Views/Home/DraftIEP/index.cshtml", model);   //DRAFT
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

                        if (iep.IepStatus.ToUpper() == IEPStatus.DRAFT && iep.Amendment)
                        {
                            iep.begin_date = meetingDate;
                        }

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

				//iep status code history just in case the teacher changed it
				var studentRec = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();

				if (studentRec != null)
				{
					studentAmmendIEP.StatusCode = studentRec.StatusCode;
				}
								
				try
                {
                    db.SaveChanges();

					//archive print
					var theIEP = GetIEPPrint(stId, IEPid);

					bool success = ArchiveIEPPrint(stId, theIEP);

					if (!success)
					{
						return Json(new { Result = "error", Message = "There was a problem creating the IEP Archive" }, JsonRequestBehavior.AllowGet);
					}

					return Json(new { Result = "success", Message = "IEP Amendment status changed to Active." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable to change the IEP status." }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize(Roles = mis + "," + admin + "," + teacher)]
        public ActionResult UpdateIEPAnnualToActive(int stId, int IEPid)
        {

            tblIEP studentActiveIEP = db.tblIEPs.Where(i => i.UserID == stId && i.IepStatus == IEPStatus.ACTIVE).FirstOrDefault();
            tblIEP studentAnnualIEP = db.tblIEPs.Where(i => i.UserID == stId && i.IEPid == IEPid).FirstOrDefault();

            if (studentAnnualIEP == null)
                return Json(new { Result = "error", Message = "No annual IEP found for this student." }, JsonRequestBehavior.AllowGet);

            if (studentActiveIEP == null)
            {

                studentAnnualIEP.IepStatus = IEPStatus.ACTIVE;
                studentAnnualIEP.IsActive = true;

                try
                {
					//iep status code history
					var studentDetails = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();
					if (studentDetails != null)
					{
						studentAnnualIEP.StatusCode = studentDetails.StatusCode;
					}

					db.SaveChanges();

					//archive print
					var theIEP = GetIEPPrint(stId, IEPid);

					bool success = ArchiveIEPPrint(stId, theIEP);

					if (!success)
					{
						return Json(new { Result = "error", Message = "There was a problem creating the IEP Archive" }, JsonRequestBehavior.AllowGet);
					}


					return Json(new { Result = "success", Message = "The IEP status is Active." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                // find the current active iep and make it inactive and change its status to DELETED
                studentActiveIEP.IepStatus = IEPStatus.ARCHIVE;
                studentActiveIEP.IsActive = false;

                studentAnnualIEP.IepStatus = IEPStatus.ACTIVE;
                studentAnnualIEP.IsActive = true;

                try
                {
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The IEP status is Active." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
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
					//iep status code history just in case the teacher changed it
					var studentRec = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();
					
					if (studentRec != null)
					{
						iepDraft.StatusCode = studentRec.StatusCode;												
					}

					// start switching the flag.
					iepDraft.IepStatus = IEPStatus.ACTIVE;

                    //iepDraft.begin_date = DateTime.Now;
                    iepDraft.end_Date = (!iepDraft.Amendment) ? iepDraft.begin_date.Value.AddYears(1) : iepDraft.end_Date;

                    try
                    {						

						db.SaveChanges();

						//archive print
						var theIEP = GetIEPPrint(stId, IEPid);

						bool success = ArchiveIEPPrint(stId, theIEP);

						if (!success)
						{
							return Json(new { Result = "error", Message = "There was a problem creating the IEP Archive" }, JsonRequestBehavior.AllowGet);
						}

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
            tblIEP studentIEP = db.tblIEPs.Where(i => i.UserID == Stid && i.IEPid == IepId).FirstOrDefault();
            if (studentIEP != null)
            {
                studentIEP.IsActive = false;
                if (studentIEP.AmendingIEPid != null)
                {
                    studentIEP.IepStatus = IEPStatus.ARCHIVE;
                }
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
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Academic").FirstOrDefault().NeedMetByGoal == 1 ? "Academic/Functional" : string.Empty;

                var modulesList = db.tblModules.OrderBy(o => o.ModuleName).Select(o => o.ModuleID.ToString()).ToList();
                List<tblGoal> goals = db.tblGoals.Where(g => g.IEPid == iep.IEPid).ToList().OrderBy(d => modulesList.IndexOf(d.Module)).ToList();
                int? modifiedby = (goals.Count > 0) ? goals.FirstOrDefault().ModifiedBy : null;
                if (modifiedby != null)
                {
                    tblUser modifier = db.tblUsers.Where(u => u.UserID == modifiedby).SingleOrDefault();
                    ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                    ViewBag.modifiedByDate = goals.FirstOrDefault().Update_Date;
                }

                foreach (tblGoal goal in goals)
                {
                    var studentGoal = new StudentGoal(goal.goalID);

                    model.studentGoals.Add(studentGoal);
                    var benchmarks = db.tblGoalBenchmarks.Where(o => o.goalID == goal.goalID);

                    foreach (tblGoalBenchmark benchmark in benchmarks)
                    {
                        var shortBenchmarks = db.tblGoalBenchmarkMethods.Where(o => o.goalBenchmarkID == benchmark.goalBenchmarkID).ToList();
                        studentGoal.shortTermBenchmarkMethods.AddRange(shortBenchmarks);
                    }

                }

                if (!isReadOnly)
                    return PartialView("_ModuleStudentGoals", model);
                else
                    return PartialView("ActiveIEP/_StudentGoals", model);
            }

            return PartialView("_ModuleStudentGoals", new StudentGoalsViewModel());
        }

        [HttpGet]
        [Authorize]
        public ActionResult DuplicateStudentServicesNextYear(int studentId, int? serviceId, int iepId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
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
                    tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    int startMonth = 7; //july
                    int endMonth = 6; //june

                    List<tblService> services = null;
                    if (serviceId.HasValue)
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear && s.ServiceID == serviceId).ToList();
                    else
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear).ToList();

                    List<StudentServiceObject> serviceList = new List<StudentServiceObject>();
                    foreach (var service in services)
                    {
                        List<tblCalendar> availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == service.BuildingID && c.canHaveClass == true && c.NoService == false && c.SchoolYear > service.SchoolYear && c.SchoolYear <= maxYear).OrderBy(c => c.SchoolYear).ThenBy(c => c.Month).ThenBy(c => c.Day).ToList();

                        tblCalendar firstDaySchoolYear = availableCalendarDays.Where(c => c.Month >= startMonth).OrderBy(c => c.Month).ThenBy(c => c.Day).First();
                        tblCalendar lastDaySchoolYear = availableCalendarDays.Where(c => c.Month <= endMonth).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

                        var item = new StudentServiceObject();
                        var meetingDate = item.DaysPerWeek = service.DaysPerWeek;
                        item.StartDate = firstDaySchoolYear != null && firstDaySchoolYear.calendarDate.HasValue ? firstDaySchoolYear.calendarDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();

                        if (iep.MeetingDate.HasValue && (iep.MeetingDate.Value > lastDaySchoolYear.calendarDate))
                        {
                            item.EndDate = iep.MeetingDate.Value.ToShortDateString();
                        }
                        else
                        {
                            item.EndDate = lastDaySchoolYear.calendarDate.Value.ToShortDateString();
                        }

                        item.LocationCode = service.LocationCode;
                        item.Minutes = service.Minutes;
                        item.ProviderID = service.ProviderID.HasValue ? service.ProviderID.Value : -1;
                        item.SchoolYear = service.SchoolYear;
                        item.ServiceCode = service.ServiceCode;
                        item.Frequency = service.Frequency;
                        item.selectedAttendingBuilding = service.BuildingID;
						item.ProvidedFor = service.ProvidedFor;


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
                return Json(new { Result = "error", Message = "The calendar for this district has not been created. Please create the calendar before you proceed." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "success", Message = "Nicely Done" }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize]
        public ActionResult StudentServices(int studentId, int IEPid)
        {
            bool isReadOnly = false;

            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name); // current teacher-esque user.
            tblUser mis = FindSupervisor.GetUSersMIS(teacher); // get the mis of the teacher
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).First(); // gimme the student's iep.
            tblIEP original = (iep.OriginalIEPid.HasValue) ? db.tblIEPs.Where(i => i.OriginalIEPid == iep.OriginalIEPid.Value).FirstOrDefault() : iep;

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse) ? true : false;

                StudentServiceViewModel model = new StudentServiceViewModel();
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
                tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

                ViewBag.studentName = student.FirstName + " " + student.LastName;
                ViewBag.isMIS = mis.UserID == teacher.UserID;

                var providers = (from p in db.tblProviders
                                 join d in db.tblProviderDistricts on p.ProviderID equals d.ProviderID
                                 where d.USD != null && d.USD == studentInfo.AssignedUSD
                                 select p).OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

                List<tblService> services = db.tblServices.Where(s => s.IEPid == iep.IEPid).ToList();

                if (services != null)
                {
                    model.studentId = studentId;
                    model.studentServices = services;
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = providers;
                    model.serviceLocations = db.tblLocations.ToList();
                    model.attendanceBuildings = db.vw_BuildingsForAttendance.Where(b => b.userID == student.UserID).Distinct().ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.IEPStartDate = original.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                    model.isOriginalIEPService = iep.IepStatus.ToUpper() == IEPStatus.DRAFT && iep.Amendment;

                    int? modifiedby = (services.Count > 0) ? services.FirstOrDefault().ModifiedBy : null;
                    if (modifiedby != null)
                    {
                        tblUser modifier = db.tblUsers.Where(u => u.UserID == modifiedby).SingleOrDefault();
                        ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        ViewBag.modifiedByDate = services.FirstOrDefault().Update_Date;
                    }
                }
                else
                {
                    model.studentId = studentId;
                    model.studentServices.Add(new tblService() { IEPid = iep.IEPid });
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = db.tblProviders.Where(p => p.UserID == mis.UserID).OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();
                    model.serviceLocations = db.tblLocations.ToList();
                    model.attendanceBuildings = db.vw_BuildingsForAttendance.Where(b => b.userID == student.UserID).Distinct().ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
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

        [HttpGet]
        [Authorize]
        public ActionResult BuildingHasCalendars(int UserID, string BuildingID)
        {
            int lastYear = DateTime.Now.AddYears(-1).Year;
            int thirdYear = DateTime.Now.AddYears(2).Year;
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == UserID).FirstOrDefault();
            List<string> attendingDistrict = studentInfo.USD.Split(',').ToList();

            if (studentInfo != null)
            {
                List<tblCalendar> Calendar = db.tblCalendars.Where(c => c.BuildingID == BuildingID && attendingDistrict.Contains(c.USD) && c.Year >= lastYear && c.Year <= thirdYear).OrderBy(c => c.Year).ToList();
                if (Calendar.Count > 0)
                {
                    JsonResult Holidays = Json(Calendar.Where(c => c.NoService || !c.canHaveClass).Select(c => c.calendarDate.Value.ToString("d-M-yyyy")).ToList(), JsonRequestBehavior.AllowGet);
                    List<int> calendarYears = Calendar.Select(c => c.Year).Distinct().ToList();
                    tblCalendar isPossibleLastFiscalDay = Calendar.Where(c => c.canHaveClass && c.Year == DateTime.Now.Year && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();
                    tblCalendar LastFiscalDayofYear = (isPossibleLastFiscalDay.calendarDate > DateTime.Now) ? isPossibleLastFiscalDay : Calendar.Where(c => c.canHaveClass && c.Year == DateTime.Now.AddYears(1).Year && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

                    return Json(new { success = true, buildingCalendar = Calendar, holidays = Holidays, buildingFiscalYears = calendarYears, lastFiscalDayofYear = LastFiscalDayofYear }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, buildingCalendar = 0, holidays = 0, buildingFiscalYears = 0 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ValidateServiceDate(int fiscalYear, string calendarDay, string buildingId)
        {
            bool isValid = false;
            bool isService = true;
            string validDates = "";
            IsValidDate(fiscalYear, calendarDay, buildingId, out isValid, out isService, out validDates);

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

        private void IsValidDate(int fiscalYear, string calendarDay, string buildingId, out bool isValid, out bool isService, out string validDates)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser mis = FindSupervisor.GetUSersMIS(teacher);

            int startMonth = 7; //july
            int endMonth = 6; //june

            DateTime searchDate = Convert.ToDateTime(calendarDay);
            isValid = false;
            isService = true;
            validDates = "";

            //start date must be within the school year
            var availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == buildingId && (c.canHaveClass == true && c.NoService == false) && c.SchoolYear == fiscalYear);

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
            int iepId = Convert.ToInt32(collection["iepId"]);

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
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            if (iep != null)
            {
                int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                if (StudentSerivceId == 0) // new service
                {
                    service = new tblService();
                    service.IEPid = iep.IEPid;
                    service.BuildingID = collection["attendanceBuilding"].ToString();
                    service.USD = db.vw_BuildingsForAttendance.Where(b => b.BuildingID == service.BuildingID && b.userID == studentId).FirstOrDefault().USD;
                    service.SchoolYear = Convert.ToInt32(collection["fiscalYear"]);
                    service.StartDate = DateTime.TryParse((collection["serviceStartDate"]), out temp) ? temp : DateTime.Now;
                    service.EndDate = DateTime.TryParse((collection["serviceEndDate"]), out temp) ? temp : DateTime.Now;
                    service.ServiceCode = collection["ServiceType"].ToString();
                    service.Frequency = Convert.ToInt32(collection["Frequency"]);
                    service.DaysPerWeek = Convert.ToByte(collection["serviceDaysPerWeek"]);
                    service.Minutes = Convert.ToInt16(collection["serviceMinutesPerDay"]);
                    service.ProviderID = Convert.ToInt32(collection["serviceProvider"]);
                    service.LocationCode = collection["location"];
                    service.ProvidedFor = collection["serviceProvidedFor"];
                    service.Completed = isCompleted;
                    service.Create_Date = DateTime.Now;
                    service.Update_Date = DateTime.Now;
                    service.ModifiedBy = ModifiedBy;

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
                    IsValidDate(service.SchoolYear, service.StartDate.ToShortDateString(), service.BuildingID, out isValidStartDate, out isValidServiceStartDate, out validDates);
                    IsValidDate(service.SchoolYear, service.EndDate.ToShortDateString(), service.BuildingID, out isValidEndDate, out isValidServiceEndDate, out validDates);
                }
                else // exsisting service
                {
                    service = db.tblServices.Where(s => s.ServiceID == StudentSerivceId).FirstOrDefault();
                    service.BuildingID = collection["attendanceBuilding"].ToString();
                    service.SchoolYear = Convert.ToInt32(collection["fiscalYear"]);
                    service.StartDate = DateTime.TryParse((collection["serviceStartDate"]), out temp) ? temp : DateTime.Now;
                    service.EndDate = DateTime.TryParse((collection["serviceEndDate"]), out temp) ? temp : DateTime.Now;
                    service.ServiceCode = collection["ServiceType"].ToString();
                    service.Frequency = Convert.ToInt32(collection["Frequency"]);
                    service.DaysPerWeek = Convert.ToByte(collection["serviceDaysPerWeek"]);
                    service.Minutes = Convert.ToInt16(collection["serviceMinutesPerDay"]);
                    service.ProviderID = Convert.ToInt32(collection["serviceProvider"]);
                    service.LocationCode = collection["location"];
                    service.ProvidedFor = collection["serviceProvidedFor"];
                    service.Update_Date = DateTime.Now;
                    service.Completed = isCompleted;
                    service.ModifiedBy = ModifiedBy;
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
                    IsValidDate(service.SchoolYear, service.StartDate.ToShortDateString(), service.BuildingID, out isValidStartDate, out isValidServiceStartDate, out validDates);
                    IsValidDate(service.SchoolYear, service.EndDate.ToShortDateString(), service.BuildingID, out isValidEndDate, out isValidServiceEndDate, out validDates);
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
        public ActionResult GetLastFiscalDay(int studentId, string fyYear)
        {
            var lastDay = GetLastFiscalCalendarDay(studentId, fyYear);

            if (lastDay != null)
                return Json(new { Result = "success", Value = lastDay.calendarDate.Value.ToString("MM/dd/yyyy") }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { Result = "success", Value = "" }, JsonRequestBehavior.AllowGet);
        }

        private tblCalendar GetLastFiscalCalendarDay(int studentId, string fyYear)
        {
            tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

            int lastYear = 0;
            Int32.TryParse(fyYear, out lastYear);

            List<tblCalendar> calendar = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.USD && c.Year == lastYear).OrderBy(c => c.Year).ToList();

            var lastDay = calendar.Where(c => c.canHaveClass && c.Year == lastYear && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

            return lastDay;

        }

        [HttpGet]
        [Authorize]
        public ActionResult StudentTransition(int studentId, int IEPid)
        {
			IEP theIEP = new IEP(studentId, IEPid);
			tblIEP iep = theIEP.current; //db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
			

			bool isReadOnly = false;
            if (iep != null)
            {
                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();

                string studentFirstName = string.Format("{0}", student.FirstName);
                string studentLastName = string.Format("{0}", student.LastName);
				
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
								
				int studentAge = theIEP.GetCalculatedAge(info.DateOfBirth, model.isDOC);

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
            model.ModuleList = db.tblModules.Where(o => o.Active == true).ToList();

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

                model.StudentId = studentId;
                model.IEPid = iep.IEPid;

                List<vw_ModuleAccommodationFlags> accommodationFlag = db.vw_ModuleAccommodationFlags.Where(vm => vm.IEPid == iep.IEPid).ToList();
                model.modulesNeedingAccommodations = accommodationFlag.Where(vm => vm.Module == "Health").FirstOrDefault().NeedMetByAccommodation ? "Health " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Motor").FirstOrDefault().NeedMetByAccommodation ? "Motor " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Communication").FirstOrDefault().NeedMetByAccommodation ? "Communication " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Social").FirstOrDefault().NeedMetByAccommodation ? "Social-Emotional " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Math").FirstOrDefault().NeedMetByAccommodation ? "Math " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Reading").FirstOrDefault().NeedMetByAccommodation ? "Reading " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Written").FirstOrDefault().NeedMetByAccommodation ? "Written-Language " : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Academic").FirstOrDefault().NeedMetByAccommodation ? "Academic/Functional" : string.Empty;

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
            tblOtherConsideration model = new tblOtherConsideration();
            bool isReadOnly = false;
            ViewBag.vehicleType = 0;
            ViewBag.minutes = "25";
            ViewBag.begin = "";
            ViewBag.end = "";

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            if (iep != null)
            {
                tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse) ? true : false;

                model.IEPid = iep.IEPid;
                var oc = db.tblOtherConsiderations.Where(i => i.IEPid == iep.IEPid);
                if (oc.Any())
                {
                    model = oc.FirstOrDefault();

                    // Load the modified by info
                    tblUser modifier = db.tblUsers.Where(u => u.UserID == model.ModifiedBy).SingleOrDefault();
                    ViewBag.modifiedByFullName = (modifier != null) ? String.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                }
                else
                {
                    //default value
                    model.DistrictAssessment_GradeNotAssessed = true;
                    model.StateAssessment_RequiredCompleted = true;
                    model.Parental_CopyIEP_flag = true;
                    model.Parental_RightsBook_flag = true;

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
            int iepId = Convert.ToInt32(collection["iepId"]);

            StudentPlan thePlan = new StudentPlan(studentId);

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
                foreach (var key in collection.AllKeys.Skip(3))
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

                thePlan.Update(studentId, iepId);
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
            tblIEP iep = db.tblIEPs.Where(u => u.UserID == id).FirstOrDefault();

            var forms = GetForms();

            var form = forms.Where(o => o.Value == fileName).FirstOrDefault();
            if (form != null)
            {
                viewModel.fileDesc = form.Text;
            }

            StudentLegalView fileViewModel = new StudentLegalView()
            {
                student = student,
                teacher = teacher,
                studentInfo = db.tblStudentInfoes.Where(u => u.UserID == student.UserID).FirstOrDefault(),
                contacts = db.tblStudentRelationships.Where(u => u.UserID == student.UserID).ToList(),
                studentTransition = iep != null ? db.tblTransitions.Where(u => u.IEPid == iep.IEPid).FirstOrDefault() : new tblTransition(),
                transitionGoals = iep != null ? db.tblTransitionGoals.Where(u => u.IEPid == iep.IEPid).ToList() : new List<tblTransitionGoal>(),
                academicGoals = iep != null ? db.tblIEPAcademics.Where(u => u.IEPid == iep.IEPid).FirstOrDefault() : new tblIEPAcademic(),
                socialGoals = iep != null ? db.tblIEPSocials.Where(u => u.IEPid == iep.IEPid).FirstOrDefault() : new tblIEPSocial(),
                reading = iep != null ? db.tblIEPReadings.Where(r => r.IEPReadingID == iep.IEPReadingID).FirstOrDefault() : new tblIEPReading(),
                math = iep != null ? db.tblIEPMaths.Where(m => m.IEPMathID == iep.IEPMathID).FirstOrDefault() : new tblIEPMath(),
                written = iep != null ? db.tblIEPWrittens.Where(w => w.IEPWrittenID == iep.IEPWrittenID).FirstOrDefault() : new tblIEPWritten()
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

                fileViewModel.studentLanguage = GetLanguage(fileViewModel.studentInfo.StudentLanguage);


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

            if (fileName == "TeamEvaluation")
            {
                viewModel.teamEval = db.tblFormTeamEvals.Where(o => o.StudentId == id).FirstOrDefault();
            }
            else if (fileName == "SummaryOfPerformance")
            {
                viewModel.summaryPerformance = db.tblFormSummaryPerformances.Where(o => o.StudentId == id).FirstOrDefault();
            }
			else if (fileName == "ConferenceSummary")
			{
				viewModel.conferenceSummary = db.tblFormConferenceSummaries.Where(o => o.StudentId == id).FirstOrDefault();
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
            //forms.Add(new SelectListItem { Text = "Summary of Performance Example", Value = "SOPExample" });
            forms.Add(new SelectListItem { Text = "IEP Team Considerations", Value = "IEPTeamConsider" });
            forms.Add(new SelectListItem { Text = "Parent Consent for Release of Information and Medicaid Reimbursement", Value = "ParentConsentMedicaid" });
            forms.Add(new SelectListItem { Text = "Physician Script", Value = "PhysicianScript" });
            forms.Add(new SelectListItem { Text = "Team Evaluation Report", Value = "TeamEvaluation" });
            forms.Add(new SelectListItem { Text = "Conference Summary", Value = "ConferenceSummary" });
            forms.Add(new SelectListItem { Text = "Summary Of Performance", Value = "SummaryOfPerformance" });



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
                theIEP.studentDetails.printStudentInfo = true;
                theIEP.studentDetails.printIEPDetails = true;
                theIEP.studentDetails.printHealth = true;
                theIEP.studentDetails.printMotor = true;
                theIEP.studentDetails.printComm = true;
                theIEP.studentDetails.printSocial = true;
                theIEP.studentDetails.printGeneral = true;
                theIEP.studentDetails.printAcademic = true;
                theIEP.studentDetails.printAcc = true;
                theIEP.studentDetails.printBehavior = true;
                theIEP.studentDetails.printTrans = true;
                theIEP.studentDetails.printOther = true;
                theIEP.studentDetails.printGoals = true;
                theIEP.studentDetails.printServices = true;
                theIEP.studentDetails.printNotice = true;
                theIEP.studentDetails.printProgressReport = false;

                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

		[HttpGet]
		[Authorize]
		public ActionResult PrintIEPSection(int stid, int iepId, string section, string goalsToPrint)
        {
            var theIEP = GetIEPPrint(stid, iepId);
            if (theIEP != null)
            {
                switch (section)
                {
                    case "Health":
                        {
                            theIEP.studentDetails.printHealth = true;
                            break;
                        }

                    case "Motor":
                        {
                            theIEP.studentDetails.printMotor = true;
                            break;
                        }
                    case "Comm":
                        {
                            theIEP.studentDetails.printComm = true;
                            break;
                        }
                    case "Social":
                        {
                            theIEP.studentDetails.printSocial = true;
                            break;
                        }
                    case "General":
                        {
                            theIEP.studentDetails.printGeneral = true;
                            break;
                        }
                    case "Academic":
                        {
                            theIEP.studentDetails.printAcademic = true;
                            break;
                        }
                    case "Acc":
                        {
                            theIEP.studentDetails.printAcc = true;
                            break;
                        }
                    case "Behavior":
                        {
                            theIEP.studentDetails.printBehavior = true;
                            break;
                        }
                    case "Trans":
                        {
                            theIEP.studentDetails.printTrans = true;
                            break;
                        }

                    case "Other":
                        {
                            theIEP.studentDetails.printOther = true;
                            break;
                        }
                    case "Goals":
                        {
                            theIEP.studentDetails.printGoals = true;
                            break;
                        }
                    case "Services":
                        {
                            theIEP.studentDetails.printServices = true;
                            break;
                        }
                    case "Progress":
                        {
							if(!string.IsNullOrEmpty(goalsToPrint))
								theIEP.studentDetails.printProgressGoals = goalsToPrint.Split(',').Select(Int32.Parse).ToList();

							theIEP.studentDetails.printProgressReport = true;
                            break;
                        }


                }

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
                theIEP.studentDetails.printStudentInfo = true;
                theIEP.studentDetails.printIEPDetails = true;
                theIEP.studentDetails.printHealth = true;
                theIEP.studentDetails.printMotor = true;
                theIEP.studentDetails.printComm = true;
                theIEP.studentDetails.printSocial = true;
                theIEP.studentDetails.printGeneral = true;
                theIEP.studentDetails.printAcademic = true;
                theIEP.studentDetails.printAcc = true;
                theIEP.studentDetails.printBehavior = true;
                theIEP.studentDetails.printTrans = true;
                theIEP.studentDetails.printOther = true;
                theIEP.studentDetails.printGoals = true;
                theIEP.studentDetails.printServices = true;
                theIEP.studentDetails.printNotice = true;
                theIEP.studentDetails.printProgressReport = false;

                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        private IEP GetIEPPrint(int stid, int iepId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(u => u.UserID == stid);

            var studentDetails = new StudentDetailsPrintViewModel();

            List<tblStudentRelationship> contacts = db.tblStudentRelationships.Where(i => i.UserID == stid).ToList();

            tblUser mis = FindSupervisor.GetUSersMIS(teacher);

            IEP theIEP = new IEP(student.UserID, iepId)
            {
                locations = db.tblLocations.ToList(),
                serviceTypes = db.tblServiceTypes.ToList(),
                serviceProviders = db.tblProviders.Where(p => p.UserID == mis.UserID).ToList(),
                studentFirstName = string.IsNullOrEmpty(student.MiddleName) ? string.Format("{0}", student.FirstName) : string.Format("{0} {1}", student.FirstName, student.MiddleName),
                studentLastName = string.Format("{0}", student.LastName),
            };

            //student goalds
            if (theIEP != null && theIEP.current != null)
            {
                if (theIEP.studentGoalBenchmarks == null)
                {
                    theIEP.studentGoalBenchmarks = new List<tblGoalBenchmark>();
                }

                if (theIEP.studentGoalBenchmarkMethods == null)
                {
                    theIEP.studentGoalBenchmarkMethods = new List<tblGoalBenchmarkMethod>();

                }

                if (theIEP.studentGoalEvalProcs == null)
                {
                    theIEP.studentGoalEvalProcs = new List<tblGoalEvaluationProcedure>();

                }

                foreach (var goal in theIEP.studentGoals)
                {
                    theIEP.studentGoalBenchmarks.AddRange(db.tblGoalBenchmarks.Where(g => g.goalID == goal.goalID).ToList());

                    theIEP.studentGoalEvalProcs.AddRange(db.tblGoalEvaluationProcedures.Where(g => g.goalID == goal.goalID).ToList());

                    if (theIEP.studentGoalBenchmarks.Any())
                    {
                        var benchmarkIds = theIEP.studentGoalBenchmarks.Select(o => o.goalBenchmarkID).ToList();
                        theIEP.studentGoalBenchmarkMethods.AddRange(db.tblGoalBenchmarkMethods.Where(g => benchmarkIds.Contains(g.goalBenchmarkID)).ToList());
                    }

                }


                var studentBehavior = db.tblBehaviors.Where(g => g.IEPid == theIEP.current.IEPid).FirstOrDefault();
                theIEP.studentBehavior = GetBehaviorModel(student.UserID, theIEP.current.IEPid);

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

                    bool isDOC = false;
                    if (theIEP.studentTransition != null)
                    {
                        isDOC = theIEP.studentTransition.isDOC;
                    }

					if (theIEP.current.begin_date != null && !isDOC)
					{
						//check student age for transition plan using the begin date plus one year
						var endDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.AddYears(1) : theIEP.current.begin_date.Value.AddYears(1);
						theIEP.studentAge = (endDate.Year - info.DateOfBirth.Year - 1) + (((endDate.Month > info.DateOfBirth.Month) || ((endDate.Month == info.DateOfBirth.Month) && (endDate.Day >= info.DateOfBirth.Day))) ? 1 : 0);
					}
					else
					{
						//use current date
						theIEP.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
					}


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
					int studentAgeAtIEP = 0;


					if (theIEP.iepStartTime.HasValue)
					{
						var iepDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value : theIEP.current.begin_date.Value;
						studentAgeAtIEP =(iepDate.Year - info.DateOfBirth.Year - 1) + (((iepDate.Month > info.DateOfBirth.Month) || ((iepDate.Month == info.DateOfBirth.Month) && (iepDate.Day >= info.DateOfBirth.Day))) ? 1 : 0);

					}
					

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
					studentDetails.studentAgeAtIEP = studentAgeAtIEP;
					studentDetails.studentAgeAtAnnualMeeting = (theIEP.current.MeetingDate.HasValue ? (theIEP.current.MeetingDate.Value.Year - info.DateOfBirth.Year - 1) + (((theIEP.current.MeetingDate.Value.Month > info.DateOfBirth.Month) || ((theIEP.current.MeetingDate.Value.Month == info.DateOfBirth.Month) && (theIEP.current.MeetingDate.Value.Day >= info.DateOfBirth.Day))) ? 1 : 0) : 0);
					studentDetails.assignChildCount = studentUSD != null ? studentUSD.DistrictName : "";
					studentDetails.placementCodeDesc = info != null ? db.tblPlacementCodes.Where(c => c.PlacementCode == info.PlacementCode).FirstOrDefault().PlacementDescription : "";
					studentDetails.edStatusCodeDesc = info != null && db.tblStatusCodes.Where(c => c.StatusCode == info.StatusCode).Any() ? db.tblStatusCodes.Where(c => c.StatusCode == info.StatusCode).FirstOrDefault().Description : "";
					studentDetails.reevalDates = db.tblArchiveEvaluationDates.Where(c => c.userID == stid).OrderByDescending(o => o.evalutationDate).ToList();

                    studentDetails.inititationDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.ToShortDateString() : "";
                    studentDetails.inititationDateNext = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.AddYears(1).ToShortDateString() : "";

					var historicalIEPs = db.tblIEPs.Where(o => o.UserID == info.UserID && (o.IepStatus == IEPStatus.ARCHIVE || o.IepStatus == IEPStatus.ACTIVE)).OrderByDescending(o => o.begin_date);
					var originalIEP = historicalIEPs.Where(o => o.OriginalIEPid == null && o.Amendment == false).Take(1).FirstOrDefault();
					var historicalIEPList = new List<IEPHistoryViewModel>();					
					int firstIEPId = originalIEP != null ? originalIEP.IEPid : 0;

					if (theIEP.current.IepStatus.ToUpper() == IEPStatus.DRAFT)
					{
						//add draft to history
						var iepType = string.Format("{0}", theIEP.anyStudentIEPActive && !theIEP.current.Amendment ? "Annual" : theIEP.anyStudentIEPActive && theIEP.current.Amendment ? "Amendment" : string.Empty);
						var historyItem = new IEPHistoryViewModel() { edStatus = theIEP.current.StatusCode, iepDate = theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : "", iepType = iepType };
						historicalIEPList.Add(historyItem);
					}					

					foreach (var history in historicalIEPs)
					{
						
						var historyItem = new IEPHistoryViewModel();

						if (history.OriginalIEPid == null)
						{						
							historyItem.iepType = "Annual";
						}
						else
						{
							if (history.Amendment)
							{						
								historyItem.iepType = "Amendment";
							}
						}

						historyItem.edStatus = string.IsNullOrEmpty(history.StatusCode) ? studentDetails.student.StatusCode : history.StatusCode;
						historyItem.iepDate = history.begin_date.HasValue ? history.begin_date.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : "";
						historicalIEPList.Add(historyItem);
					}


					if (studentDetails.student.ExitDate.HasValue)
					{
						var exitItem = new IEPHistoryViewModel();
						exitItem.iepType = "Exit";
						exitItem.iepDate = studentDetails.student.ExitDate.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture); 
						exitItem.edStatus = "D";
						historicalIEPList.Add(exitItem);
					}

					ViewBag.History = historicalIEPList;
                }

                theIEP.studentDetails = studentDetails;

                return theIEP;
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

            return RedirectToAction("Portal", "Home");

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

        [HttpPost]
        public ActionResult SearchUserName(string username)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (user != null && (!string.IsNullOrEmpty(username)))
            {
                string usernameVal = username.Trim();

                if (user.RoleID == owner)
                {
                    var districts = (from district in db.tblDistricts select district).Distinct().ToList();
                    var buildings = (from building in db.tblBuildings select building).Distinct().ToList();

                    var filterUsers = db.vw_UserList.Where(ul => ul.RoleID != owner)
                        .Where(o => o.LastName.Contains(usernameVal) || o.FirstName.Contains(usernameVal) || o.MiddleName.Contains(usernameVal)).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false })
                        .Distinct()
                        .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

                    return Json(new { result = true, filterUsers = filterUsers }, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == user.UserID select district).Distinct().ToList();
                    var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == user.UserID select building).Distinct().ToList();

                    List<String> myDistricts = districts.Select(d => d.USD).ToList();
                    List<String> myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");

                    var filterUsers = db.vw_UserList.Where(ul => (ul.RoleID == admin || ul.RoleID == teacher || ul.RoleID == student || ul.RoleID == nurse) && (myBuildings.Contains(ul.BuildingID) && myDistricts.Contains(ul.USD))).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false })
                        .Where(o => o.LastName.Contains(usernameVal) || o.FirstName.Contains(usernameVal) || o.MiddleName.Contains(usernameVal))
                        .Distinct()
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToList().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

                    return Json(new { result = true, filterUsers = filterUsers }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SpedProReport()
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            var canReset = (user != null && (user.RoleID == owner || user.RoleID == mis)) ? true : false;
            ViewBag.canReset = canReset;
            return View("~/Reports/SpedPro/Index.cshtml");
        }

        [HttpPost]
        public ActionResult GetSpedProStudentList(int fiscalYear)
        {
            string iepStatus = IEPStatus.ACTIVE;
            List<tblUser> studentsList = new List<tblUser>();
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (MIS != null)
            {
                var canReset = (MIS != null && (MIS.RoleID == owner || MIS.RoleID == mis)) ? true : false;
                var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList();
                List<String> myBuildings = buildings.Select(b => b.BuildingID).ToList();

                var query = (from iep in db.tblIEPs
                             join student in db.tblUsers
                                 on iep.UserID equals student.UserID
                             join services in db.tblServices
                                 on iep.IEPid equals services.IEPid
                             join building in db.tblBuildingMappings
                                 on student.UserID equals building.UserID
                             where
                             iep.IepStatus == iepStatus
                             && (student.Archive == null || student.Archive == false)
                             && services.SchoolYear == fiscalYear
                             && (iep.FiledOn != null)
                             && services.ServiceCode != "NS"
                             && myBuildings.Contains(building.BuildingID)
                             select new { iep, student }).Distinct().OrderBy(o => o.student.LastName).ThenBy(o => o.student.FirstName).ToList();

                if (query.Count() > 0)
                {
                    studentsList.AddRange(query.Select(o => o.student));
                }
            }

            return Json(new { result = true, students = studentsList }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult DownloadSpedPro(FormCollection collection)
        {
            bool isReset = !string.IsNullOrEmpty(collection["cbReset"]) ? true : false;
            string fiscalYearStr = collection["fiscalYear"];
            int fiscalYear = 0;
            Int32.TryParse(fiscalYearStr, out fiscalYear);
            string studentResetList = collection["studentReset"];

            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                var canReset = (MIS != null && (MIS.RoleID == owner || MIS.RoleID == mis)) ? true : false;
                ViewBag.canReset = canReset;

                var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList();
                List<String> myBuildings = buildings.Select(b => b.BuildingID).ToList();

                string iepStatus = IEPStatus.ACTIVE;
                var exportErrors = new List<ExportErrorView>();


                if (isReset && !string.IsNullOrEmpty(studentResetList))
                {

                    try
                    {
                        var userIds = Array.ConvertAll(studentResetList.Split(','), int.Parse).ToList();

                        var resetQuery = (from iep in db.tblIEPs
                                          join student in db.tblUsers
                                              on iep.UserID equals student.UserID
                                          join services in db.tblServices
                                              on iep.IEPid equals services.IEPid
                                          join building in db.tblBuildingMappings
                                              on student.UserID equals building.UserID
                                          where
                                          iep.IepStatus == iepStatus
                                          && (student.Archive == null || student.Archive == false)
                                          && services.SchoolYear == fiscalYear
                                          && (iep.FiledOn != null)
                                          && services.ServiceCode != "NS"
                                          && myBuildings.Contains(building.BuildingID)
                                          && userIds.Contains(student.UserID)
                                          select iep).Distinct();

                        foreach (var item in resetQuery)
                        {
                            item.FiledOn = null;
                        }

                        db.SaveChanges();

                        ViewBag.message = "The IEPs were successfully reset!";
                    }
                    catch (Exception e)
                    {
                        exportErrors.Add(new ExportErrorView()
                        {
                            UserID = "",
                            Description = e.Message
                        });

                        ViewBag.errors = exportErrors;
                    }

                    return View("~/Reports/SpedPro/Index.cshtml");

                }


                var query = (from iep in db.tblIEPs
                             join student in db.tblUsers
                                 on iep.UserID equals student.UserID
                             join services in db.tblServices
                                 on iep.IEPid equals services.IEPid
                             join building in db.tblBuildingMappings
                                 on student.UserID equals building.UserID
                             where
                             iep.IepStatus == iepStatus
                             && (student.Archive == null || student.Archive == false)
                             && services.SchoolYear == fiscalYear
                             && (services.FiledOn == null || iep.FiledOn == null)
                             && services.ServiceCode != "NS"
                             && myBuildings.Contains(building.BuildingID)
                             select new { iep, student }).Distinct().ToList();

                if (query.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    bool checkPrevious = false;

                    foreach (var item in query)
                    {
                        //this is either a new iep or a new annual iep, we will need to resend any services for the fy
                        checkPrevious = item.iep.OriginalIEPid == null ? true : false;

                        IEP theIEP = new IEP()
                        {
                            current = item.iep,
                            studentFirstName = string.Format("{0}", item.student.FirstName),
                            studentLastName = string.Format("{0}", item.student.LastName),
                        };

                        if (theIEP != null && theIEP.current != null)
                        {
                            var studentDetails = new StudentDetailsPrintViewModel();
                            theIEP.studentServices = db.tblServices.Where(g => g.IEPid == theIEP.current.IEPid && g.ServiceCode != "NS" && g.SchoolYear == fiscalYear).ToList(); //exclude servies marked as No Service
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

                        if (checkPrevious)
                        {
                            //check for fy services not included in current IEP

                            var otherIEPs = (from iep in db.tblIEPs
                                             join student in db.tblUsers
                                                 on iep.UserID equals student.UserID
                                             join services in db.tblServices
                                                 on iep.IEPid equals services.IEPid
                                             join building in db.tblBuildingMappings
                                                 on student.UserID equals building.UserID
                                             where
                                             iep.IepStatus == IEPStatus.ARCHIVE
                                             && (student.Archive == null || student.Archive == false)
                                             && services.SchoolYear == fiscalYear
                                             && services.ServiceCode != "NS"
                                             && iep.UserID == item.iep.UserID
                                             && myBuildings.Contains(building.BuildingID)
                                             select iep).Distinct().ToList();

                            //if an iep has been amended, exclude those ieps
                            var excludeIEPS = otherIEPs.Where(o => o.OriginalIEPid != null).Select(o => o.OriginalIEPid.Value).ToList();

                            var otherServices = (from iep in db.tblIEPs
                                                 join student in db.tblUsers
                                                     on iep.UserID equals student.UserID
                                                 join services in db.tblServices
                                                     on iep.IEPid equals services.IEPid
                                                 join building in db.tblBuildingMappings
                                                     on student.UserID equals building.UserID
                                                 where
                                                 iep.IepStatus == IEPStatus.ARCHIVE
                                                 && (student.Archive == null || student.Archive == false)
                                                 && services.SchoolYear == fiscalYear
                                                 && services.ServiceCode != "NS"
                                                 && iep.UserID == item.iep.UserID
                                                 && myBuildings.Contains(building.BuildingID)
                                                 && !excludeIEPS.Contains(iep.IEPid)
                                                 select services).Distinct().ToList();


                            if (theIEP.studentServices != null)
                            {
                                theIEP.studentServices.AddRange(otherServices);
                            }
                            else
                            {
                                theIEP.studentServices = otherServices;
                            }


                        }

                        var errors = CreateSpedProExport(theIEP, fiscalYear, sb);

                        if (errors.Count > 0)
                        {
                            exportErrors.AddRange(errors);

                        }

                    }//end foreach


                    if (exportErrors.Count == 0)
                    {
                        //on save if no errors
                        db.SaveChanges();
                        var byteArray = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                        OutputResponse(byteArray, "SpedProExport.txt", "text/plain");
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
            }

            return View("~/Reports/SpedPro/Index.cshtml");
        }

        private List<ExportErrorView> CreateSpedProExport(IEP studentIEP, int schoolYear, StringBuilder sb)
        {
            var errors = new List<ExportErrorView>();

            //1 KidsID Req
            sb.AppendFormat("{0}", studentIEP.studentDetails.student.KIDSID);

            //2 Last Name, Student’s Legal Req less < 60 characters
            sb.AppendFormat("\t{0}", studentIEP.studentLastName.Length > 60 ? studentIEP.studentLastName.Substring(0, 60) : studentIEP.studentLastName);

            //3 Student’s Gender
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.Gender == "M" ? 1 : 0);

            //4 DOB MM/DD/YYYY
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.DateOfBirth.ToShortDateString());

            //5 School Year YYYY Req
            sb.AppendFormat("\t{0}", schoolYear);

            //6 Assign Child Count Req
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.assignChildCount);

            //7 Neighborhood Building Identifier Req
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.neighborhoodBuilding.BuildingID);

            //8 grade level req
            var grade = db.tblGrades.Where(o => o.gradeID == studentIEP.studentDetails.student.Grade).FirstOrDefault();

            var gradeCode = grade != null && grade.SpedCode != null ? grade.SpedCode : "";

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
                sb.AppendFormat("\t{0}", gradeCode);
            }

            //9 status code req
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.StatusCode);

            //10 exit date
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.ExitDate.HasValue ? studentIEP.studentDetails.student.ExitDate.Value.ToShortDateString() : "");

            //11 School Psychologist Provider ID
            sb.AppendFormat("\t{0}", "");

            //12 slp provider id
            sb.AppendFormat("\t{0}", "");

            //13 case manager provider id
            sb.AppendFormat("\t{0}", "");

            //14 extended school year
            sb.AppendFormat("\t{0}", studentIEP.studentOtherConsiderations != null ? studentIEP.studentOtherConsiderations.ExtendedSchoolYear_Necessary : "");

            //15 sped transportation		
            sb.AppendFormat("\t{0}", studentIEP.studentServices != null && studentIEP.studentServices.Count(o => o.ServiceCode == "ST") > 0 ? "1" : "");

            //16 All Day Kindergarten
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.FullDayKG == null ? "" : studentIEP.studentDetails.student.FullDayKG.Value == true ? "1" : "");

            //17 Behavior Intervention Plan - BIP BehaviorInterventionPlan
            sb.AppendFormat("\t{0}", studentIEP.studentSocial != null && studentIEP.studentSocial.BehaviorInterventionPlan ? "1" : "");

            //18 Claiming Code req
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.ClaimingCode ? "1" : "");

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
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.PlacementCode);
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
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.studentCounty);
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
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.parentLang);
            }

            string serviceEndDateOverride = "";

            //if exit data exists
            if (studentIEP.studentDetails.student.ExitDate.HasValue)
            {
                serviceEndDateOverride = studentIEP.studentDetails.student.ExitDate.Value.ToShortDateString();
            }

            int count = 1;
            foreach (var service in studentIEP.studentServices.Distinct())
            {
                if (count == 25)
                    break;

                service.FiledOn = DateTime.Now;

				DateTime? serviceIEPDate = null;

				//1 IEP date req
				if (service.IEPid != studentIEP.current.IEPid)
                {
                    //need to look up date from the iep this service is from
                    var serviceIEP = db.tblIEPs.Where(o => o.IEPid == service.IEPid).FirstOrDefault();
					

					if (serviceIEP.OriginalIEPid != null)
					{
						//look up date of orginal iep
						var originalIEP = db.tblIEPs.Where(o => o.IEPid == serviceIEP.OriginalIEPid).FirstOrDefault();
						if (originalIEP != null && originalIEP.begin_date.HasValue)
							serviceIEPDate = originalIEP.begin_date.Value;
					}
					else
					{
						if (serviceIEP.begin_date.HasValue)
						{
							serviceIEPDate = serviceIEP.begin_date.Value;
						}
					}

                    if (!serviceIEPDate.HasValue)
                    {
                        errors.Add(new ExportErrorView()
                        {
                            UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                            Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: R1 IEP date")
                        });
                    }
                    else
                    {
                        sb.AppendFormat("\t{0}", serviceIEPDate.Value.ToShortDateString());
                    }

                }
                else
                {
					if (studentIEP.current.OriginalIEPid != null)
					{
						//look up date of orginal iep
						var originalIEP2 = db.tblIEPs.Where(o => o.IEPid == studentIEP.current.OriginalIEPid).FirstOrDefault();
						if (originalIEP2 != null && originalIEP2.begin_date.HasValue)
							serviceIEPDate = originalIEP2.begin_date.Value;
					}
					else
					{
						serviceIEPDate = studentIEP.current.begin_date.Value;
					}

					if (!serviceIEPDate.HasValue)
                    {
                        errors.Add(new ExportErrorView()
                        {
                            UserID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                            Description = string.Format("Student: {0}, {1} Error: {2}", studentIEP.studentFirstName, studentIEP.studentLastName, "Missing required field: R1 IEP date")
                        });
                    }
                    else
                    {
                        sb.AppendFormat("\t{0}", serviceIEPDate.Value.ToShortDateString());
                    }
                }

                //2 gap allow
                sb.AppendFormat("\t{0}", "");

                //3 Responsible School req
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.neighborhoodBuilding.BuildingID);

                //4 primary disablity
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.primaryDisability);

                //5 secondary disablity
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.secondaryDisability);

                //6 gifted
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.isGifted ? "1" : "0");

                //7 service location req
                sb.AppendFormat("\t{0}", string.IsNullOrEmpty(service.BuildingID) ? studentIEP.studentDetails.building.BuildingID : service.BuildingID);

                //8 Primary Service Location Indicator
                sb.AppendFormat("\t{0}", "");

                //9 setting code
                sb.AppendFormat("\t{0}", service.LocationCode);

                //10 service code
                sb.AppendFormat("\t{0}", service.ServiceCode);

                //11 provider id
                sb.AppendFormat("\t{0}", service.tblProvider != null ? service.tblProvider.ProviderCode.Length > 10 ? service.tblProvider.ProviderCode.Substring(0, 10) : service.tblProvider.ProviderCode : "");

                //12 Primary Provider Indicator
                sb.AppendFormat("\t{0}", "");

                //13 Service Start Date
                sb.AppendFormat("\t{0}", service.StartDate.ToShortDateString());

                //14 Service end Date
                sb.AppendFormat("\t{0}", string.IsNullOrEmpty(serviceEndDateOverride) ? service.EndDate.ToShortDateString() : serviceEndDateOverride);

                //15 minutes
                sb.AppendFormat("\t{0}", service.Minutes);

                //16 days per
                sb.AppendFormat("\t{0}", service.DaysPerWeek);

                //17 freq
                sb.AppendFormat("\t{0}", service.Frequency);

                //18 total days
                sb.AppendFormat("\t{0}", "");

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
        public ActionResult DownloadPDF(FormCollection collection)
        {

            string StudentHTMLContent = collection["studentText"];
            string HTMLContent = collection["printText"];
            string HTMLContent2 = collection["printText2"];
            string HTMLContent3 = collection["printText3"];
            string studentName = collection["studentName"];
            string studentId = collection["studentId"];
            string isArchive = collection["isArchive"];
            string iepIDStr = collection["iepID"];
            string isIEP = collection["isIEP"];
            string formName = collection["formName"];
            string isSave = collection["isSave"];

            if (isSave == "1")
            {
                try
                {
                    SaveFormValues(HTMLContent, formName, studentId);
                }
                catch
                {

                }

                return RedirectToAction("IEPFormModule", "Home", new { studentId = Int32.Parse(studentId), saved = 1 });
            }
            else
            {
                var mergedFile = this.CreateIEPPdf(StudentHTMLContent, HTMLContent, HTMLContent2, HTMLContent3, studentName, studentId, isArchive, iepIDStr, isIEP, formName);
                if (mergedFile != null)
                {
                    string downloadFileName = string.IsNullOrEmpty(HTMLContent) ? "StudentInformation.pdf" : "IEP.pdf";
                    OutputResponse(mergedFile, downloadFileName, "application/pdf");

                }
            }

            return null;

        }

        private byte[] CreateIEPPdf(string StudentHTMLContent, string HTMLContent, string HTMLContent2, string HTMLContent3, string studentName, string studentId,
        string isArchive, string iepIDStr, string isIEP, string formName)
        {
            if (!string.IsNullOrEmpty(HTMLContent) || !string.IsNullOrEmpty(StudentHTMLContent) || !string.IsNullOrEmpty(HTMLContent2) || !string.IsNullOrEmpty(HTMLContent3))
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

                if (formName == "Parents Rights-English")
                    studentName = string.Empty;


                bool isDraft = false;

                var iepObj = db.tblIEPs.Where(o => o.IEPid == iepId).FirstOrDefault();
                if (iepObj != null)
                {
                    isDraft = iepObj.IepStatus != null && iepObj.IepStatus.ToUpper() == "DRAFT" ? true : false;
                }


                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                var cssText = @"<style>hr{color:whitesmoke;padding:0;margin:0;padding-top:2px;padding-bottom:2px;}h5{font-weight:500}.module-page{font-size:9pt;}.header{color:white;}img{margin-top:-10px;}.input-group-addon, .transitionGoalLabel, .transitionServiceLabel {font-weight:600;}.transitionServiceLabel, .underline{ text-decoration: underline;}.transition-break{page-break-before:always;}td { padding: 10px;}th {font-weight:600;}table {width:700px;border-spacing: 0px;border:none;font-size:9pt}.module-page, span {font-size:10pt;}label{font-weight:600;font-size:9pt}.text-center{text-align:center} h3 {font-weight:400;font-size:11pt;width:100%;text-align:center;padding:8px;}p {padding-top:5px;padding-bottom:5px;font-size:9pt}.section-break {page-break-after:always;color:white;background-color:white}.funkyradio {padding-bottom:15px;}.radio-inline {font-weight:normal;}div{padding-top:10px;}.form-check {padding-left:5px;}.dont-break {margin-top:10px;page-break-inside: avoid;} .form-group{margin-bottom:8px;} div.form-group-label{padding:0;padding-top:3px;padding-bottom:3px;} .checkbox{margin:0;padding:0} .timesfont{font-size:12pt;font-family:'Times New Roman',serif} .hidden {color:white} </style>";
                string result = "";
                if (!string.IsNullOrEmpty(HTMLContent))
                {
                    result = System.Text.RegularExpressions.Regex.Replace(HTMLContent, @"\r\n?|\n", "");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"new-line-val", "<br/>");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"</?textarea>", "");
                }

                string cssTextResult = System.Text.RegularExpressions.Regex.Replace(cssText, @"\r\n?|\n", "");
                byte[] studentFile = null;

                if (!string.IsNullOrEmpty(StudentHTMLContent))
                {
                    string result2 = System.Text.RegularExpressions.Regex.Replace(StudentHTMLContent, @"\r\n?|\n", "");
                    result2 = System.Text.RegularExpressions.Regex.Replace(result2, @"textarea", "p");
                    studentFile = CreatePDFBytes(cssTextResult, result2, "studentInformationPage", imgfoot, "", isDraft, false);
                }

                byte[] secondaryPageFile = null;
                if (!string.IsNullOrEmpty(HTMLContent2))
                {
                    string secondaryPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent2, @"\r\n?|\n", "");
                    secondaryPage = System.Text.RegularExpressions.Regex.Replace(secondaryPage, @"</?textarea>", "");
                    secondaryPageFile = CreatePDFBytes(cssTextResult, secondaryPage, "module-page", imgfoot, studentName, isDraft, true);
                }

                byte[] thirdPageFile = null;
                if (!string.IsNullOrEmpty(HTMLContent3))
                {
                    string thirdPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent3, @"\r\n?|\n", "");
                    thirdPage = System.Text.RegularExpressions.Regex.Replace(thirdPage, @"</?textarea>", "");
                    thirdPageFile = CreatePDFBytes(cssTextResult, thirdPage, "module-page", imgfoot, studentName, isDraft, true);
                }

                byte[] iepFile = null;
                if (!string.IsNullOrEmpty(result))
                    iepFile = CreatePDFBytes(cssTextResult, result, "module-page", imgfoot, studentName, isDraft, true);


                List<byte[]> pdfByteContent = new List<byte[]>();

                if (studentFile != null)
                    pdfByteContent.Add(studentFile);

                if (iepFile != null)
                {
                    pdfByteContent.Add(iepFile);

                    //extra primary contacts
                    if (secondaryPageFile != null)
                    {
                        pdfByteContent.Add(secondaryPageFile);
                    }

                    if (thirdPageFile != null)
                    {
                        pdfByteContent.Add(thirdPageFile);
                    }

                }
                else
                    formName = "Student Information";//this is just the student info page print

                var mergedFile = concatAndAddContent(pdfByteContent);

                if (isArchive == "1")
                {
                    try
                    {
						var formNameValue = formName;
						
						if (formName == null || formName == "IEP")
						{
							if (iepObj != null)
							{
								if(iepObj.Amendment)
									formNameValue = string.Format("Amendment IEP {0}", iepObj.begin_date.HasValue ? iepObj.begin_date.Value.ToShortDateString() : "");
								else
									formNameValue = string.Format("Annual IEP {0}", iepObj.begin_date.HasValue ? iepObj.begin_date.Value.ToShortDateString() : "");
							}
						}

						var archive = new tblFormArchive();
                        archive.Creator_UserID = teacher.UserID;
                        archive.Student_UserID = id;
                        archive.FormName = string.IsNullOrEmpty(formNameValue) ? "IEP" : formNameValue;
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

                            Document pdfDoc = new Document(PageSize.LETTER, 36, 36, 35, 50);

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
                        //ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(string.Format("Date Printed: {0}", DateTime.Now.ToShortDateString()), blackFont), 568f, 15f, 0);
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

        private void OutputResponse(byte[] memoryStream, string fileName, string contentType)
        {
            Response.Clear();

            Response.ContentType = contentType; //"application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);

            Response.BinaryWrite(memoryStream);
            Response.End();
        }

		private bool ArchiveIEPPrint(int studentId, IEP theIEP)
		{
			bool success = false;

			//create archive
			try
			{
				
				if (theIEP != null)
				{
					theIEP.studentDetails.printStudentInfo = true;
					theIEP.studentDetails.printIEPDetails = true;
					theIEP.studentDetails.printHealth = true;
					theIEP.studentDetails.printMotor = true;
					theIEP.studentDetails.printComm = true;
					theIEP.studentDetails.printSocial = true;
					theIEP.studentDetails.printGeneral = true;
					theIEP.studentDetails.printAcademic = true;
					theIEP.studentDetails.printAcc = true;
					theIEP.studentDetails.printBehavior = true;
					theIEP.studentDetails.printTrans = true;
					theIEP.studentDetails.printOther = true;
					theIEP.studentDetails.printGoals = true;
					theIEP.studentDetails.printServices = true;
					theIEP.studentDetails.printNotice = true;
					theIEP.studentDetails.printProgressReport = false;
					theIEP.studentDetails.isArchive = true;
					theIEP.isServerRender = true;
				}

				var data = RenderRazorViewToString("~/Views/Home/_PrintPartial.cshtml", theIEP);

				string result = System.Text.RegularExpressions.Regex.Replace(data, @"\r\n?|\n|\t", "");
				result = System.Text.RegularExpressions.Regex.Replace(result, @"break-line-val", "<br/>");
				HtmlDocument doc = new HtmlDocument();
				doc.OptionWriteEmptyNodes = true;
				doc.OptionFixNestedTags = true;
				doc.LoadHtml(result);

				var studentInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("studentInformationPage")).FirstOrDefault();
				var moduleInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("module-page")).FirstOrDefault();
				var mergedFile = CreateIEPPdf(studentInfo.InnerHtml, moduleInfo.InnerHtml, "", "", "", studentId.ToString(), "1", theIEP.current.IEPid.ToString(), "1", string.Format("Annual IEP {0}", theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.ToShortDateString() : theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString()));

				//print progress report separately
				if (theIEP != null)
				{
					theIEP.studentDetails.printStudentInfo = false;
					theIEP.studentDetails.printIEPDetails = false;
					theIEP.studentDetails.printHealth = false;
					theIEP.studentDetails.printMotor = false;
					theIEP.studentDetails.printComm = false;
					theIEP.studentDetails.printSocial = false;
					theIEP.studentDetails.printGeneral = false;
					theIEP.studentDetails.printAcademic = false;
					theIEP.studentDetails.printAcc = false;
					theIEP.studentDetails.printBehavior = false;
					theIEP.studentDetails.printTrans = false;
					theIEP.studentDetails.printOther = false;
					theIEP.studentDetails.printGoals = false;
					theIEP.studentDetails.printServices = false;
					theIEP.studentDetails.printNotice = false;
					theIEP.studentDetails.printProgressReport = true;
					theIEP.studentDetails.isArchive = true;
					theIEP.isServerRender = true;
				}

				data = RenderRazorViewToString("~/Views/Home/_PrintPartial.cshtml", theIEP);

				result = System.Text.RegularExpressions.Regex.Replace(data, @"\r\n?|\n|\t", "");
				result = System.Text.RegularExpressions.Regex.Replace(result, @"break-line-val", "<br/>");
				doc = new HtmlDocument();
				doc.OptionWriteEmptyNodes = true;
				doc.OptionFixNestedTags = true;
				doc.LoadHtml(result);

				var progressModuleInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("module-page")).FirstOrDefault();
				var progressReport = CreateIEPPdf("", progressModuleInfo.InnerHtml, "", "", "", studentId.ToString(), "1", theIEP.current.IEPid.ToString(), "1", string.Format("Progress Report {0}", theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.ToShortDateString() : theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString()));

				success = true;

			}
			catch (Exception e)
			{
				success = false;
                Console.Write(e.InnerException.ToString());				
			}

			return success;
		}

        #region FormPDFDownload
        private void SaveFormValues(string HTMLContent, string formName, string studentId)
        {
            //capture data
            int sid = !string.IsNullOrEmpty(studentId) ? Int32.Parse(studentId) : 0;

            if (sid == 0)
                return;

            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.OptionWriteEmptyNodes = true;
            htmlDocument.OptionFixNestedTags = true;
            htmlDocument.LoadHtml(HTMLContent);

            var spans = htmlDocument.DocumentNode.Descendants().Where(o => o.Name.Equals("span") && o.Id != "").ToList();
            var checkboxes = htmlDocument.DocumentNode.Descendants().Where(o => o.Name.Equals("img") && o.HasClass("imgCheck")).ToList();

			if (formName == "Team Evaluation Report")
			{
				var teamEval = db.tblFormTeamEvals.Any(o => o.StudentId == sid) ? db.tblFormTeamEvals.FirstOrDefault(o => o.StudentId == sid) : new tblFormTeamEval();

				teamEval.StudentId = sid;
				teamEval.ReasonReferral = GetInputValue("txtReasonReferral", spans);
				teamEval.MedicalFindings = GetInputValue("txtMedicalFindings", spans);
				teamEval.Hearing = GetInputValue("txtHearing", spans);
				teamEval.Vision = GetInputValue("txtVision", spans);
				teamEval.RelevantBehavior = GetInputValue("txtRelevantBehavior", spans);
				teamEval.InfoReview = GetInputValue("txtInfoReview", spans);
				teamEval.ParentInterview = GetInputValue("txtParentInterview", spans);
				teamEval.TestData = GetInputValue("txtTestData", spans);
				teamEval.IntellectualDevelopment = GetInputValue("txtIntellectualDevelopment", spans);
				teamEval.Peformance = GetInputValue("txtPeformance", spans);
				teamEval.Disadvantage = GetInputValue("txtDisadvantage", spans);
				teamEval.DisadvantageExplain = GetInputValue("txtDisadvantageExplain", spans);
				teamEval.Regulations = GetInputValue("txtRegulations", spans);
				teamEval.SustainedResources = GetInputValue("txtSustainedResources", spans);
				teamEval.Strengths = GetInputValue("txtStrengths", spans);
				teamEval.AreaOfConcern = GetInputValue("txtAreaOfConcern", spans);
				teamEval.GeneralEducationExpectations = GetInputValue("txtGeneralEducationExpectations", spans);
				teamEval.Tried = GetInputValue("txtTried", spans);
				teamEval.NotWorked = GetInputValue("txtNotWorked", spans);
				teamEval.GeneralDirection = GetInputValue("txtGeneralDirection", spans);
				teamEval.MeetEligibility = GetInputValue("txtMeetEligibility", spans);
				teamEval.ResourcesNeeded = GetInputValue("txtResourcesNeeded", spans);
				teamEval.SpecificNeeds = GetInputValue("txtSpecificNeeds", spans);
				teamEval.ConvergentData = GetInputValue("txtConvergentData", spans);
				teamEval.ListSources = GetInputValue("txtListSources", spans);


				teamEval.Regulation_flag = GetCheckboxInputValue("Regulation_flag_Yes", "Regulation_flag_No", checkboxes);
				teamEval.SustainedResources_flag = GetCheckboxInputValue("SustainedResources_flag_Yes", "SustainedResources_flag_No", checkboxes);
				teamEval.ConvergentData_flag = GetCheckboxInputValue("ConvergentData_flag_Yes", "ConvergentData_flag_No", checkboxes);

				if (teamEval.FormTeamEvalId == 0)
				{
					teamEval.CreatedBy = currentUser.UserID;
					teamEval.Create_Date = DateTime.Now;
					teamEval.ModifiedBy = currentUser.UserID;
					teamEval.Update_Date = DateTime.Now;
					db.tblFormTeamEvals.Add(teamEval);
				}
				else
				{
					teamEval.ModifiedBy = currentUser.UserID;
					teamEval.Update_Date = DateTime.Now;
				}

				db.SaveChanges();
			}
			else if (formName == "Summary Of Performance")
			{
				var summaryPerf = db.tblFormSummaryPerformances.Any(o => o.StudentId == sid) ? db.tblFormSummaryPerformances.FirstOrDefault(o => o.StudentId == sid) : new tblFormSummaryPerformance();

				summaryPerf.StudentId = sid;
				summaryPerf.Goal_Learning = GetInputValue("Goal_Learning", spans);
				summaryPerf.Goal_LearningRecommendation = GetInputValue("Goal_LearningRecommendation", spans);
				summaryPerf.Goal_Working = GetInputValue("Goal_Working", spans);
				summaryPerf.Goal_WorkingRecommendation = GetInputValue("Goal_WorkingRecommendation", spans);
				summaryPerf.Goal_Living = GetInputValue("Goal_Living", spans);
				summaryPerf.Goal_LivingRecommendation = GetInputValue("Goal_LivingRecommendation", spans);
				summaryPerf.AC_ReadingPerformance = GetInputValue("AC_ReadingPerformance", spans);
				summaryPerf.AC_ReadingAccommodations = GetInputValue("AC_ReadingAccommodations", spans);
				summaryPerf.AC_MathPerformance = GetInputValue("AC_MathPerformance", spans);
				summaryPerf.AC_MathAccommodations = GetInputValue("AC_MathAccommodations", spans);
				summaryPerf.AC_LanguagePerformance = GetInputValue("AC_LanguagePerformance", spans);
				summaryPerf.AC_LanguageAccommodations = GetInputValue("AC_LanguageAccommodations", spans);
				summaryPerf.AC_LearningPerformance = GetInputValue("AC_LearningPerformance", spans);
				summaryPerf.AC_LearningAccommodations = GetInputValue("AC_LearningAccommodations", spans);
				summaryPerf.AC_OtherPerformance = GetInputValue("AC_OtherPerformance", spans);
				summaryPerf.AC_OtherAccommodations = GetInputValue("AC_OtherAccommodations", spans);
				summaryPerf.Functional_SocialPerformance = GetInputValue("Functional_SocialPerformance", spans);
				summaryPerf.Functional_SocialAccommodations = GetInputValue("Functional_SocialAccommodations", spans);
				summaryPerf.Functional_LivingPerformance = GetInputValue("Functional_LivingPerformance", spans);
				summaryPerf.Functional_LivingAccommodations = GetInputValue("Functional_LivingAccommodations", spans);
				summaryPerf.Functional_MobiilityPerformance = GetInputValue("Functional_MobiilityPerformance", spans);
				summaryPerf.Functional_MobiilityAccommodations = GetInputValue("Functional_MobiilityAccommodations", spans);
				summaryPerf.Functional_AdvocacyPerformance = GetInputValue("Functional_AdvocacyPerformance", spans);
				summaryPerf.Functional_AdvocacyAccommodations = GetInputValue("Functional_AdvocacyAccommodations", spans);
				summaryPerf.Functional_EmploymentPerformance = GetInputValue("Functional_EmploymentPerformance", spans);
				summaryPerf.Functional_EmploymentAccommodations = GetInputValue("Functional_EmploymentAccommodations", spans);
				summaryPerf.Functional_AdditionsPerformance = GetInputValue("Functional_AdditionsPerformance", spans);
				summaryPerf.Functional_AdditionsAccommodations = GetInputValue("Functional_AdditionsAccommodations", spans);
				summaryPerf.DateCompleted = GetInputDateValue("DateCompleted", spans);
				summaryPerf.Documentation_PsychologicalAssementName = GetInputValue("Documentation_PsychologicalAssementName", spans);
				summaryPerf.Documentation_PsychologicalDate = GetInputDateValue("Documentation_PsychologicalDate", spans);
				summaryPerf.Documentation_NeuropsychologicalAssementName = GetInputValue("Documentation_NeuropsychologicalAssementName", spans);
				summaryPerf.Documentation_NeuropsychologicalDate = GetInputDateValue("Documentation_NeuropsychologicalDate", spans);
				summaryPerf.Documentation_MedicalAssementName = GetInputValue("Documentation_MedicalAssementName", spans);
				summaryPerf.Documentation_MedicalDate = GetInputDateValue("Documentation_MedicalDate", spans);
				summaryPerf.Documentation_CommunicationAssementName = GetInputValue("Documentation_CommunicationAssementName", spans);
				summaryPerf.Documentation_CommunicationDate = GetInputDateValue("Documentation_CommunicationDate", spans);
				summaryPerf.Documentation_AdaptiveBehaviorAssementName = GetInputValue("Documentation_AdaptiveBehaviorAssementName", spans);
				summaryPerf.Documentation_AdaptiveBehaviorDate = GetInputDateValue("Documentation_AdaptiveBehaviorDate", spans);
				summaryPerf.Documentation_InterpersonalAssementName = GetInputValue("Documentation_InterpersonalAssementName", spans);
				summaryPerf.Documentation_InterpersonalDate = GetInputDateValue("Documentation_InterpersonalDate", spans);
				summaryPerf.Documentation_SpeechAssementName = GetInputValue("Documentation_SpeechAssementName", spans);
				summaryPerf.Documentation_SpeechDate = GetInputDateValue("Documentation_SpeechDate", spans);
				summaryPerf.Documentation_MTSSAssementName = GetInputValue("Documentation_MTSSAssementName", spans);
				summaryPerf.Documentation_MTSSDate = GetInputDateValue("Documentation_MTSSDate", spans);
				summaryPerf.Documentation_CareerAssementName = GetInputValue("Documentation_CareerAssementName", spans);
				summaryPerf.Documentation_CareerDate = GetInputDateValue("Documentation_CareerDate", spans);
				summaryPerf.Documentation_CommunityAssementName = GetInputValue("Documentation_CommunityAssementName", spans);
				summaryPerf.Documentation_CommunityDate = GetInputDateValue("Documentation_CommunityDate", spans);
				summaryPerf.Documentation_SelfDeterminationAssementName = GetInputValue("Documentation_SelfDeterminationAssementName", spans);
				summaryPerf.Documentation_SelfDeterminationDate = GetInputDateValue("Documentation_SelfDeterminationDate", spans);
				summaryPerf.Documentation_AssistiveTechAssementName = GetInputValue("Documentation_AssistiveTechAssementName", spans);
				summaryPerf.Documentation_AssistiveTechDate = GetInputDateValue("Documentation_AssistiveTechDate", spans);
				summaryPerf.Documentation_ClassroomAssementName = GetInputValue("Documentation_ClassroomAssementName", spans);
				summaryPerf.Documentation_ClassroomDate = GetInputDateValue("Documentation_ClassroomDate", spans);
				summaryPerf.Documentation_OtherAssementName = GetInputValue("Documentation_OtherAssementName", spans);
				summaryPerf.Documentation_OtherDate = GetInputDateValue("Documentation_OtherDate", spans);
				summaryPerf.AdditionalInformation = GetInputValue("AdditionalInformation", spans);


				if (summaryPerf.FormSummaryPerformanceId == 0)
				{
					summaryPerf.CreatedBy = currentUser.UserID;
					summaryPerf.Create_Date = DateTime.Now;
					summaryPerf.ModifiedBy = currentUser.UserID;
					summaryPerf.Update_Date = DateTime.Now;
					db.tblFormSummaryPerformances.Add(summaryPerf);
				}
				else
				{
					summaryPerf.ModifiedBy = currentUser.UserID;
					summaryPerf.Update_Date = DateTime.Now;
				}

				db.SaveChanges();
			}
			else if (formName == "Conference Summary")
			{
				var conf = db.tblFormConferenceSummaries.Any(o => o.StudentId == sid) ? db.tblFormConferenceSummaries.FirstOrDefault(o => o.StudentId == sid) : new tblFormConferenceSummary();

				conf.StudentId = sid;
				conf.BuildingAdministrator = GetInputValue("txtBuildingAdministrator", spans);
				conf.RequestedBy = GetInputValue("txtRequestedBy", spans);
				conf.ReasonForConfrence = GetInputValue("txtReasonForConfrence", spans);
				conf.Conclusions = GetInputValue("txtConclusions", spans);
				if (conf.FormConferenceSummaryId == 0)
				{
					conf.CreatedBy = currentUser.UserID;
					conf.Create_Date = DateTime.Now;
					conf.ModifiedBy = currentUser.UserID;
					conf.Update_Date = DateTime.Now;
					db.tblFormConferenceSummaries.Add(conf);
				}
				else
				{
					conf.ModifiedBy = currentUser.UserID;
					conf.Update_Date = DateTime.Now;
				}

				db.SaveChanges();
			}
		}

        private string GetInputValue(string inputName, List<HtmlNode> inputs)
        {
            var input = inputs.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
                return input.InnerHtml != null ? input.InnerHtml.Replace("&nbsp;", "") : "";
            else

				return "";
        }

        private DateTime? GetInputDateValue(string inputName, List<HtmlNode> inputs)
        {
            var input = inputs.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {
                DateTime dtVal;
                if (DateTime.TryParse(input.InnerHtml, out dtVal))
                    return dtVal;
                else
                    return null;
            }
            else
                return null;
        }

        private bool? GetCheckboxInputValue(string inputName, string inputName2, List<HtmlNode> checkboxes)
        {
            bool? returnValue = null;
            var valYes = "";
            var valNo = "";

            var input = checkboxes.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {
                valYes = input.OuterHtml != null && input.OuterHtml.Contains("check_yes") ? "Y" : "";
            }

            var input2 = checkboxes.Where(o => o.Id == inputName).FirstOrDefault();
            if (input2 != null)
            {
                valNo = input2.OuterHtml != null && input2.OuterHtml.Contains("check_yes") ? "Y" : "";
            }

            if (valYes == "Y")
                returnValue = true;
            else if (valNo == "Y")
                returnValue = false;

            return returnValue;

        }

        #endregion
    }
}