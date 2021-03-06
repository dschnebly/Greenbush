﻿using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
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
        private const string principal = "11";
        private const string superintendent = "12";

        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

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
        public ActionResult Portal(int? logon)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(owner))
                {
                    return RedirectToAction("OwnerPortal");
                }
                else if (User.IsInRole(mis))
                {
                    return RedirectToAction("MISPortal", new { logon = logon });
                }
                else if (User.IsInRole(admin))
                {
                    return RedirectToAction("AdminPortal", new { logon = logon });
                }
                else if (User.IsInRole(teacher))
                {
                    return RedirectToAction("TeacherPortal", new { logon = logon });
                }
                else if (User.IsInRole(nurse) || User.IsInRole(principal) || User.IsInRole(superintendent))
                {
                    return RedirectToAction("NursePortal");
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = owner)]
        public ActionResult OwnerPortal()
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            tblUser OWNER = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (OWNER != null)
            {
                List<tblDistrict> allDistricts = db.tblDistricts.Distinct().ToList();
                string usd = allDistricts.FirstOrDefault().USD;

                PortalViewModel model = new PortalViewModel
                {
                    user = OWNER,
                    districts = allDistricts,
                    buildings = (from building in db.tblBuildings select building).Distinct().ToList(),
                    members = db.uspUserAssignedList(OWNER.UserID, usd, null, null, true).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, IEPDate = u.ActiveIEP_BeginDate, ReEvaluationDate = u.ReEvalConsentSigned, InitalEvaluationDate = u.InitialEvalConsentSigned, hasIEP = u.hasIEP ?? false }).ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList(),
                };

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(OWNER);

                return View("OwnerPortal", model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = mis)]
        public ActionResult MISPortal(int? logon)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {

                List<tblDistrict> allDistricts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == MIS.UserID select district).Distinct().ToList();
                string usd = allDistricts.FirstOrDefault().USD;

                PortalViewModel model = new PortalViewModel
                {
                    user = MIS,
                    districts = allDistricts,
                    buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList(),
                    members = db.uspUserAssignedList(MIS.UserID, usd, null, null, true).Where(u => u.StatusActive == 1).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, IEPDate = u.ActiveIEP_BeginDate, ReEvaluationDate = u.ReEvalConsentSigned, InitalEvaluationDate = u.InitialEvalConsentSigned, hasIEP = u.hasIEP ?? false }).ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList()
                };

                //dashboard notify
                model.draftIeps = GetDraftIeps(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.dueIeps = GetIepsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.evalsDue = GetEvalsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.showDashboardNotification = logon.HasValue && logon.Value == 1;

                
                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(MIS);

                return View("MISPortal", model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = admin)]
        public ActionResult AdminPortal(int? userId, int? logon)
        {

            tblUser ADMIN = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (ADMIN != null)
            {

                PortalViewModel model = new PortalViewModel
                {
                    user = ADMIN,
                    districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == ADMIN.UserID select district).Distinct().ToList(),
                    buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == ADMIN.UserID select building).Distinct().ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList(),
                    members = db.uspUserAssignedList(ADMIN.UserID, null, null, null, true).Where(u => u.StatusActive == 1).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, BirthDate = u.DateOfBirth, IEPDate = u.ActiveIEP_BeginDate, ReEvaluationDate = u.ReEvalConsentSigned, InitalEvaluationDate = u.InitialEvalConsentSigned , hasIEP = u.hasIEP ?? false }).ToList()
                };

                //dashboard notify
                 model.draftIeps = GetDraftIeps(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.dueIeps = GetIepsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.evalsDue = GetEvalsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.showDashboardNotification = logon.HasValue && logon.Value == 1;

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(ADMIN);

                return View("AdminPortal", model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = teacher)]
        public ActionResult TeacherPortal(int? userId, int? logon, bool hasSeenAgreement = false)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (teacher != null)
            {
                PortalViewModel model = new PortalViewModel
                {
                    user = teacher,
                    districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == teacher.UserID select district).Distinct().ToList(),
                    buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == teacher.UserID select building).Distinct().ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList(),
                    members = db.uspUserAssignedList(teacher.UserID, null, null, null, null).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, BirthDate = u.DateOfBirth, IEPDate = u.ActiveIEP_BeginDate, ReEvaluationDate = u.ReEvalConsentSigned, InitalEvaluationDate = u.InitialEvalConsentSigned, hasIEP = u.hasIEP ?? false }).ToList()
                };

                //dashboard notify
                model.draftIeps = GetDraftIeps(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.dueIeps = GetIepsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.evalsDue = GetEvalsDue(model.members != null ? string.Join(",", model.members.Where(u => u.RoleID == student).Select(o => o.UserID)) : "");
                model.showDashboardNotification = logon.HasValue && logon.Value == 1;

                // show the latest updated version changes
                ViewBag.UpdateCount = VersionCompare.GetVersionCount(teacher);

                return View(model);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [Authorize(Roles = nurse + "," + principal + "," + superintendent)]
        public ActionResult NursePortal(int? userId)
        {
            tblUser nurse = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (nurse != null)
            {
                PortalViewModel model = new PortalViewModel
                {
                    user = nurse,
                    districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == nurse.UserID select district).Distinct().ToList(),
                    buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == nurse.UserID select building).Distinct().ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList(),
                    members = db.uspUserAssignedList(nurse.UserID, null, null, null, true).Where(u => u.StatusActive == 1).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, BirthDate = u.DateOfBirth, IEPDate = u.ActiveIEP_BeginDate, ReEvaluationDate = u.ReEvalConsentSigned, InitalEvaluationDate = u.InitialEvalConsentSigned, hasIEP = u.hasIEP ?? false }).ToList()
                };

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

                List<tblDistrict> MISDistrictList = (from buildingMaps in db.tblBuildingMappings
                                                     join districts in db.tblDistricts
                                                          on buildingMaps.USD equals districts.USD
                                                     where buildingMaps.UserID == MIS.UserID
                                                     select districts).Distinct().ToList();

                List<tblBuilding> MISBuildingList = (from buildingMaps in db.tblBuildingMappings
                                                     join buildings in db.tblBuildings
                                                        on buildingMaps.BuildingID equals buildings.BuildingID
                                                     where buildingMaps.UserID == MIS.UserID
                                                     select buildings).Distinct().OrderBy(b => b.BuildingID).ToList();

                List<tblCalendar> defaultCalendar = new List<tblCalendar>();
                foreach (tblCalendarTemplate day in temp)
                {
                    tblCalendar calendar = new tblCalendar
                    {
                        canHaveClass = day.canHaveClass,
                        Day = day.Day,
                        Month = day.Month,
                        Year = day.Year,
                        NoService = day.NoService,
                        SchoolYear = day.SchoolYear
                    };

                    defaultCalendar.Add(calendar);
                }

                MISCalendarViewModel model = new MISCalendarViewModel
                {
                    districts = MISDistrictList,
                    buildings = MISBuildingList,
                    calendarDays = defaultCalendar
                };

                return PartialView("_ModuleCalendarSection", model);
            }

            if (view == "ServiceProviderModule" && MIS != null)
            {
                MISProviderViewModel model = new MISProviderViewModel();

                List<tblDistrict> MISDistrictList = (from buildingMaps in db.tblBuildingMappings
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

                MISDistricContactViewModel model = new MISDistricContactViewModel
                {
                    myDistricts = (from buildingMaps in db.tblBuildingMappings join districts in db.tblDistricts on buildingMaps.USD equals districts.USD where buildingMaps.UserID == MIS.UserID select districts).Distinct().ToList()
                };
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
                    foreach (tblProviderDistrict existingPD in provider.tblProviderDistricts.ToList())
                    {
                        db.tblProviderDistricts.Remove(existingPD);
                    }

                    db.SaveChanges();

                    foreach (string district in providerDistrict)
                    {
                        db.tblProviderDistricts.Add(new tblProviderDistrict() { ProviderID = provider.ProviderID, USD = district, CreatedBy = owner.UserID, Create_Date = DateTime.Now });
                        db.SaveChanges();
                    }

                    IQueryable<ProviderViewModel> listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.LastName).ThenBy(o => o.FirstName) }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tblProvider newProvider = new tblProvider
                    {
                        FirstName = providerFirstName.ToString(),
                        LastName = providerLastName.ToString(),
                        ProviderCode = providerCode.ToString(),
                        UserID = owner.UserID,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };

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
                                foreach (string district in providerDistrict)
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

                    List<tblDistrict> MISDistrictList = (from buildingMaps in db.tblBuildingMappings
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
                    foreach (tblProviderDistrict existingPD in provider.tblProviderDistricts.ToList())
                    {
                        db.tblProviderDistricts.Remove(existingPD);
                    }

                    db.tblProviders.Remove(provider);
                    db.SaveChanges();

                    IQueryable<ProviderViewModel> listOfProviders = db.tblProviders.Where(p => p.UserID == owner.UserID).Select(o => new ProviderViewModel { ProviderID = o.ProviderID, ProviderCode = o.ProviderCode, FirstName = o.FirstName, LastName = o.LastName });

                    return Json(new { Result = "success", id = provider.ProviderID, errors = "", providerList = listOfProviders.OrderBy(o => o.LastName).ThenBy(o => o.FirstName) }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "error", id = providerId, errors = "Unknown Provider Name." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", id = providerId, errors = "Unknown error." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FilterMyUserList(FormCollection collection)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (submitter != null)
            {
                string selectedRoleID = collection["userRoles"];
                int selectedUserId = Convert.ToInt32(collection["filterName"]);
                string selectedDistrict = collection["userDistricts"] == "-1" ? null : collection["userDistricts"];
                string selectedBuilding = collection["userBuildings"] == "-1" ? null : collection["userBuildings"];
                int searchUserId = Convert.ToInt32(collection["filterName"]) == -1 ? 0 : Convert.ToInt32(collection["filterName"]);
                int? searchHasIEP = Convert.ToInt32(collection["filterActive"]) == 1 ? 1 : Convert.ToInt32(collection["filterActive"]) == 2 ? 0 : -1;
                int statusActive = collection["statusActive"] == "0" ? 0 : collection["statusActive"] == "1" ? 1 : -1;

                PortalViewModel model = new PortalViewModel
                {
                    user = submitter,
                    districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitter.UserID select district).Distinct().ToList(),
                    buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == submitter.UserID select building).Distinct().ToList(),
                    activeEducationalStatuses = db.tblStatusCodes.Where(o => o.Type == "Active").Select(o => o.StatusCode).ToList()
                };

                // owner - because he's special.
                if (submitter.UserID == 1)
                {
                    model.districts = db.tblDistricts.Distinct().ToList();
                    model.buildings = (from building in db.tblBuildings select building).Distinct().ToList();
                }

                if (selectedDistrict != null)
                {
                    model.buildings = model.buildings.Where(b => b.USD == selectedDistrict).ToList();
                }

                if (selectedRoleID != "999")
                {
                    model.members = db.uspUserAssignedList(submitter.UserID, selectedDistrict, selectedBuilding, null, null).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, hasIEP = u.hasIEP ?? false }).ToList();

                    if (searchUserId > 0)
                    {
                        model.members = model.members.Where(u => u.UserID == searchUserId).ToList();
                    }

                    if (selectedRoleID != "-1")
                    {
                        model.members = model.members.Where(u => u.RoleID == selectedRoleID).ToList();
                    }

                    if (searchHasIEP.HasValue && searchHasIEP != -1 && selectedRoleID == "5")
                    {
                        bool hasIEP = searchHasIEP == 1;
                        model.members = model.members.Where(u => u.hasIEP == hasIEP).ToList();
                    }

                    if (statusActive != -1 && selectedRoleID == "5") //educational status
                    {
                        model.members = model.members.Where(u => u.StatusActive == statusActive).ToList();
                    }

                }
                else
                {
                    model.members = db.uspUserAssignedList(submitter.UserID, selectedDistrict, selectedBuilding, null, true).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, KidsID = u.KIDSID.ToString(), StatusActive = u.StatusActive, StatusCode = u.StatusCode, hasIEP = u.hasIEP ?? false }).ToList();

                    if (searchUserId > 0)
                    {
                        model.members = model.members.Where(u => u.UserID == searchUserId).ToList();
                    }
                }

                ViewData["selectedUserRoles"] = selectedRoleID;
                ViewData["selectedFilterName"] = searchUserId;
                ViewData["selectedDistricts"] = selectedDistrict == null ? "-1" : selectedDistrict;
                ViewData["selectedBuildings"] = selectedBuilding;
                ViewData["selectedFilterActive"] = searchHasIEP;
                ViewData["selectedStatusActive"] = statusActive;

                if (submitter.RoleID == owner)
                {
                    return View("OwnerPortal", model);
                }
                else if(submitter.RoleID == mis)
                {
                    return View("MISPortal", model);
                }
                else if (submitter.RoleID == admin)
                {
                    return View("AdminPortal", model);
                }
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetProviderDistrict(int providerId)
        {
            List<string> districts = db.tblProviderDistricts.Where(p => p.ProviderID == providerId).Select(p => p.USD).ToList();

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

                    db.tblArchiveCalendars.Add(new tblArchiveCalendar()
                    {
                        USD = calendar.USD,
                        BuildingID = calendar.BuildingID,
                        Year = calendar.Year,
                        Month = calendar.Month,
                        Day = calendar.Day,
                        NoService = calendar.NoService,
                        canHaveClass = calendar.canHaveClass,
                        calendarDate = calendar.calendarDate,
                        SchoolYear = calendar.SchoolYear,
                        Create_Date = calendar.Create_Date,
                        Update_Date = calendar.Update_Date,
                        CreatedBy = calendar.CreatedBy,
                        ModifiedBy = MIS.UserID
                    });


                    calendar.canHaveClass = hasSchool;
                    calendar.ModifiedBy = MIS.UserID;
                    calendar.Update_Date = DateTime.Now;

                    string updateValues = string.Format("Calendar Update for USD: {0} BuildingID: {1} Date: {2}", usd, bId, calendar.calendarDate.HasValue ? calendar.calendarDate.Value.ToShortDateString() : "");

                    db.tblAuditLogs.Add(new tblAuditLog() { IEPid = null, ModifiedBy = MIS.UserID, Create_Date = DateTime.Now, TableName = "tblCalendar ", ColumnName = "canHaveClass", UserID = null, Update_Date = DateTime.Now, Value = updateValues });

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

                string saveStuff = "INSERT INTO [tblCalendar] ([USD], [BuildingID], [Year], [Month], [Day], [NoService], [canHaveClass], [CreatedBy] ) SELECT @USD, @BuildingID, [Year], [Month], [Day], [NoService], [canHaveClass], @UserId FROM [dbo].[tblCalendarTemplate]";
                using (SqlCommand querySaveStuff = new SqlCommand(saveStuff))
                {
                    querySaveStuff.Connection = SQLConn;
                    querySaveStuff.Parameters.Clear();
                    querySaveStuff.Parameters.AddWithValue("@USD", usd);
                    querySaveStuff.Parameters.AddWithValue("@BuildingID", bId);
                    querySaveStuff.Parameters.AddWithValue("@UserId", MIS.UserID);
                    querySaveStuff.ExecuteNonQuery();
                }

                string saveMoreStuff = "INSERT INTO [tblCalendarReporting] ([USD], [BuildingID], [SchoolYear]) SELECT DISTINCT @USD, @BuildingID, SchoolYear FROM [dbo].[tblCalendarTemplate] ORDER BY SchoolYear";
                using (SqlCommand querySaveMoreStuff = new SqlCommand(saveMoreStuff))
                {
                    querySaveMoreStuff.Connection = SQLConn;
                    querySaveMoreStuff.Parameters.Clear();
                    querySaveMoreStuff.Parameters.AddWithValue("@USD", usd);
                    querySaveMoreStuff.Parameters.AddWithValue("@BuildingID", bId);
                    querySaveMoreStuff.Parameters.AddWithValue("@UserId", MIS.UserID);
                    querySaveMoreStuff.ExecuteNonQuery();
                }

                db.tblAuditLogs.Add(new tblAuditLog() { IEPid = null, ModifiedBy = MIS.UserID, Create_Date = DateTime.Now, TableName = "tblCalendar tblCalendarReporting", UserID = null, Update_Date = DateTime.Now, Value = "CopyCalendar - INSERT" });
                db.SaveChanges();
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
                string district = collection["district"]; //from
                string building = collection["building"]; //from 
                string year = string.IsNullOrEmpty(collection["year"]) ? "0" : collection["year"].ToString(); //from
                short schoolYear = Convert.ToInt16(year);


                string[] selectedDistricts = collection["selectedDistrict[]"].Split(',').Distinct().ToArray(); //copy to
                string[] selectedBuildings = collection["selectedBuilding[]"].Split(','); //copy to

                for (int i = 0; i < selectedDistricts.Length; i++)
                {

                    foreach (string selectedBuilding in selectedBuildings)
                    {
                        string districtUSD = selectedDistricts[i].ToString();
                        int calendarExists = db.tblCalendars.Count(c => c.USD == districtUSD && c.BuildingID == selectedBuilding);

                        if (calendarExists == 0)
                        {
                            //if calendar does not exist, first create calendar from template, the update
                            CopyCalendar(districtUSD, selectedBuilding, MIS);
                        }

                        using (SqlConnection SQLConn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndividualizedEducationProgramConnectionString"].ConnectionString))
                        {
                            if (SQLConn.State != ConnectionState.Open) { SQLConn.Open(); }

                            //archive
                            string getChanges = @"select Cal_Upd.calendarID
											FROM tblCalendar Cal_Upd
											CROSS JOIN tblCalendar Cal_Orig
											WHERE Cal_Orig.calendarDate = Cal_Upd.calendarDate 
											AND Cal_Orig.USD = @USD_Orig 
											AND Cal_Orig.BuildingID = @BuildingID_Orig 
											AND Cal_Upd.USD = @USD_Upd 
											AND Cal_Upd.BuildingID = @BuildingID_Upd 
                                            AND Cal_Orig.SchoolYear = @SchoolYear                                           
											AND(Cal_Orig.canHaveClass != Cal_Upd.canHaveClass OR Cal_Orig.NoService != Cal_Upd.NoService)";

                            using (SqlCommand queryGetChanges = new SqlCommand(getChanges))
                            {
                                queryGetChanges.Connection = SQLConn;
                                queryGetChanges.Parameters.Clear();
                                queryGetChanges.CommandTimeout = 180;
                                queryGetChanges.Parameters.AddWithValue("@USD_Orig", district);
                                queryGetChanges.Parameters.AddWithValue("@BuildingID_Orig", building);
                                queryGetChanges.Parameters.AddWithValue("@USD_Upd", selectedDistricts[i]);
                                queryGetChanges.Parameters.AddWithValue("@BuildingID_Upd", selectedBuilding);
                                queryGetChanges.Parameters.AddWithValue("@SchoolYear", schoolYear);

                                using (var reader = queryGetChanges.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var calId = (int)reader.GetValue(0);

                                        var ac = db.tblCalendars.Where(o => o.calendarID == calId).FirstOrDefault();
                                        if (ac != null)
                                        {
                                            db.tblArchiveCalendars.Add(new tblArchiveCalendar()
                                            {
                                                USD = ac.USD,
                                                BuildingID = ac.BuildingID,
                                                Year = ac.Year,
                                                Month = ac.Month,
                                                Day = ac.Day,
                                                NoService = ac.NoService,
                                                canHaveClass = ac.canHaveClass,
                                                calendarDate = ac.calendarDate,
                                                SchoolYear = ac.SchoolYear,
                                                Create_Date = ac.Create_Date,
                                                Update_Date = ac.Update_Date,
                                                CreatedBy = ac.CreatedBy,
                                                ModifiedBy = MIS.UserID
                                            });
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }


                            string saveStuff = @"UPDATE Cal_Upd 
											SET 
											  Cal_Upd.[NoService] = Cal_Orig.[NoService]
											, Cal_Upd.[canHaveClass] = Cal_Orig.[canHaveClass] 
                                            ,Cal_Upd.ModifiedBy = @ModifiedBy
											FROM tblCalendar Cal_Upd
											CROSS JOIN tblCalendar Cal_Orig
											WHERE Cal_Orig.calendarDate = Cal_Upd.calendarDate 
											AND Cal_Orig.USD = @USD_Orig 
											AND Cal_Orig.BuildingID = @BuildingID_Orig 
											AND Cal_Upd.USD = @USD_Upd 
											AND Cal_Upd.BuildingID = @BuildingID_Upd 
                                            AND Cal_Orig.SchoolYear = @SchoolYear                                           
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
                                querySaveStuff.Parameters.AddWithValue("@SchoolYear", schoolYear);
                                querySaveStuff.Parameters.AddWithValue("@ModifiedBy", MIS.UserID);
                                querySaveStuff.ExecuteNonQuery();
                            }

                            string saveMoreStuff = @"UPDATE CalR_Upd 
											SET CalR_Upd.DaysPerWeek = CalR_Orig.DaysPerWeek
											, CalR_Upd.TotalDays = CalR_Orig.TotalDays
											, CalR_Upd.TotalWeeks = CalR_Orig.TotalWeeks
                                            , CalR_Upd.ModifiedBy = @ModifiedBy
											FROM tblCalendarReporting CalR_Upd
											CROSS JOIN tblCalendarReporting CalR_Orig
											WHERE
											CalR_Orig.SchoolYear = CalR_Upd.SchoolYear
											AND CalR_Orig.USD = @USD_Orig
											AND CalR_Orig.BuildingID = @BuildingID_Orig
                                            AND CalR_Orig.SchoolYear = @SchoolYear 
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
                                querySaveMoreStuff.Parameters.AddWithValue("@SchoolYear", schoolYear);
                                querySaveMoreStuff.Parameters.AddWithValue("@ModifiedBy", MIS.UserID);
                                querySaveMoreStuff.ExecuteNonQuery();
                            }


                        }
                    }
                }

                string info = string.Format("CopyOverToCalendars District: {0} Building: {1} to Districts: {2} Building {3}", district, building, string.Join(",", selectedDistricts), string.Join(",", selectedBuildings));
                db.tblAuditLogs.Add(new tblAuditLog() { IEPid = null, ModifiedBy = MIS.UserID, Create_Date = DateTime.Now, TableName = "tblCalendar, tblCalendarReporting", UserID = null, Update_Date = DateTime.Now, Value = info });
                db.SaveChanges();

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
                    reports = new tblCalendarReporting
                    {
                        USD = usd,
                        BuildingID = building,
                        SchoolYear = schoolYear,
                        DaysPerWeek = daysPerWeek,
                        TotalDays = totalDays,
                        TotalWeeks = totalWeeks,
                        MinutesPerDay = minutesPerDay
                    };

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

                List<tblUser> query = (from u in db.tblUsers
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
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            bool isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

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
                            ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_HealthSection", healthModel);
                        }
                        else
                        {
                            return PartialView("_ModuleHealthSection", healthModel);
                        }

                    case "AcademicModule":
                        ModuleAcademicViewModel academicModel = new ModuleAcademicViewModel
                        {
                            Academic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == iep.IEPAcademicID).FirstOrDefault(),
                            Reading = db.tblIEPReadings.Where(r => r.IEPReadingID == iep.IEPReadingID).FirstOrDefault(),
                            Math = db.tblIEPMaths.Where(m => m.IEPMathID == iep.IEPMathID).FirstOrDefault(),
                            Written = db.tblIEPWrittens.Where(w => w.IEPWrittenID == iep.IEPWrittenID).FirstOrDefault()
                        };

                        if (academicModel.Academic == null) { academicModel.Academic = new tblIEPAcademic(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Academic.ModifiedBy).SingleOrDefault(); ViewBag.academicModifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Reading == null) { academicModel.Reading = new tblIEPReading(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Reading.ModifiedBy).SingleOrDefault(); ViewBag.readingModifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Math == null) { academicModel.Math = new tblIEPMath(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Math.ModifiedBy).SingleOrDefault(); ViewBag.mathModifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }
                        if (academicModel.Written == null) { academicModel.Written = new tblIEPWritten(); } else { modifier = db.tblUsers.Where(u => u.UserID == academicModel.Written.ModifiedBy).SingleOrDefault(); ViewBag.writtenModifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null; }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_AcademicSection", academicModel);
                        }
                        else
                        {
                            return PartialView("_ModuleAcademicSection", academicModel);
                        }

                    case "MotorModule":
                        tblIEPMotor motorModel = db.tblIEPMotors.Where(m => m.IEPMotorID == iep.IEPMotorID).FirstOrDefault();
                        if (motorModel == null)
                        {
                            motorModel = new tblIEPMotor();
                        }
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == motorModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_MotorSection", motorModel);
                        }
                        else
                        {
                            return PartialView("_ModuleMotorSection", motorModel);
                        }

                    case "CommunicationModule":
                        tblIEPCommunication communicationModel = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == iep.IEPCommunicationID).FirstOrDefault();
                        if (communicationModel == null)
                        {
                            communicationModel = new tblIEPCommunication();
                        }
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == communicationModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_CommunicationSection", communicationModel);
                        }
                        else
                        {
                            return PartialView("_ModuleCommunicationSection", communicationModel);
                        }

                    case "SocialModule":
                        tblIEPSocial socialModel = db.tblIEPSocials.Where(s => s.IEPSocialID == iep.IEPSocialID).FirstOrDefault();
                        if (socialModel == null)
                        {
                            socialModel = new tblIEPSocial();
                        }
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == socialModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_SocialSection", socialModel);
                        }
                        else
                        {
                            return PartialView("_ModuleSocialSection", socialModel);
                        }

                    case "GeneralIntelligenceModule":
                        tblIEPIntelligence intelligenceModel = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == iep.IEPIntelligenceID).FirstOrDefault();
                        if (intelligenceModel == null)
                        {
                            intelligenceModel = new tblIEPIntelligence();
                        }
                        else
                        { // Load the modified by info
                            modifier = db.tblUsers.Where(u => u.UserID == intelligenceModel.ModifiedBy).SingleOrDefault();
                            ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                        }

                        if (isReadOnly)
                        {
                            return PartialView("ActiveIEP/_GeneralIntelligenceSection", intelligenceModel);
                        }
                        else
                        {
                            return PartialView("_ModuleGeneralIntelligenceSection", intelligenceModel);
                        }

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

                IQueryable<tblUser> teachers = (from org in db.tblOrganizationMappings
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
            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            ViewBag.UserRoleId = currentUser.RoleID;

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
        public ActionResult StudentProcedures(int stid, int? iepID = null, int? rtnId = null)
        {
            StudentProcedureViewModel model = new StudentProcedureViewModel();

            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            tblUser student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();
            tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == student.UserID).FirstOrDefault();
            tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
            tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();
            tblStudentRelationship relationship = db.tblStudentRelationships.Where(r => r.UserID == stid && r.PrimaryContact == 1).FirstOrDefault();

            ViewBag.UserRoleId = currentUser.RoleID;
            ViewBag.ReturnBtn = rtnId.HasValue ? string.Format("/Home/TeacherStudentsRole/{0}", rtnId.Value) : ""; //re-route back button
            ViewBag.studentDistrict = district.USD ?? "101";

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

                model.KIDSID = info.KIDSID.ToString();
                model.PRIMARYPARENT = relationship != null;
                model.STUDENTGRADE = theIEP.studentGrade;
                model.STUDENTCODE = theIEP.studentCode;
                model.STUDENTSERVICES = db.tblServices.Where(s => s.IEPid == iepID).OrderBy(s => s.StartDate).ThenBy(s => s.ServiceCode).ToList();
                model.studentAge = theIEP.GetCalculatedAge(info.DateOfBirth, model.isDoc);

                // need to check if transition plan is required and completed
                if (theIEP.isTransitionNeeded(model.studentAge, model.isDoc) && !model.isGiftedOnly && (theIEP.iepStatusType == "DRAFT" || theIEP.iepStatusType == "AMENDMENT"))
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
            tblUser submitter = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == stId && i.IEPid == IepId).FirstOrDefault();

            if (iep != null)
            {
                if (DateTime.TryParseExact(IEPStartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    if (DateTime.TryParseExact(IEPMeetingDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime meetingDate))
                    {
                        iep.begin_date = startDate;
                        iep.MeetingDate = meetingDate;

                        if (iep.IepStatus.ToUpper() == IEPStatus.DRAFT && iep.Amendment)
                        {
                            iep.begin_date = meetingDate;
                        }

                        db.tblAuditLogs.Add(new tblAuditLog() { IEPid = IepId, ModifiedBy = submitter.UserID, Create_Date = DateTime.Now, TableName = "tblIEP", UserID = stId, Update_Date = DateTime.Now, Value = "Updated IEP" });
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

                //copy over updated progress reports information
                var activeGoals = db.tblGoals.Where(o => o.IEPid == studentAmmendIEP.AmendingIEPid);
                //&& (o.Update_Date >= studentAmmendIEP.Create_Date || o.Create_Date >= studentAmmendIEP.Create_Date));

                var amendGoals = db.tblGoals.Where(o => o.IEPid == IEPid);

                foreach (var activeGoal in activeGoals)
                {
                    var amendGoal = amendGoals.Where(o => o.Module == activeGoal.Module && o.Title == activeGoal.Title).FirstOrDefault();
                    if (amendGoal != null)
                    {
                        amendGoal.Progress_Quarter1 = activeGoal.Progress_Quarter1;
                        amendGoal.Progress_Quarter2 = activeGoal.Progress_Quarter2;
                        amendGoal.Progress_Quarter3 = activeGoal.Progress_Quarter3;
                        amendGoal.Progress_Quarter4 = activeGoal.Progress_Quarter4;
                        amendGoal.ProgressDate_Quarter1 = activeGoal.ProgressDate_Quarter1;
                        amendGoal.ProgressDate_Quarter2 = activeGoal.ProgressDate_Quarter2;
                        amendGoal.ProgressDate_Quarter3 = activeGoal.ProgressDate_Quarter3;
                        amendGoal.ProgressDate_Quarter4 = activeGoal.ProgressDate_Quarter4;
                        amendGoal.ProgressDescription_Quarter1 = activeGoal.ProgressDescription_Quarter1;
                        amendGoal.ProgressDescription_Quarter2 = activeGoal.ProgressDescription_Quarter2;
                        amendGoal.ProgressDescription_Quarter3 = activeGoal.ProgressDescription_Quarter3;
                        amendGoal.ProgressDescription_Quarter4 = activeGoal.ProgressDescription_Quarter4;
                    }
                }

                if (activeGoals != null && activeGoals.Count() > 0)
                {

                    var activeGoalIds = activeGoals.Select(o => o.goalID).ToList();
                    var amendeGoalIds = amendGoals.Select(o => o.goalID).ToList();
                    var activeGoalBenchmarks = db.tblGoalBenchmarks.Where(o => activeGoalIds.Contains(o.goalID)).ToList();
                    var amendGoalBenchmarks = db.tblGoalBenchmarks.Where(o => amendeGoalIds.Contains(o.goalID)).ToList();

                    foreach (var activeGoalBenchmark in activeGoalBenchmarks)
                    {
                        var amendGoalBenchmark = amendGoalBenchmarks.Where(o => o.Method == activeGoalBenchmark.Method
                        && o.ObjectiveBenchmark == activeGoalBenchmark.ObjectiveBenchmark).FirstOrDefault();

                        if (amendGoalBenchmark != null)
                        {
                            amendGoalBenchmark.Progress_Quarter1 = activeGoalBenchmark.Progress_Quarter1;
                            amendGoalBenchmark.Progress_Quarter2 = activeGoalBenchmark.Progress_Quarter2;
                            amendGoalBenchmark.Progress_Quarter3 = activeGoalBenchmark.Progress_Quarter3;
                            amendGoalBenchmark.Progress_Quarter4 = activeGoalBenchmark.Progress_Quarter4;
                            amendGoalBenchmark.ProgressDate_Quarter1 = activeGoalBenchmark.ProgressDate_Quarter1;
                            amendGoalBenchmark.ProgressDate_Quarter2 = activeGoalBenchmark.ProgressDate_Quarter2;
                            amendGoalBenchmark.ProgressDate_Quarter3 = activeGoalBenchmark.ProgressDate_Quarter3;
                            amendGoalBenchmark.ProgressDate_Quarter4 = activeGoalBenchmark.ProgressDate_Quarter4;
                            amendGoalBenchmark.ProgressDescription_Quarter1 = activeGoalBenchmark.ProgressDescription_Quarter1;
                            amendGoalBenchmark.ProgressDescription_Quarter2 = activeGoalBenchmark.ProgressDescription_Quarter2;
                            amendGoalBenchmark.ProgressDescription_Quarter3 = activeGoalBenchmark.ProgressDescription_Quarter3;
                            amendGoalBenchmark.ProgressDescription_Quarter4 = activeGoalBenchmark.ProgressDescription_Quarter4;
                        }
                    }

                }

                //archive previously active iep and progress
                IEP theIEP = GetIEPPrint(stId, studentActiveIEP.IEPid);

                bool success = ArchiveIEPPrint(stId, theIEP, true);

                if (!success)
                {
                    return Json(new { Result = "error", Message = "There was a problem creating the IEP Archive" }, JsonRequestBehavior.AllowGet);
                }

                // find the current active iep and make it inactive and change its status to DELETED
                studentActiveIEP.IepStatus = IEPStatus.ARCHIVE;
                studentActiveIEP.IsActive = false;
                studentAmmendIEP.IepStatus = IEPStatus.ACTIVE;

                //iep status code history just in case the teacher changed it
                tblStudentInfo studentRec = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();

                if (studentRec != null)
                {
                    studentAmmendIEP.StatusCode = studentRec.StatusCode;                   
                }

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


        [HttpGet]
        [Authorize(Roles = mis + "," + admin + "," + teacher)]
        public ActionResult UpdateIEPAnnualToActive(int stId, int IEPid)
        {

            tblIEP studentActiveIEP = db.tblIEPs.Where(i => i.UserID == stId && i.IepStatus == IEPStatus.ACTIVE).FirstOrDefault();
            tblIEP studentAnnualIEP = db.tblIEPs.Where(i => i.UserID == stId && i.IEPid == IEPid).FirstOrDefault();

            if (studentAnnualIEP == null)
            {
                return Json(new { Result = "error", Message = "No annual IEP found for this student." }, JsonRequestBehavior.AllowGet);
            }

            if (studentActiveIEP == null)
            {

                studentAnnualIEP.IepStatus = IEPStatus.ACTIVE;
                studentAnnualIEP.IsActive = true;

                try
                {
                    //iep status code history
                    tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();
                    if (studentDetails != null)
                    {
                        studentAnnualIEP.StatusCode = studentDetails.StatusCode;
                        studentAnnualIEP.Primary_DisabilityCode = studentDetails.Primary_DisabilityCode;
                        studentAnnualIEP.Secondary_DisabilityCode = studentDetails.Secondary_DisabilityCode;
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "The IEP status is Active." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {

                //archive previously active iep and progress
                IEP theIEP = GetIEPPrint(stId, studentActiveIEP.IEPid);

                bool success = ArchiveIEPPrint(stId, theIEP, true);

                if (!success)
                {
                    return Json(new { Result = "error", Message = "There was a problem creating the IEP Archive" }, JsonRequestBehavior.AllowGet);
                }


                // find the current active iep and make it inactive and change its status to DELETED
                studentActiveIEP.IepStatus = IEPStatus.ARCHIVE;
                studentActiveIEP.IsActive = false;


                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Error. " + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }

                studentAnnualIEP.IepStatus = IEPStatus.ACTIVE;
                studentAnnualIEP.IsActive = true;

                try
                {
                    tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();
                    if (studentDetails != null)
                    {
                        studentAnnualIEP.StatusCode = studentDetails.StatusCode;
                        studentAnnualIEP.Primary_DisabilityCode = studentDetails.Primary_DisabilityCode;
                        studentAnnualIEP.Secondary_DisabilityCode = studentDetails.Secondary_DisabilityCode;
                    }

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
                    if (iepDraft.IepStatus == IEPStatus.DRAFT && iepDraft.FiledOn != null)
                    {
                        iepDraft.FiledOn = null; //reset spedpro status so it will download again
                    }                    

                    // start switching the flag.
                    iepDraft.IepStatus = IEPStatus.ACTIVE;

                    //iepDraft.begin_date = DateTime.Now;
                    iepDraft.end_Date = (!iepDraft.Amendment) ? iepDraft.begin_date.Value.AddYears(1) : iepDraft.end_Date;

                    try
                    {
                        tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == stId).FirstOrDefault();
                        if (studentDetails != null)
                        {                            
                            iepDraft.Primary_DisabilityCode = studentDetails.Primary_DisabilityCode;
                            iepDraft.Secondary_DisabilityCode = studentDetails.Secondary_DisabilityCode;
                        }
                                                
                        db.SaveChanges();

                        return Json(new { Result = "success", Message = "IEP Status changed to Active." }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "Error. " + e.Message.ToString() }, JsonRequestBehavior.AllowGet);
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

                db.SaveChanges();

                tblIEP activeIEP = db.tblIEPs.Where(i => i.UserID == Stid && i.IepStatus.ToUpper() == IEPStatus.ACTIVE).FirstOrDefault();

                return Json(new { Result = "success", Message = "IEP is archived.", ActiveIEP = activeIEP != null ? activeIEP.IEPid : 0 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Result = "error", Message = "Unknown Error. Unable make the IEP Inactive." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public ActionResult UpdateRevertIEPtoDraft(int Stid, int IepId, string MyReason)
        {
            tblUser submitter = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
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
                    db.tblAuditLogs.Add(new tblAuditLog() { TableName = "tblIEP", ColumnName = "IEPStatus", Update_Date = DateTime.Now, Create_Date = DateTime.Now, IEPid = IepId, Value = MyReason, UserID = submitter.UserID, ModifiedBy = submitter.UserID });

                    studentActiveIEP.IepStatus = IEPStatus.DRAFT;
                    studentActiveIEP.begin_date = null;
                    studentActiveIEP.MeetingDate = null;
                    studentActiveIEP.Update_Date = DateTime.Now;
                    db.SaveChanges();

                    //revert old copy to make it active again
                    tblIEP tblArchivedIEP = db.tblIEPs.Where(i => i.IEPid == studentActiveIEP.AmendingIEPid && i.UserID == studentActiveIEP.UserID).FirstOrDefault();
                    if (tblArchivedIEP != null)
                    {
                        db.tblAuditLogs.Add(new tblAuditLog() { TableName = "tblIEP", ColumnName = "IEPStatus", Update_Date = DateTime.Now, Create_Date = DateTime.Now, IEPid = tblArchivedIEP.IEPid, Value = "Changing status of archived iep to Active due to current active iep being reverted to draft.", UserID = submitter.UserID, ModifiedBy = submitter.UserID });

                        tblArchivedIEP.IepStatus = IEPStatus.ACTIVE;
                        tblArchivedIEP.ModifiedBy = submitter.UserID;
                        tblArchivedIEP.IsActive = true;
                        studentActiveIEP.begin_date = null;
                        studentActiveIEP.MeetingDate = null;
                        studentActiveIEP.Update_Date = DateTime.Now;
                        db.SaveChanges();
                    }

                    return Json(new { Result = "success", Message = "IEP is reverted." }, JsonRequestBehavior.AllowGet);
                }


                return Json(new { Result = "error", Message = "There is already another Draft in play. Unable make to revert this IEP" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable make to revert this IEP" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateStudentIEPGrade(int grade = 0, int stid = 0, int iepId = 0)
        {
            if (grade != 0 && stid != 0 && iepId != 0)
            {
                tblIEP iep = db.tblIEPs.Where(i => i.IEPid == iepId && i.UserID == stid).FirstOrDefault();
                if (iep != null)
                {
                    iep.Grade = grade;
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "Student's IEP Grade Updated" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "error", Message = "Unknown Error. Unable to change the student's iep grade" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateStudentCode(string code, int stid = 0, int iepId = 0)
        {
            if (code != null && stid != 0 && iepId != 0)
            {
                tblIEP iep = db.tblIEPs.Where(i => i.IEPid == iepId && i.UserID == stid).FirstOrDefault();
                if (iep != null)
                {
                    iep.StatusCode = code;
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "Student's Code Updated" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Result = "error", Message = "Unknown Error. Unable to change the student's code" }, JsonRequestBehavior.AllowGet);
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
            bool isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

            if (iep != null)
            {
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
                ViewBag.studentName = student.FirstName + " " + student.LastName;

                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                StudentGoalsViewModel model = new StudentGoalsViewModel
                {
                    studentId = studentId,
                    iepId = iep.IEPid,
                    isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse),
                    canAddProgress = teacher == null || teacher.RoleID != nurse
                };

                List<vw_ModuleGoalFlags> GoalFlag = db.vw_ModuleGoalFlags.Where(vm => vm.IEPid == iep.IEPid).ToList();
                model.modulesNeedingGoals = GoalFlag.Where(vm => vm.Module == "Health").FirstOrDefault().NeedMetByGoal.Value ? "Health " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Motor").FirstOrDefault().NeedMetByGoal.Value ? "Motor " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Communication").FirstOrDefault().NeedMetByGoal.Value ? "Communication " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Social").FirstOrDefault().NeedMetByGoal.Value ? "Social-Emotional " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Math").FirstOrDefault().NeedMetByGoal.Value ? "Math " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Reading").FirstOrDefault().NeedMetByGoal.Value ? "Reading " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Written").FirstOrDefault().NeedMetByGoal.Value ? "Written&nbsp;Language " : string.Empty;
                model.modulesNeedingGoals += GoalFlag.Where(vm => vm.Module == "Academic").FirstOrDefault().NeedMetByGoal.Value ? "Academic/Functional" : string.Empty;

                List<string> modulesList = db.tblModules.OrderBy(o => o.ModuleName).Select(o => o.ModuleID.ToString()).ToList();
                List<tblGoal> goals = db.tblGoals.Where(g => g.IEPid == iep.IEPid).ToList().OrderBy(d => modulesList.IndexOf(d.Module)).ToList();
                int? modifiedby = (goals.Count > 0) ? goals.FirstOrDefault().ModifiedBy : null;
                if (modifiedby != null)
                {
                    tblUser modifier = db.tblUsers.Where(u => u.UserID == modifiedby).SingleOrDefault();
                    ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
                    ViewBag.modifiedByDate = goals.FirstOrDefault().Update_Date;
                }

                foreach (tblGoal goal in goals)
                {
                    StudentGoal studentGoal = new StudentGoal(goal.goalID);

                    model.studentGoals.Add(studentGoal);
                    IQueryable<tblGoalBenchmark> benchmarks = db.tblGoalBenchmarks.Where(o => o.goalID == goal.goalID);

                    foreach (tblGoalBenchmark benchmark in benchmarks)
                    {
                        List<tblGoalBenchmarkMethod> shortBenchmarks = db.tblGoalBenchmarkMethods.Where(o => o.goalBenchmarkID == benchmark.goalBenchmarkID).ToList();
                        studentGoal.shortTermBenchmarkMethods.AddRange(shortBenchmarks);
                    }

                }

                if (!isReadOnly)
                {
                    return PartialView("_ModuleStudentGoals", model);
                }
                else
                {
                    return PartialView("ActiveIEP/_StudentGoals", model);
                }
            }

            return PartialView("_ModuleStudentGoals", new StudentGoalsViewModel());
        }

        [HttpGet]
        [Authorize]
        public ActionResult MISNotes(int stid)
        {
            MISNotesViewModel viewModel = new MISNotesViewModel();

            List<MISNotesUI> misnotes = (from note in db.tblStudentNotes_MIS
                                         join user in db.tblUsers on note.CreatedBy equals user.UserID
                                         where note.isArchive == false && note.StudentID == stid
                                         select new MISNotesUI
                                         {
                                             CommentId = note.StudentNoteMISID,
                                             Note = note.Note,
                                             StudentID = note.StudentID,
                                             CreatedBy = note.CreatedBy,
                                             isArchive = note.isArchive,
                                             Create_Date = note.Create_Date,
                                             ModifiedBy = note.ModifiedBy.Value,
                                             Update_Date = note.Update_Date,
                                             FirstName = user.FirstName,
                                             LastName = user.LastName
                                         }).ToList();

            viewModel.notes = misnotes;
            viewModel.student = db.tblUsers.Where(u => u.UserID == stid).FirstOrDefault();

            return View(viewModel);
        }


        public ActionResult SaveMISNote(FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["stid"]);
            string comment = collection["Message"].ToString();

            try
            {
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();

                if (student != null)
                {
                    tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                    tblStudentNotes_MIS note = new tblStudentNotes_MIS
                    {
                        CreatedBy = user.UserID,
                        ModifiedBy = user.UserID,
                        isArchive = false,
                        Note = comment,
                        StudentID = studentId,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };

                    db.tblStudentNotes_MIS.Add(note);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new EmailException("Encountered an error while adding the MIS note.", e);
            }

            return RedirectToAction("MISNotes", new { stid = studentId });
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteNote(int studentId, int commentId)
        {
            tblStudentNotes_MIS note = db.tblStudentNotes_MIS.Where(n => n.StudentID == studentId && n.StudentNoteMISID == commentId).FirstOrDefault();
            if (note != null)
            {
                note.isArchive = true;
                db.SaveChanges();

                return RedirectToAction("MISNotes", new { stid = studentId });
            }
            else
            {
                return Json(new { Result = "error", Message = "This MIS note was not found in our database." }, JsonRequestBehavior.AllowGet);
            }
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
                    {
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear && s.ServiceID == serviceId).ToList();
                    }
                    else
                    {
                        services = db.tblServices.Where(s => s.IEPid == iep.IEPid && s.SchoolYear == currentYear).ToList();
                    }

                    List<StudentServiceObject> serviceList = new List<StudentServiceObject>();
                    foreach (tblService service in services)
                    {
                        List<tblCalendar> availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == service.BuildingID && c.canHaveClass == true && c.NoService == false && c.SchoolYear > service.SchoolYear && c.SchoolYear <= maxYear).OrderBy(c => c.SchoolYear).ThenBy(c => c.Month).ThenBy(c => c.Day).ToList();

                        tblCalendar firstDaySchoolYear = availableCalendarDays.Where(c => c.Month >= startMonth).OrderBy(c => c.Month).ThenBy(c => c.Day).First();
                        tblCalendar lastDaySchoolYear = availableCalendarDays.Where(c => c.Month <= endMonth).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

                        StudentServiceObject item = new StudentServiceObject();
                        byte meetingDate = item.DaysPerWeek = service.DaysPerWeek;
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
                            foreach (tblGoal goal in service.tblGoals)
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
            tblIEP original = (iep.OriginalIEPid.HasValue) ? db.tblIEPs.Where(i => i.IEPid == iep.OriginalIEPid.Value).FirstOrDefault() : iep;

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse);

                StudentServiceViewModel model = new StudentServiceViewModel();
                tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
                tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

                ViewBag.studentName = student.FirstName + " " + student.LastName;
                ViewBag.isMIS = mis.UserID == teacher.UserID;
                ViewBag.isOwner = teacher.UserID == 1;

                List<tblProvider> providers = (from p in db.tblProviders
                                               join d in db.tblProviderDistricts on p.ProviderID equals d.ProviderID
                                               where d.USD != null && d.USD == studentInfo.AssignedUSD
                                               select p).OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

                List<tblService> services = db.tblServices.Where(s => s.IEPid == iep.IEPid).OrderBy(s => s.StartDate).ThenBy(s => s.ServiceCode).ToList();

                if (services != null)
                {
                    var studentUSDList = studentInfo.USD.Split(',').ToList();
                    var previousBuildings = services.Where(o => !studentUSDList.Contains(o.USD)).Select(o => o.BuildingID).ToList();

                    model.studentId = studentId;
                    model.studentServices = services;
                    model.serviceTypes = db.tblServiceTypes.ToList();
                    model.serviceProviders = providers;
                    model.serviceLocations = db.tblLocations.ToList();
                    model.attendanceBuildings = db.vw_BuildingsForAttendance.Where(b => b.userID == student.UserID).Distinct().ToList();
                    model.previousAttendanceBuildings = db.tblBuildings.Where(o => previousBuildings.Contains(o.BuildingID)).Distinct().ToList();
                    model.studentGoals = db.tblGoals.Where(g => g.IEPid == iep.IEPid && g.hasSerivce == true).ToList();
                    model.IEPStartDate = original.begin_date ?? DateTime.Now;
                    model.MeetingDate = iep.MeetingDate ?? DateTime.Now;
                    model.CreateDate = iep.Create_Date;
                    model.isOriginalIEPService = iep.IepStatus.ToUpper() == IEPStatus.DRAFT && iep.Amendment;
                    model.IEPStatus = iep.IepStatus.ToUpper();
                    model.primaryProviderId = iep.PrimaryProviderID;
                    ViewBag.ServiceStartDate = iep.begin_date;

                    int? modifiedby = (services.Count > 0) ? services.FirstOrDefault().ModifiedBy : null;
                    if (modifiedby != null)
                    {
                        tblUser modifier = db.tblUsers.Where(u => u.UserID == modifiedby).SingleOrDefault();
                        ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
                    model.IEPStatus = iep.IepStatus.ToUpper();
                }

                if (isReadOnly && !ViewBag.IsOwner)
                {
                    return PartialView("ActiveIEP/_StudentServices", model);
                }
                else
                {
                    return PartialView("_ModuleStudentServices", model);
                }
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
            IsValidDate(fiscalYear, calendarDay, buildingId, out bool isValid, out bool isService, out string validDates);

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

            tblCalendarReporting reporting = db.tblCalendarReportings.Where(r => r.BuildingID == studentInfo.BuildingID && r.USD == studentInfo.AssignedUSD && r.SchoolYear == fiscalYear).FirstOrDefault();

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
            IQueryable<tblCalendar> availableCalendarDays = db.tblCalendars.Where(c => c.BuildingID == buildingId && (c.canHaveClass == true && c.NoService == false) && c.SchoolYear == fiscalYear).OrderBy(o => o.calendarDate);

            tblCalendar firstDaySchoolYear = null;
            tblCalendar lastDaySchoolYear = null;

            if (availableCalendarDays != null)
            {
                firstDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == startMonth).FirstOrDefault();
                lastDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == endMonth).OrderByDescending(o => o.Day).FirstOrDefault();

                //keep looking for first day
                if (firstDaySchoolYear == null)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        startMonth++;
                        firstDaySchoolYear = availableCalendarDays.Where(o => o.SchoolYear == fiscalYear && o.Month == startMonth && o.Year == fiscalYear - 1).OrderBy(o => o.calendarDate).FirstOrDefault();
                        if (firstDaySchoolYear != null)
                        {
                            break;
                        }
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
                        {
                            break;
                        }
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
            int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;
            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == iepId).FirstOrDefault();
            if (iep != null)
            {
                if (StudentSerivceId == 0) // new service
                {
                    service = new tblService
                    {
                        IEPid = iep.IEPid,
                        BuildingID = collection["attendanceBuilding"].ToString()
                    };
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
                            int.TryParse(goalsArr[i], out int goalId);

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
                    IsValidDate(service.SchoolYear, service.EndDate.Value.ToShortDateString(), service.BuildingID, out isValidEndDate, out isValidServiceEndDate, out validDates);
                }
                else // exsisting service
                {
                    service = db.tblServices.Where(s => s.ServiceID == StudentSerivceId).FirstOrDefault();
                    service.BuildingID = collection["attendanceBuilding"].ToString();

                    var attendanceBuilding = db.vw_BuildingsForAttendance.Where(b => b.BuildingID == service.BuildingID && b.userID == studentId).FirstOrDefault();

                    if (attendanceBuilding == null)
                    {
                        var previousBuilding = db.tblBuildings.Where(o => o.BuildingID == service.BuildingID).FirstOrDefault();
                        if (previousBuilding != null)
                        {
                            attendanceBuilding = new vw_BuildingsForAttendance() { BuildingID = previousBuilding.BuildingID, USD = previousBuilding.USD };
                        }
                    }

                    service.USD = attendanceBuilding != null ? attendanceBuilding.USD : "";
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
                            int.TryParse(goalsArr[i], out int goalId);

                            if (goalId > 0)
                            {
                                tblGoal goal = db.tblGoals.Where(g => g.goalID == goalId).First();
                                service.tblGoals.Add(goal);
                            }
                        }
                    }

                    //check dates
                    IsValidDate(service.SchoolYear, service.StartDate.ToShortDateString(), service.BuildingID, out isValidStartDate, out isValidServiceStartDate, out validDates);
                    IsValidDate(service.SchoolYear, service.EndDate.Value.ToShortDateString(), service.BuildingID, out isValidEndDate, out isValidServiceEndDate, out validDates);
                }

                if (isValidStartDate && isValidServiceStartDate && isValidEndDate && isValidServiceEndDate)
                {
                    // just so we know if we are adding or editing a service in the auditlog.
                    string action = service.ServiceID == 0 ? "Adding new service" : "Editing service " + service.ServiceID.ToString();
                    AuditLog audit = new AuditLog(studentId, ModifiedBy, db, iepId) { Created = service.Create_Date, TableName = "tblService", ColumnName = "All Columns", SessionID = HttpContext.Session.SessionID, Value = action };
                    audit.SaveChanges();

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
                return Json(new { Result = "success", Message = "The service has been saved.", key = StudentSerivceId }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "false", Message = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteStudentService(int studentServiceId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            tblService service = db.tblServices.Where(s => s.ServiceID == studentServiceId).FirstOrDefault();
            if (service != null)
            {
                tblIEP iep = db.tblIEPs.Where(i => i.IEPid == service.IEPid).FirstOrDefault();
                db.tblServices.Remove(service);

                AuditLog audit = new AuditLog(iep.UserID, teacher.UserID, db, service.IEPid) { TableName = "tblService", ColumnName = "All Columns", Created = service.Create_Date, SessionID = HttpContext.Session.SessionID, Value = "Delete service " + service.ServiceID.ToString() };
                audit.SaveChanges();

                return Json(new { Result = "success", Message = "The Service has been delete." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "Unknown Error Occured." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetLastFiscalDay(int studentId, string fyYear)
        {
            tblCalendar lastDay = GetLastFiscalCalendarDay(studentId, fyYear);

            if (lastDay != null)
            {
                return Json(new { Result = "success", Value = lastDay.calendarDate.Value.ToString("MM/dd/yyyy") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Result = "success", Value = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        private tblCalendar GetLastFiscalCalendarDay(int studentId, string fyYear)
        {
            tblUser student = db.tblUsers.Where(s => s.UserID == studentId).FirstOrDefault();
            tblStudentInfo studentInfo = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();

            int.TryParse(fyYear, out int lastYear);

            List<tblCalendar> calendar = db.tblCalendars.Where(c => c.BuildingID == studentInfo.BuildingID && c.USD == studentInfo.USD && c.Year == lastYear).OrderBy(c => c.Year).ToList();

            tblCalendar lastDay = calendar.Where(c => c.canHaveClass && c.Year == lastYear && (c.Month == 6 || c.Month == 5)).OrderByDescending(c => c.Month).ThenByDescending(c => c.Day).First();

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

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse);

                tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).FirstOrDefault();
                tblDistrict district = db.tblDistricts.Where(d => d.USD == building.USD).FirstOrDefault();

                StudentTransitionViewModel model = new StudentTransitionViewModel
                {
                    studentId = studentId,
                    student = student,
                    isDOC = district.DOC,
                    iepId = iep.IEPid,
                    assessments = db.tblTransitionAssessments.Where(a => a.IEPid == iep.IEPid).ToList(),
                    services = db.tblTransitionServices.Where(s => s.IEPid == iep.IEPid).ToList(),
                    goals = db.tblTransitionGoals.Where(g => g.IEPid == iep.IEPid).ToList(),
                    transition = db.tblTransitions.Where(t => t.IEPid == iep.IEPid).FirstOrDefault() ?? new tblTransition()
                };

                int studentAge = theIEP.GetCalculatedAge(info.DateOfBirth, model.isDOC);

                model.isRequired = studentAge > 13 || (model.isDOC && studentAge <= 21);
                model.gender = info.Gender;
                model.careers = db.tblCareerPaths.Where(o => o.Active == true).ToList();

                bool hasEmploymentGoal = model.goals.Any(o => o.GoalType == "employment");
                bool hasEducationGoal = model.goals.Any(o => o.GoalType == "education");
                if (hasEmploymentGoal && hasEducationGoal)
                {
                    model.canComplete = true;
                }

                ViewBag.studentFirstName = studentFirstName;
                ViewBag.studentLastName = studentLastName;
                ViewBag.studentAge = studentAge;

                if (isReadOnly)
                {
                    return PartialView("ActiveIEP/_StudentTransition", model);
                }
                else
                {
                    return PartialView("_ModuleStudentTransition", model);
                }
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [HttpGet]
        [Authorize]
        public ActionResult StudentContingency(int studentId, int IEPid)
        {
            bool isReadOnly = false;
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            ContingencyPlanModel model = new ContingencyPlanModel();
            model.StudentId = studentId;
            model.IEPId = IEPid;
            model.Plan = db.tblContingencyPlans.Where(p => p.IEPid == IEPid).FirstOrDefault() ?? new tblContingencyPlan() { IEPid = IEPid, NoContingencyPlan = true, RemoteLearning_DistrictResponse = false, RemoteLearning_ParentRequest = false, Completed = false };

            tblIEP studentIEP = db.tblIEPs.Where(i => i.IEPid == IEPid).FirstOrDefault();
            if (studentIEP != null)
            {
                isReadOnly = (studentIEP.IepStatus == IEPStatus.ACTIVE) || (studentIEP.IepStatus == IEPStatus.ARCHIVE) || (teacher != null && teacher.RoleID == nurse);
            }

            if (isReadOnly)
            {
                return PartialView("ActiveIEP/_StudentContingency", model);
            }
            else
            {
                return PartialView("_ModuleStudentContingency", model);
            }
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

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

                BehaviorViewModel model = GetBehaviorModel(studentId, iep.IEPid);

                if (isReadOnly)
                {
                    return PartialView("ActiveIEP/_Behavior", model);
                }
                else
                {
                    return PartialView("_ModuleBehavior", model);
                }
            }

            return RedirectToAction("StudentProcedures", new { stid = studentId });
        }

        [Authorize]
        public ActionResult Accommodations(int studentId, int IEPid)
        {
            AccomodationViewModel model = new AccomodationViewModel();
            bool isReadOnly = false;

            tblIEP iep = db.tblIEPs.Where(i => i.UserID == studentId && i.IEPid == IEPid).FirstOrDefault();
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            List<SelectListItem> locationList = new List<SelectListItem>();
            model.ModuleList = db.tblModules.Where(o => o.Active == true).ToList();

            if (iep != null)
            {
                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

                model.StudentId = studentId;
                model.IEPid = iep.IEPid;

                List<vw_ModuleAccommodationFlags> accommodationFlag = db.vw_ModuleAccommodationFlags.Where(vm => vm.IEPid == iep.IEPid).ToList();
                model.modulesNeedingAccommodations = accommodationFlag.Where(vm => vm.Module == "Health").FirstOrDefault().NeedMetByAccommodation.Value ? "Health;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Motor").FirstOrDefault().NeedMetByAccommodation.Value ? "Motor;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Communication").FirstOrDefault().NeedMetByAccommodation.Value ? "Communication;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Social").FirstOrDefault().NeedMetByAccommodation.Value ? "Social-Emotional;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Math").FirstOrDefault().NeedMetByAccommodation.Value ? "Math;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Reading").FirstOrDefault().NeedMetByAccommodation.Value ? "Reading;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Written").FirstOrDefault().NeedMetByAccommodation.Value ? "Written Language;" : string.Empty;
                model.modulesNeedingAccommodations += accommodationFlag.Where(vm => vm.Module == "Academic").FirstOrDefault().NeedMetByAccommodation.Value ? "Academic/Functional;" : string.Empty;

                IQueryable<tblAccommodation> accommodations = db.tblAccommodations.Where(i => i.IEPid == iep.IEPid);
                if (accommodations.Any())
                {
                    model.AccomList = accommodations.OrderBy(o => o.AccomType).ToList();

                    IQueryable<int> accommodationIds = accommodations.Select(o => o.AccommodationID);
                    if (accommodationIds != null)
                    {
                        model.AccommModules = db.tblAccommodationModules.Where(i => accommodationIds.Contains(i.AccommodationID)).ToList();
                    }
                }

                IQueryable<tblLocation> locations = db.tblLocations.Where(o => o.Active == true);
                if (locations.Any())
                {
                    foreach (tblLocation loc in locations)
                    {
                        locationList.Add(new SelectListItem() { Text = loc.Name, Value = loc.LocationCode });
                    }

                    model.Locations = locationList;
                }

                model.DefaultStartDate = iep.begin_date.HasValue ? iep.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                model.DefaultEndDate = string.Empty;
            }

            ViewBag.Locations = locationList;

            if (isReadOnly)
            {
                return PartialView("ActiveIEP/_Accommodations", model);
            }
            else
            {
                return PartialView("_ModuleAccommodations", model);
            }
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

                isReadOnly = (iep.IepStatus == IEPStatus.ACTIVE) || (iep.IepStatus == IEPStatus.ARCHIVE) || (user != null && user.RoleID == nurse);

                model.IEPid = iep.IEPid;
                IQueryable<tblOtherConsideration> oc = db.tblOtherConsiderations.Where(i => i.IEPid == iep.IEPid);
                if (oc.Any())
                {
                    model = oc.FirstOrDefault();

                    // Load the modified by info
                    tblUser modifier = db.tblUsers.Where(u => u.UserID == model.ModifiedBy).SingleOrDefault();
                    ViewBag.modifiedByFullName = (modifier != null) ? string.Format("{0} {1}", modifier.FirstName, modifier.LastName) : null;
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
            {
                studentName = string.Format("{0}", student.FirstName);
            }

            ViewBag.StudentName = studentName;
            ViewBag.StudentId = studentId;
            ViewBag.FullName = string.Format("{0} {1}", student.FirstName, student.LastName);

            if (isReadOnly)
            {
                return PartialView("ActiveIEP/_OtherConsiderations", model);
            }
            else
            {
                return PartialView("_ModuleOtherConsiderations", model);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteAccommodation(int accomId)
        {
            try
            {
                tblAccommodation accomodation = db.tblAccommodations.FirstOrDefault(o => o.AccommodationID == accomId);
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

        [HttpGet]
        [Authorize]
        public ActionResult UpdatePrimaryServiceProvider(int stdIEPId, int studentId, int ProviderId)
        {
            tblIEP studentIEP = db.tblIEPs.Where(i => i.IEPid == stdIEPId && i.UserID == studentId).FirstOrDefault();
            if (studentIEP != null)
            {
                studentIEP.PrimaryProviderID = ProviderId;
                db.SaveChanges();

                return Json(new { result = "success", message = "successfully updated the primary service provider." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "error", message = "Unable to update the primary service provider." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult StudentPlanning(FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["student.UserID"]);
            int iepId = Convert.ToInt32(collection["iepId"]);

            StudentPlan thePlan = new StudentPlan(studentId)
            {
                // reset all the no concern flags
                AcademicNoConcern = false,
                CommunicationNoConcern = false,
                HealthNoConcern = false,
                IntelligenceNoConcern = false,
                MathNoConcern = false,
                MotorNoConcern = false,
                ReadingNoConcern = false,
                SocialNoConcern = false,
                WrittenNoConcern = false,
                RequireAssistiveTechnology = false
            };

            if (thePlan != null)
            {
                foreach (string key in collection.AllKeys.Skip(3))
                {
                    string value = collection[key];

                    if (value == "on")
                    {
                        thePlan[key] = true;
                    }
                    else if (DateTime.TryParse(value, out DateTime dateTimeValue))
                    {
                        thePlan[key] = dateTimeValue;
                    }
                    else if (int.TryParse(value, out int intValue))
                    {
                        thePlan[key] = intValue;
                    }
                    else
                    {
                        thePlan[key] = value == "1";
                    }
                }

                thePlan.Update(studentId, iepId);
            }

            return Json(new { result = "success", message = studentId }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult IEPFormModule(int studentId, bool? home = false)
        {
            IEPFormViewModel viewModel = new IEPFormViewModel();

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (student != null)
            {
                viewModel.IEPForms = GetForms();
                viewModel.StudentId = studentId;
                viewModel.StudentName = string.Format("{0} {1}", !string.IsNullOrEmpty(student.FirstName) ? student.FirstName : "", !string.IsNullOrEmpty(student.LastName) ? student.LastName : "");
                viewModel.Archives = db.tblFormArchives.Where(u => u.Student_UserID == studentId).OrderByDescending(o => o.ArchiveDate).ToList();
                viewModel.CanDelete = user != null && user.RoleID == owner;

                ViewBag.ReturnToHome = home;
            }

            return PartialView("_IEPFormModule", viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult MakeFormInactive(int formId)
        {
            tblFormArchive theForm = db.tblFormArchives.Where(f => f.FormArchiveID == formId).FirstOrDefault();
            if (theForm != null)
            {
                theForm.isActive = false;
                db.SaveChanges();

                return Json(new { result = "success", message = "The form was set to inactive." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { result = "error", message = "Unable to change the status to inactive." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult MakeFormActive(int formId)
        {
            tblFormArchive theForm = db.tblFormArchives.Where(f => f.FormArchiveID == formId).FirstOrDefault();
            if (theForm != null)
            {
                theForm.isActive = true;
                db.SaveChanges();

                return Json(new { result = "success", message = "The form was set to active." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { result = "error", message = "Unable to change the status to active." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult IEPFormFile(int id, string fileName, bool? home = false)
        {
            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            tblUser teacher = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            
            var iep = db.tblIEPs.Where(u => u.UserID == id && u.IepStatus == IEPStatus.ACTIVE).FirstOrDefault();

            IEP theIEP = new IEP();

            if (iep != null)
                theIEP = GetIEPPrint(id, iep.IEPid);

            IEPFormFileViewModel viewModel = new IEPFormFileViewModel
            {
                studentId = id,
                fileName = fileName,
                ActiveIEP = theIEP,
                districtList =  (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == teacher.UserID select district).Distinct().ToList(),

            };

            List<SelectListItem> forms = GetForms();

            SelectListItem form = forms.Where(o => o.Value == fileName).FirstOrDefault();
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
            };

            if (fileViewModel.studentInfo != null)
            {
                tblBuilding building = db.tblBuildings.Where(b => b.BuildingID == fileViewModel.studentInfo.BuildingID).FirstOrDefault();
                fileViewModel.building = building != null ? building.BuildingName : "";
                fileViewModel.buildingAddress = building != null ? building.Address_Mailing : "";
                fileViewModel.buildingPhone = building != null ? building.Phone : "";
                fileViewModel.buildingCityStZip = building != null ? string.Format("{0}, {1} {2}", building.City, building.State, building.Zip) : "";
                fileViewModel.buildingPhone = building != null ? building.Phone : "";


                tblBuilding neighborhoodBuilding = db.tblBuildings.Where(b => b.BuildingID == fileViewModel.studentInfo.NeighborhoodBuildingID).FirstOrDefault();
                fileViewModel.buildingNeigborhood = neighborhoodBuilding != null ? neighborhoodBuilding.BuildingName : "";

                tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                if (MIS != null)
                {
                    fileViewModel.districtContact = (from contact in db.tblContacts where contact.Active == 1 && contact.USD == fileViewModel.studentInfo.AssignedUSD select contact).FirstOrDefault();
                }

                fileViewModel.studentLanguage = GetLanguage(fileViewModel.studentInfo.StudentLanguage);
                fileViewModel.studentGradeText = GetGradeFullDescription(fileViewModel.studentInfo.Grade);
            }

            tblArchiveEvaluationDate lastReEval = db.tblArchiveEvaluationDates.Where(c => c.userID == id).OrderBy(o => o.evalutationDate).FirstOrDefault();
            if (lastReEval != null)
            {
                fileViewModel.lastReEvalDate = lastReEval.evalutationDate.ToShortDateString();
            }

            if (fileViewModel.studentInfo != null && fileViewModel.studentInfo.AssignedUSD != null)
            {
                tblDistrict district = db.tblDistricts.Where(c => c.USD == fileViewModel.studentInfo.AssignedUSD).FirstOrDefault();
                fileViewModel.districtName = district != null ? district.DistrictName : "";
            }

            switch (fileName)
            {
                case "TeamEvaluation":
                    viewModel.teamEval = db.tblFormTeamEvals.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "SummaryOfPerformance":
                    viewModel.summaryPerformance = db.tblFormSummaryPerformances.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "ConferenceSummary":
                    viewModel.conferenceSummary = db.tblFormConferenceSummaries.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "IEPAmendment":
                    viewModel.formAmend = db.tblFormIEPAmendments.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "IEPMtgConsent":
                    viewModel.formMtgConsent = db.tblFormIEPMeetingConsentToInvites.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "IEPMtgExcusal":
                    viewModel.formMtgExcusal = db.tblFormIEPMeetingExcusals.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "IEPTeamConsider":
                    viewModel.formIEPTeamConsider = db.tblFormIEPTeamConsiderations.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "ManiDetermReview":
                    tblFormManifestationDeterminiation mani = db.tblFormManifestationDeterminiations.Where(o => o.StudentId == id).FirstOrDefault();
                    if (mani != null)
                    {
                        IQueryable<tblFormManifestDeterm_TeamMembers> maniTeam = db.tblFormManifestDeterm_TeamMembers.Where(o => o.FormManifestationDeterminiationId == mani.FormManifestationDeterminiationId);
                        foreach (tblFormManifestDeterm_TeamMembers mt in maniTeam)
                        {
                            mani.tblFormManifestDeterm_TeamMembers.Add(mt);
                        }
                    }
                    viewModel.formMani = mani;
                    break;
                case "NoticeOfMeeting":
                    viewModel.formNotice = db.tblFormNoticeOfMeetings.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "ParentConsentMedicaid":
                    viewModel.formConsentMedicaid = db.tblFormParentConsents.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "PhysicianScript":
                    viewModel.formPhysician = db.tblFormPhysicianScripts.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "PriorWrittenNoticeId":
                    viewModel.formPWN = db.tblFormPriorWritten_Ident.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "RequestConsent":
                    viewModel.formPWNEval = db.tblFormPriorWritten_Eval.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "RevAllSvscPWN":
                    viewModel.formPWNRevAll = db.tblFormPriorWritten_ReokeAll.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "RevPartSvscPWN":
                    viewModel.formPWNRevPart = db.tblFormPriorWritten_ReokePart.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "RevAllSvscConsent":
                    viewModel.formRevAll = db.tblFormRevokeConsentAlls.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "RevPartSvscConsent":
                    viewModel.formRevPart = db.tblFormRevokeConsentParts.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "TransportationRequest":
                    viewModel.formTransRequest = db.tblFormTransportationRequests.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "ContinuousLearningPlan":
                    viewModel.continuousLearningPlan = db.tblFormContinuousLearningPlans.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "ChildOutcomesSummary":
                    var childOutcome = db.tblFormChildOutcomes.Where(o => o.StudentId == id).FirstOrDefault();
                    if (childOutcome != null)
                    {
                        childOutcome.tblFormChildOutcomes_PersonsInvolved = db.tblFormChildOutcomes_PersonsInvolved.Where(o => o.FormChildOutcomeID == childOutcome.FormChildOutcomeID).ToList();
                        childOutcome.tblFormChildOutcomes_SupportingEvidence = db.tblFormChildOutcomes_SupportingEvidence.Where(o => o.FormChildOutcomeID == childOutcome.FormChildOutcomeID).ToList();
                    }
                    viewModel.childOutcome = childOutcome;
                    break;
                case "TransitionReferral":
                    viewModel.formTransReferral = db.tblFormTransitionReferrals.Where(o => o.StudentId == id).FirstOrDefault();
                    break;
                case "IdeaGiftedFileReview":
                    viewModel.formFileReview = db.tblFormFileReviews.Where(o => o.StudentID == id).FirstOrDefault();
                    break;

            }

            viewModel.fileModel = fileViewModel;
            ViewBag.ReturnToHome = home;
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
            List<SelectListItem> forms = new List<SelectListItem>
            {
                new SelectListItem { Text = "Parents Rights-English", Value = "ParentsRights" },
                new SelectListItem { Text = "Notice Of Meeting", Value = "NoticeOfMeeting" },
                new SelectListItem { Text = "Prior Written Notice - Evaluation -English", Value = "RequestConsent" },
                new SelectListItem { Text = "Prior Written Notice - Identification", Value = "PriorWrittenNoticeId" },
                new SelectListItem { Text = "Revocation of Consent-Particular Services", Value = "RevPartSvscConsent" },
                new SelectListItem { Text = "Prior Written Notice-Revocation of Particular Services", Value = "RevPartSvscPWN" },
                new SelectListItem { Text = "Revocation of Consent-All Services", Value = "RevAllSvscConsent" },
                new SelectListItem { Text = "Prior Written Notice-Revocation of All Services", Value = "RevAllSvscPWN" },
                new SelectListItem { Text = "Sample Public Notice (Child Find)", Value = "SamplePublicNotice" },
                new SelectListItem { Text = "IEP Meeting-Consent to Invite Representative of Non-Educational Agency", Value = "IEPMtgConsent" },
                new SelectListItem { Text = "IEP Meeting-Excusal from Attendance Form", Value = "IEPMtgExcusal" },
                new SelectListItem { Text = "IEP Amendment Form", Value = "IEPAmendment" },
                new SelectListItem { Text = "Re-Evaluation Not Needed Agreement Form", Value = "IEPReEvalNotNeeded" },
                new SelectListItem { Text = "Manifestation Determination Review Form", Value = "ManiDetermReview" },
                new SelectListItem { Text = "IEP Team Considerations", Value = "IEPTeamConsider" },
                new SelectListItem { Text = "Parent Consent for Release of Information and Medicaid Reimbursement", Value = "ParentConsentMedicaid" },
                new SelectListItem { Text = "Physician Script", Value = "PhysicianScript" },
                new SelectListItem { Text = "Team Evaluation Report", Value = "TeamEvaluation" },
                new SelectListItem { Text = "Conference Summary", Value = "ConferenceSummary" },
                new SelectListItem { Text = "Summary Of Performance", Value = "SummaryOfPerformance" },
                new SelectListItem { Text = "Request for Transportation", Value = "TransportationRequest" },
                new SelectListItem { Text = "Individual Continuous Learning Plan", Value = "ContinuousLearningPlan" },
                new SelectListItem { Text = "Child Outcomes Summary", Value = "ChildOutcomesSummary" },
                new SelectListItem { Text = "Transition Referral", Value = "TransitionReferral" },
                new SelectListItem { Text = "10 Day Waiver", Value = "TenDayWaiver" },
                new SelectListItem { Text = "IDEA & Gifted File Review", Value = "IdeaGiftedFileReview" },
                new SelectListItem { Text = "Dynamic Learning Maps", Value = "DLM" },
            };

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
            IEP theIEP = GetIEPPrint(stid, iepId);
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
                theIEP.studentDetails.printContingencyPlan = true;


                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintIEPSection(int stid, int iepId, string section, string goalsToPrint)
        {
            IEP theIEP = GetIEPPrint(stid, iepId);
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
                            if (!string.IsNullOrEmpty(goalsToPrint))
                            {
                                theIEP.studentDetails.printProgressGoals = goalsToPrint.Split(',').Select(int.Parse).ToList();
                            }

                            theIEP.studentDetails.printProgressReport = true;
                            break;
                        }
                    case "Contingency":
                        {
                            theIEP.studentDetails.printContingencyPlan = true;
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
            IEP theIEP = GetIEPPrint(stid, iepId);
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
                theIEP.studentDetails.printContingencyPlan = false;

                return View("PrintIEP", theIEP);
            }

            // Unknow error happened.
            return RedirectToAction("Index", "Home", null);
        }

        private IEP GetIEPPrint(int stid, int iepId)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser student = db.tblUsers.SingleOrDefault(u => u.UserID == stid);

            StudentDetailsPrintViewModel studentDetails = new StudentDetailsPrintViewModel();

            List<tblStudentRelationship> contacts = db.tblStudentRelationships.Where(i => i.UserID == stid && i.PrimaryContact == 1).ToList();

            tblUser mis = FindSupervisor.GetUSersMIS(teacher);

            if (mis == null)
            {
                mis = teacher;
            }

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
                theIEP.current.Primary_DisabilityCode = GetDisability(theIEP.current.Primary_DisabilityCode);
                theIEP.current.Secondary_DisabilityCode = GetDisability(theIEP.current.Secondary_DisabilityCode);

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

                foreach (tblGoal goal in theIEP.studentGoals)
                {
                    theIEP.studentGoalBenchmarks.AddRange(db.tblGoalBenchmarks.Where(g => g.goalID == goal.goalID).ToList());

                    theIEP.studentGoalEvalProcs.AddRange(db.tblGoalEvaluationProcedures.Where(g => g.goalID == goal.goalID).ToList());

                    if (theIEP.studentGoalBenchmarks.Any())
                    {
                        List<int> benchmarkIds = theIEP.studentGoalBenchmarks.Select(o => o.goalBenchmarkID).ToList();
                        theIEP.studentGoalBenchmarkMethods.AddRange(db.tblGoalBenchmarkMethods.Where(g => benchmarkIds.Contains(g.goalBenchmarkID)).ToList());
                    }
                }

                tblBehavior studentBehavior = db.tblBehaviors.Where(g => g.IEPid == theIEP.current.IEPid).FirstOrDefault();
                theIEP.studentBehavior = GetBehaviorModel(student.UserID, theIEP.current.IEPid);

                StudentTransitionViewModel stvw = new StudentTransitionViewModel
                {
                    studentId = student.UserID,
                    student = student,
                    assessments = db.tblTransitionAssessments.Where(a => a.IEPid == theIEP.current.IEPid).ToList(),
                    services = db.tblTransitionServices.Where(s => s.IEPid == theIEP.current.IEPid).ToList(),
                    goals = db.tblTransitionGoals.Where(g => g.IEPid == theIEP.current.IEPid).ToList(),
                    transition = db.tblTransitions.Where(t => t.IEPid == theIEP.current.IEPid).FirstOrDefault() ?? new tblTransition()
                };
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
                        DateTime endDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.AddYears(1) : theIEP.current.begin_date.Value.AddYears(1);
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
                    tblBuilding studentBuilding = db.tblBuildings.Where(c => c.BuildingID == info.BuildingID).Take(1).FirstOrDefault();
                    tblBuilding studentNeighborhoodBuilding = db.tblBuildings.Where(c => c.BuildingID == info.NeighborhoodBuildingID).Take(1).FirstOrDefault();
                    tblCounty studentCounty = db.tblCounties.Where(c => c.CountyCode == info.County).FirstOrDefault();
                    tblDistrict studentUSD = db.tblDistricts.Where(c => c.USD == info.AssignedUSD).FirstOrDefault();
                    var serviceBuildingIds = theIEP.studentServices != null ? theIEP.studentServices.Select(o => o.BuildingID).ToList() : new List<string>();
                    var serviceBuildings = db.vw_BuildingList.Where(c => serviceBuildingIds.Contains(c.BuildingID)).ToList();

                    int studentAgeAtIEP = 0;
                    if (theIEP.iepStartTime.HasValue)
                    {
                        DateTime iepDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value : theIEP.current.begin_date.Value;
                        studentAgeAtIEP = (iepDate.Year - info.DateOfBirth.Year - 1) + (((iepDate.Month > info.DateOfBirth.Month) || ((iepDate.Month == info.DateOfBirth.Month) && (iepDate.Day >= info.DateOfBirth.Day))) ? 1 : 0);
                    }

                    studentDetails.student = info;
                    studentDetails.teacher = teacher;
                    studentDetails.ethnicity = info.Ethicity == "Y" ? "Hispanic" : "Not Hispanic or Latino";
                    studentDetails.gender = info.Gender == "F" ? "Female" : "Male";
                    studentDetails.contacts = contacts;
                    studentDetails.building = studentBuilding;
                    studentDetails.neighborhoodBuilding = studentNeighborhoodBuilding;
                    studentDetails.serviceAttendanceBuildings = serviceBuildings;
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
                    studentDetails.grade = GetGradeFullDescription(theIEP.current.Grade == null ? info.Grade : theIEP.current.Grade);
                    studentDetails.gradeCurrent = GetGradeFullDescription(info.Grade);
                    studentDetails.annualInititationDate = theIEP.iepStartTime.HasValue ? theIEP.iepStartTime.Value.ToShortDateString() : "";
                    studentDetails.inititationDate = theIEP.current.MeetingDate.HasValue ? theIEP.current.MeetingDate.Value.ToShortDateString() : "";
                    studentDetails.contingencyPlan = db.tblContingencyPlans.Where(p => p.IEPid == iepId).FirstOrDefault();

                    var schoolYear = db.tblCalendars.FirstOrDefault(o => o.calendarDate == DateTime.Today);
                    if (schoolYear != null)
                    {
                        studentDetails.schoolYear = string.Format("{0} - {1}", schoolYear.SchoolYear - 1, schoolYear.SchoolYear);
                    }

                    //teacher
                    List<tblOrganizationMapping> existingAssignments = db.tblOrganizationMappings.Where(u => u.UserID == stid).ToList();

                    if (existingAssignments.Any())
                    {
                        var assignedTeacher = existingAssignments.Take(1).FirstOrDefault();
                        if (assignedTeacher != null)
                        {
                            var teachObj = db.tblUsers.SingleOrDefault(u => u.UserID == assignedTeacher.AdminID);
                            studentDetails.teacherName = teachObj == null ? "" : string.Format("{0}, {1}", teachObj.LastName, teachObj.FirstName);
                        }
                    }

                    if (theIEP.current.Amendment)
                    {
                        var amendingIEP = db.tblIEPs.Where(o => o.IEPid == theIEP.current.OriginalIEPid).FirstOrDefault();
                        if (amendingIEP != null)
                            studentDetails.inititationDateNext = amendingIEP.MeetingDate.HasValue ? amendingIEP.MeetingDate.Value.AddYears(1).ToShortDateString() : "";
                    }
                    else
                    {
                        studentDetails.inititationDateNext = theIEP.current.MeetingDate.HasValue ? theIEP.current.MeetingDate.Value.AddYears(1).ToShortDateString() : "";
                    }

                    IOrderedQueryable<tblIEP> historicalIEPs = db.tblIEPs.Where(o => o.UserID == info.UserID && (o.IepStatus == IEPStatus.ARCHIVE || o.IepStatus == IEPStatus.ACTIVE)).OrderByDescending(o => o.begin_date);
                    List<IEPHistoryViewModel> historicalIEPList = new List<IEPHistoryViewModel>();

                    if (theIEP.current.IepStatus.ToUpper() == IEPStatus.DRAFT)
                    {
                        //add draft to history
                        string iepType = string.Format("{0}", theIEP.anyStudentIEPActive && !theIEP.current.Amendment ? "Annual" : theIEP.anyStudentIEPActive && theIEP.current.Amendment ? "Amendment" : string.Empty);
                        IEPHistoryViewModel historyItem = new IEPHistoryViewModel() { edStatus = theIEP.current.StatusCode, iepDate = theIEP.current.MeetingDate.HasValue ? theIEP.current.MeetingDate.Value.ToShortDateString() : "", iepType = iepType };
                        historicalIEPList.Add(historyItem);
                    }

                    foreach (tblIEP history in historicalIEPs)
                    {
                        IEPHistoryViewModel historyItem = new IEPHistoryViewModel();
                        historyItem.iepType = history.OriginalIEPid == null ? "Annual" : "Amendment";
                        historyItem.edStatus = string.IsNullOrEmpty(history.StatusCode) ? "" : history.StatusCode;
                        historyItem.iepDate = history.MeetingDate.HasValue ? history.MeetingDate.Value.ToShortDateString() : "";
                        historicalIEPList.Add(historyItem);
                    }

                    if (studentDetails.student.ExitDate.HasValue)
                    {
                        IEPHistoryViewModel exitItem = new IEPHistoryViewModel
                        {
                            iepType = "Exit",
                            iepDate = studentDetails.student.ExitDate.Value.ToShortDateString(),
                            edStatus = studentDetails.student.StatusCode //"D"

                        };
                        historicalIEPList.Add(exitItem);
                    }

                    studentDetails.history = historicalIEPList;
                }


                if (theIEP.accommodations != null)
                {
                    foreach (tblAccommodation accom in theIEP.accommodations)
                    {
                        string shortDesc = "Accommodation ";

                        if (@accom.AccomType == 1)
                        {
                            shortDesc = "Accommodation ";
                        }
                        else if (@accom.AccomType == 2)
                        {
                            shortDesc = "Modification ";
                        }
                        else if (@accom.AccomType == 3)
                        {
                            shortDesc = "Supplemental Aids and Services ";
                        }
                        else if (@accom.AccomType == 4)
                        {
                            shortDesc = "Support for School Personnel ";
                        }
                        else if (@accom.AccomType == 5)
                        {
                            shortDesc = "Transportation ";
                        }

                        IQueryable<int> moduleList = db.tblAccommodationModules.Where(o => o.AccommodationID == accom.AccommodationID).Select(o => o.ModuleID);

                        string modules = string.Join("<br />", db.tblModules.Where(o => moduleList.Contains(o.ModuleID)).Select(o => o.ModuleName));

                        AccomodationPrintViewModel accommodationView = new AccomodationPrintViewModel
                        {
                            StudentId = stid,
                            Location = accom.Location,
                            Description = accom.Description,
                            Duration = accom.Duration,
                            Frequency = accom.Frequency,
                            AnticipatedEndDate = accom.AnticipatedEndDate,
                            AnticipatedStartDate = accom.AnticipatedStartDate,
                            AccomType = shortDesc,
                            Module = modules
                        };

                        studentDetails.accommodationList.Add(accommodationView);
                    }
                }

                theIEP.studentDetails = studentDetails;

                return theIEP;
            }

            return null;
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
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
                mailMessage.Body = string.Format("{0} has contacted you from email {1} with this message {2}", collection["Name"], collection["email"], collection["Message"]);
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
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            ViewBag.fileVersion = version;

            tblUser user = db.tblUsers.SingleOrDefault(u => u.Email == User.Identity.Name);
            user.LastVersionNumberSeen = version;
            db.SaveChanges();

            List<tblVersionLog> model = new List<tblVersionLog>();
            model = db.tblVersionLogs.Where(u => u.VersionNumber == version).ToList();

            return View(model);
        }

        #region StudentForms

        [Authorize]
        public ActionResult DownloadArchive(int id)
        {
            //TODO: Check if user has permissions to update permissions
            tblFormArchive document = db.tblFormArchives.Where(o => o.FormArchiveID == id).FirstOrDefault();
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
            {
                return null;
            }
        }

        [Authorize]
        public ActionResult DeleteArchive(int id)
        {
            //TODO: Check if user has permissions to update permissions
            tblFormArchive document = db.tblFormArchives.Where(o => o.FormArchiveID == id).FirstOrDefault();

            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (document != null && (user.RoleID == owner))
            {
                document.isActive = false;
                db.SaveChanges();
                return Json(new { Result = true, Message = "The document was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Result = false, Message = "Unable to delete the document. Please contact the system administrator for more assistance." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult DeleteUploadForm(int studentId, int formId)
        {
            tblFormArchive form = db.tblFormArchives.Where(f => f.Student_UserID == studentId && f.FormArchiveID == formId).FirstOrDefault();
            if (form != null)
            {
                //delete the form in the database
                db.tblFormArchives.Remove(form);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to delete the file from the database: " + e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "success", Message = "The uploaded file was removed from the database." }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to delete the uploaded form." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadStudentFile(HttpPostedFileBase myFile, int studentId)
        {
            try
            {
                using (BinaryReader binaryReader = new BinaryReader(myFile.InputStream))
                {
                    string fileName = Path.GetFileName(myFile.FileName);
                    string fileNameExt = Path.GetExtension(fileName);

                    if (fileNameExt.ToLower() != ".pdf")
                    {
                        return Json(new { result = false, message = "Please select a valid PDF" }, "text/plain");
                    }

                    byte[] fileData = binaryReader.ReadBytes(myFile.ContentLength);

                    tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                    //int iepId = db.tblIEPs.Where(i => i.UserID == studentId).OrderBy(i => i.IepStatus).FirstOrDefault().IEPid;

                    tblFormArchive archive = new tblFormArchive
                    {
                        Creator_UserID = teacher.UserID,
                        Student_UserID = studentId,
                        FormName = string.IsNullOrEmpty(fileName) ? "Upload" : fileName,
                        FormFile = fileData,
                        ArchiveDate = DateTime.Now,
                        isUpload = true,
                        isActive = true
                    };

                    db.tblFormArchives.Add(archive);
                    db.SaveChanges();
                }

                List<tblFormArchive> archives = db.tblFormArchives.Where(u => u.Student_UserID == studentId && u.isUpload).OrderByDescending(o => o.ArchiveDate).ToList();

                List<IEPFormFileViewModel> archiveList = new List<IEPFormFileViewModel>();
                foreach (tblFormArchive archive in archives)
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
        #endregion

        [HttpPost]
        public ActionResult SearchUserName(string username)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (user != null && (!string.IsNullOrEmpty(username)))
            {
                string usernameVal = username.Trim();

                if (user.RoleID == owner)
                {
                    List<tblDistrict> districts = (from district in db.tblDistricts select district).Distinct().ToList();
                    List<tblBuilding> buildings = (from building in db.tblBuildings select building).Distinct().ToList();

                    List<StudentIEPViewModel> filterUsers = db.vw_UserList.Where(ul => ul.RoleID != owner)
                        .Where(o => o.LastName.Contains(usernameVal) || o.FirstName.Contains(usernameVal) || o.MiddleName.Contains(usernameVal)).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false })
                        .Distinct()
                        .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

                    return Json(new { result = true, filterUsers = filterUsers }, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    List<tblDistrict> districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == user.UserID select district).Distinct().ToList();
                    List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == user.UserID select building).Distinct().ToList();

                    List<string> myDistricts = districts.Select(d => d.USD).ToList();
                    List<string> myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");

                    List<StudentIEPViewModel> filterUsers = db.vw_UserList.Where(ul => (ul.RoleID == admin || ul.RoleID == teacher || ul.RoleID == student || ul.RoleID == nurse) && (myBuildings.Contains(ul.BuildingID) && myDistricts.Contains(ul.USD))).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false })
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
            bool canReset = (user != null && (user.RoleID == owner || user.RoleID == mis));
            ViewBag.canReset = canReset;
            return View("~/Reports/SpedPro/Index.cshtml");
        }

        [HttpPost]
        public ActionResult SpedProStudentList(int fiscalYear, string buildingId, string districtId, bool isReset)
        {
            string iepStatus = IEPStatus.ACTIVE;
            List<tblUser> studentsList = new List<tblUser>();
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (MIS != null)
            {
                bool canReset = (MIS != null && (MIS.RoleID == owner || MIS.RoleID == mis));

                List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList();
                if (!string.IsNullOrEmpty(districtId) && districtId != "-1")
                {
                    buildings = buildings.Where(o => o.USD == districtId).ToList();
                }

                List<string> myBuildings = buildings.Select(b => b.BuildingID).ToList();

                if (!string.IsNullOrEmpty(buildingId) && buildingId != "-1")
                {
                    myBuildings.Clear();
                    myBuildings.Add(buildingId);
                }

                if (isReset)
                {
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
                else
                {
                    //not filed yet
                    var query = (from iep in db.tblIEPs
                                 join student in db.tblUsers
                                     on iep.UserID equals student.UserID
                                 join services in db.tblServices
                                     on iep.IEPid equals services.IEPid
                                 join building in db.tblBuildingMappings
                                     on student.UserID equals building.UserID
                                 join studentInfo in db.tblStudentInfoes
                                    on student.UserID equals studentInfo.UserID
                                 where
                                 iep.IepStatus == iepStatus
                                 && (studentInfo.ExitDate == null || services.StartDate < studentInfo.ExitDate)
                                 && (student.Archive == null || student.Archive == false)
                                 && services.SchoolYear == fiscalYear
                                 && (services.FiledOn == null || iep.FiledOn == null)
                                 && services.ServiceCode != "NS"
                                 && myBuildings.Contains(building.BuildingID)
                                 select new { iep, student }).Distinct().ToList();

                    if (query.Count() > 0)
                    {
                        studentsList.AddRange(query.Select(o => o.student));
                    }
                }
            }

            var students = studentsList.Select(x => new
            {
                UserID = x.UserID,
                LastName = x.LastName,
                FirstName = x.FirstName,
            }).ToList();

            return Json(new { result = true, students = students }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult SpedProDownload(FormCollection collection)
        {
            bool isReset = !string.IsNullOrEmpty(collection["cbReset"]);
            string fiscalYearStr = collection["fiscalYear"];
            int.TryParse(fiscalYearStr, out int fiscalYear);

            string districtId = collection["districtDD"];
            string buildingId = collection["buildingDD"];
            string studentList = collection["studentDD"];

            var studentIds = string.IsNullOrEmpty(studentList) ? Enumerable.Empty<int>() : Array.ConvertAll(studentList.Split(','), int.Parse).ToList();

            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (MIS != null)
            {
                bool canReset = (MIS != null && (MIS.RoleID == owner || MIS.RoleID == mis));
                ViewBag.canReset = canReset;

                List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct().ToList();
                if (!string.IsNullOrEmpty(districtId) && districtId != "-1")
                {
                    buildings = buildings.Where(o => o.USD == districtId).ToList();
                }

                List<string> myBuildings = buildings.Select(b => b.BuildingID).ToList();
                if (!string.IsNullOrEmpty(buildingId) && buildingId != "-1")
                {
                    myBuildings.Clear();
                    myBuildings.Add(buildingId);
                }

                string iepStatus = IEPStatus.ACTIVE;
                List<ExportErrorView> exportErrors = new List<ExportErrorView>();


                if (isReset)
                {
                    try
                    {

                        IQueryable<tblIEP> resetQuery = (from iep in db.tblIEPs
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
                                                          && (!studentIds.Any() || studentIds.Contains(student.UserID))
                                                         select iep).Distinct();

                        foreach (tblIEP item in resetQuery)
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

                    ViewBag.BuildingId = buildingId;
                    ViewBag.DistictId = districtId;
                    ViewBag.FiscalYear = fiscalYear;

                    return View("~/Reports/SpedPro/Index.cshtml");

                }

                var query = (from iep in db.tblIEPs
                             join student in db.tblUsers
                                 on iep.UserID equals student.UserID
                             join services in db.tblServices
                                 on iep.IEPid equals services.IEPid
                             join building in db.tblBuildingMappings
                                 on student.UserID equals building.UserID
                             join studentInfo in db.tblStudentInfoes
                                on student.UserID equals studentInfo.UserID
                             where
                             iep.IepStatus == iepStatus
                             && (studentInfo.ExitDate == null || services.StartDate < studentInfo.ExitDate)
                             && (student.Archive == null || student.Archive == false)
                             && services.SchoolYear == fiscalYear
                             && (services.FiledOn == null || iep.FiledOn == null)
                             && services.ServiceCode != "NS"
                             && myBuildings.Contains(building.BuildingID)
                             && (!studentIds.Any() || studentIds.Contains(student.UserID))
                             select new { iep, student }).Distinct().ToList();

                if (query.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    bool checkPrevious = true;

                    foreach (var item in query)
                    {
                        //this is either a new iep or a new annual iep, we will need to resend any services for the fy
                        //checkPrevious = item.iep.OriginalIEPid == null;

                        IEP theIEP = new IEP()
                        {
                            current = item.iep,
                            studentFirstName = string.Format("{0}", item.student.FirstName),
                            studentLastName = string.Format("{0}", item.student.LastName),
                        };

                        if (theIEP != null && theIEP.current != null)
                        {
                            StudentDetailsPrintViewModel studentDetails = new StudentDetailsPrintViewModel();
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
                                tblBuilding studentBuilding = db.tblBuildings.Where(c => c.BuildingID == info.BuildingID).Take(1).FirstOrDefault();
                                tblBuilding studentNeighborhoodBuilding = db.tblBuildings.Where(c => c.BuildingID == info.NeighborhoodBuildingID).Take(1).FirstOrDefault();
                                tblCounty studentCounty = db.tblCounties.Where(c => c.CountyCode == info.County).FirstOrDefault();
                                tblDistrict studentUSD = db.tblDistricts.Where(c => c.USD == info.AssignedUSD).FirstOrDefault();

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

                            List<tblIEP> otherIEPs = (from iep in db.tblIEPs
                                                      join student in db.tblUsers
                                                          on iep.UserID equals student.UserID
                                                      join services in db.tblServices
                                                          on iep.IEPid equals services.IEPid
                                                      join building in db.tblBuildingMappings
                                                          on student.UserID equals building.UserID
                                                      join studentInfo in db.tblStudentInfoes
                                                            on student.UserID equals studentInfo.UserID
                                                      where
                                                      iep.IepStatus == IEPStatus.ARCHIVE
                                                      && (studentInfo.ExitDate == null || services.StartDate < studentInfo.ExitDate)
                                                      && (student.Archive == null || student.Archive == false)
                                                      && services.SchoolYear == fiscalYear
                                                      && services.ServiceCode != "NS"
                                                      && iep.UserID == item.iep.UserID
                                                      && myBuildings.Contains(building.BuildingID)
                                                      select iep).Distinct().ToList();

                            //if an iep has been amended, exclude those ieps
                            List<int> excludeIEPS = otherIEPs.Where(o => o.AmendingIEPid.HasValue).Select(o => o.AmendingIEPid.Value).ToList();

                            //check if the active iep amended an iep
                            if (theIEP.current.AmendingIEPid.HasValue)
                            {
                                excludeIEPS.Add(theIEP.current.AmendingIEPid.Value);
                            }

                            List<tblService> otherServices = (from iep in db.tblIEPs
                                                              join student in db.tblUsers
                                                                  on iep.UserID equals student.UserID
                                                              join services in db.tblServices
                                                                  on iep.IEPid equals services.IEPid
                                                              join building in db.tblBuildingMappings
                                                                  on student.UserID equals building.UserID
                                                              join studentInfo in db.tblStudentInfoes
                                                                    on student.UserID equals studentInfo.UserID
                                                              where
                                                              iep.IepStatus == IEPStatus.ARCHIVE
                                                               && (studentInfo.ExitDate == null || services.StartDate < studentInfo.ExitDate)
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

                        List<ExportErrorView> errors = SpedProExport(theIEP, fiscalYear, sb);

                        if (errors.Count > 0)
                        {
                            exportErrors.AddRange(errors);

                        }

                    }//end foreach


                    if (exportErrors.Count == 0)
                    {
                        //on save if no errors
                        db.SaveChanges();
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                        OutputResponse(byteArray, "SpedProExport.txt", "text/plain");
                    }
                    else
                    {
                        ViewBag.errors = exportErrors;

                        ViewBag.BuildingId = buildingId;
                        ViewBag.DistictId = districtId;
                        ViewBag.FiscalYear = fiscalYear;
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
                    ViewBag.BuildingId = buildingId;
                    ViewBag.DistictId = districtId;
                    ViewBag.FiscalYear = fiscalYear;
                }
            }

            return View("~/Reports/SpedPro/Index.cshtml");
        }

        private List<ExportErrorView> SpedProExport(IEP studentIEP, int schoolYear, StringBuilder sb)
        {
            List<ExportErrorView> errors = new List<ExportErrorView>();

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
            tblGrade grade = db.tblGrades.Where(o => o.gradeID == studentIEP.studentDetails.student.Grade).FirstOrDefault();

            string gradeCode = grade != null && grade.SpedCode != null ? grade.SpedCode : "";

            if (gradeCode == "")
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = studentIEP.studentDetails.student.UserID.ToString(),
                    KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: Grade")
                });
            }
            else
            {
                sb.AppendFormat("\t{0}", gradeCode);
            }

            if(!string.IsNullOrEmpty(studentIEP.studentDetails.student.StatusCode) &&
                (studentIEP.studentDetails.student.StatusCode == "2"))
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = studentIEP.studentDetails.student.UserID.ToString(),
                    KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format(" {0}, {1} ERROR: {2} {3}", studentIEP.studentLastName, studentIEP.studentFirstName, "Invalid Status Code: ", studentIEP.studentDetails.student.StatusCode)
                });
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
            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.ClaimingCode.HasValue && studentIEP.studentDetails.student.ClaimingCode.Value ? "1" : "");

            //19 Placed By KDCF/JJA/LEA/Parent req
            if (string.IsNullOrEmpty(studentIEP.studentDetails.student.PlacementCode))
            {
                errors.Add(new ExportErrorView()
                {
                    UserID = studentIEP.studentDetails.student.UserID.ToString(),
                    KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: Placed By KDCF/JJA/LEA/Parent")
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
                    UserID = studentIEP.studentDetails.student.UserID.ToString(),
                    KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: County of Residence")
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
                    UserID = studentIEP.studentDetails.student.UserID.ToString(),
                    KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                    Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: Language of Parent")
                });
            }
            else
            {
                sb.AppendFormat("\t{0}", studentIEP.studentDetails.parentLang);
            }


            int count = 1;
            foreach (tblService service in studentIEP.studentServices.Distinct())
            {
                string serviceEndDateOverride = "";
                int primaryProviderId = 0;
                string primaryDisability = studentIEP.current.Primary_DisabilityCode;
                string secondaryDisability = studentIEP.current.Secondary_DisabilityCode;

                if (count == 25)
                {
                    break;
                }

                service.FiledOn = DateTime.Now;

                DateTime? serviceIEPDate = null;

                //1 IEP date req
                if (service.IEPid != studentIEP.current.IEPid)
                {
                    //need to look up date from the iep this service is from
                    tblIEP serviceIEP = db.tblIEPs.Where(o => o.IEPid == service.IEPid).FirstOrDefault();

                    primaryProviderId = serviceIEP.PrimaryProviderID.HasValue ? serviceIEP.PrimaryProviderID.Value : 0;

                    //look up disability information from the original iep
                    primaryDisability = serviceIEP.Primary_DisabilityCode;
                    secondaryDisability = serviceIEP.Secondary_DisabilityCode;

                    if (serviceIEP.OriginalIEPid != null)
                    {
                        //look up date of orginal iep -- these are amendments?
                        tblIEP originalIEP = db.tblIEPs.Where(o => o.IEPid == serviceIEP.OriginalIEPid).FirstOrDefault();
                        if (originalIEP != null && originalIEP.MeetingDate.HasValue)
                        {
                            serviceIEPDate = originalIEP.MeetingDate.Value;
                        }
                    }
                    else
                    {
                        if (serviceIEP.MeetingDate.HasValue)
                        {
                            serviceIEPDate = serviceIEP.MeetingDate.Value;
                        }
                    }

                    if (!serviceIEPDate.HasValue)
                    {
                        errors.Add(new ExportErrorView()
                        {
                            UserID = studentIEP.studentDetails.student.UserID.ToString(),
                            KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                            Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: IEP date")
                        });
                    }
                    else
                    {
                        sb.AppendFormat("\t{0}", serviceIEPDate.Value.ToShortDateString());
                    }

                    //END DATE CHECK
                    //need to look if the iep was ended earlier than expected and replaced with a new Annual - if so we need to correct the end date
                    //to be the last valid day before the begin date of the new iep


                    if (studentIEP.current.Amendment)
                    {
                        //on amendments we need to look up its original iep meeting date and use that to compare
                        if (studentIEP.current.OriginalIEPid != null)
                        {
                            tblIEP currentIEPOriginalIEP = db.tblIEPs.Where(o => o.IEPid == studentIEP.current.OriginalIEPid).FirstOrDefault();
                            if (currentIEPOriginalIEP.MeetingDate <= service.EndDate)
                            {
                                serviceEndDateOverride = SpedProEndDateCheck(currentIEPOriginalIEP.MeetingDate, service);
                            }

                        }
                    }
                    else if (studentIEP.current.MeetingDate <= service.EndDate)
                    {
                        //need to end 1 day prior to the new annual beginning date or next available valid date
                        serviceEndDateOverride = SpedProEndDateCheck(studentIEP.current.MeetingDate, service);
                    }

                }
                else
                {
                    primaryProviderId = studentIEP.current.PrimaryProviderID.HasValue ? studentIEP.current.PrimaryProviderID.Value : 0;

                    if (studentIEP.current.OriginalIEPid != null)
                    {
                        //look up date of orginal iep
                        tblIEP originalIEP2 = db.tblIEPs.Where(o => o.IEPid == studentIEP.current.OriginalIEPid).FirstOrDefault();
                        if (originalIEP2 != null && originalIEP2.MeetingDate.HasValue)
                        {
                            serviceIEPDate = originalIEP2.MeetingDate.Value;
                        }
                    }
                    else
                    {
                        serviceIEPDate = studentIEP.current.MeetingDate.Value;
                    }

                    if (!serviceIEPDate.HasValue)
                    {
                        errors.Add(new ExportErrorView()
                        {
                            UserID = studentIEP.studentDetails.student.UserID.ToString(),
                            KidsID = string.Format("KIDSID: {0}", studentIEP.studentDetails.student.KIDSID.ToString()),
                            Description = string.Format(" {0}, {1} ERROR: {2}", studentIEP.studentLastName, studentIEP.studentFirstName, "Missing required field: IEP date")
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
                sb.AppendFormat("\t{0}", primaryDisability);

                //5 secondary disablity
                sb.AppendFormat("\t{0}", secondaryDisability);

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
                sb.AppendFormat("\t{0}", service.ProviderID.HasValue && primaryProviderId == service.ProviderID.Value ? "1" : "");

                //13 Service Start Date
                sb.AppendFormat("\t{0}", service.StartDate.ToShortDateString());

                //14 Service end Date
                if (!studentIEP.studentDetails.student.ExitDate.HasValue && string.IsNullOrEmpty(serviceEndDateOverride))
                    sb.AppendFormat("\t{0}", service.EndDate.Value.ToShortDateString());
                else
                {
                    bool isOverwritten = false;
                    //check if exit date applies
                    if (studentIEP.studentDetails.student.ExitDate.HasValue)
                    {
                        var studentExitDate = db.tblCalendars.Where(o => o.calendarDate == studentIEP.studentDetails.student.ExitDate.Value && o.BuildingID == service.BuildingID).FirstOrDefault();

                        if (studentExitDate != null
                            && studentExitDate.SchoolYear == service.SchoolYear
                            && service.IEPid == studentIEP.current.IEPid)
                        {
                            //only use exit date if it is in the same school year
                            sb.AppendFormat("\t{0}", studentIEP.studentDetails.student.ExitDate.Value.ToShortDateString());
                            isOverwritten = true;
                        }
                    }

                    if (!isOverwritten)
                    {
                        if (!string.IsNullOrEmpty(serviceEndDateOverride))
                        {
                            //there is an override due to a previous IEP that was ended early
                            sb.AppendFormat("\t{0}", serviceEndDateOverride);
                        }
                        else
                        {
                            sb.AppendFormat("\t{0}", service.EndDate.Value.ToShortDateString());
                        }
                    }
                }

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
            string fileName = collection["fileName"];
            
            byte[] mergedFile = CreateIEPPdf(StudentHTMLContent, HTMLContent, HTMLContent2, HTMLContent3, studentName, studentId, isArchive, iepIDStr, isIEP, formName);
            if (mergedFile != null)
            {
                string downloadFileName = string.IsNullOrEmpty(formName) ? string.IsNullOrEmpty(HTMLContent) ? "StudentInformation.pdf" : "IEP.pdf" : string.Format("{0}.pdf", formName);
                OutputResponse(mergedFile, downloadFileName, "application/pdf");
            }

            TempData["Error"] = "There was a problem printing the page.";
            return RedirectToAction("Index", "Error");

        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult DownloadPDFMulti(FormCollection collection)
        {


            string HTMLContent2 = collection["multiContactPrintText"];

            string studentName = collection["studentName"];
            string studentId = collection["studentId"];

            string iepIDStr = collection["iepID"];

            string formName = collection["formName"];
            string fileName = collection["fileName"];


            byte[] mergedFile = CreateIEPPdf("", "", HTMLContent2, "", studentName, studentId, "0", iepIDStr, "0", formName);
            if (mergedFile != null)
            {
                string downloadFileName = string.IsNullOrEmpty(formName) ? "IEP_Form.pdf" : string.Format("{0}.pdf", formName);
                OutputResponse(mergedFile, downloadFileName, "application/pdf");

            }


            return null;

        }

        private byte[] CreateIEPPdf(string StudentHTMLContent, string HTMLContent, string HTMLContent2, string HTMLContent3, string studentName, string studentId,
        string isArchive, string iepIDStr, string isIEP, string formName)
        {
            if (!string.IsNullOrEmpty(HTMLContent) || !string.IsNullOrEmpty(StudentHTMLContent) || !string.IsNullOrEmpty(HTMLContent2) || !string.IsNullOrEmpty(HTMLContent3))
            {
                string logoImage = Server.MapPath("../Content/IEPBackpacklogo_black2.png");
                iTextSharp.text.Image imgfoot = null;

                if (System.IO.File.Exists(logoImage))
                {
                    imgfoot = iTextSharp.text.Image.GetInstance(logoImage);
                }


                int.TryParse(studentId, out int id);

                int.TryParse(iepIDStr, out int iepId);

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
                {
                    studentName = string.Empty;
                }

                bool isDraft = false;

                tblIEP iepObj = db.tblIEPs.Where(o => o.IEPid == iepId).FirstOrDefault();
                if (iepObj != null)
                {
                    isDraft = iepObj.IepStatus != null && iepObj.IepStatus.ToUpper() == "DRAFT" ? true : false;
                }


                tblUser teacher = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

                string cssText = @"<style>hr{color:whitesmoke;padding:0;margin:0;padding-top:2px;padding-bottom:2px;}h5{font-weight:500}.module-page{font-size:9pt;}.header{color:white;}img{margin-top:-10px;}.input-group-addon, .transitionGoalLabel, .transitionServiceLabel {font-weight:600;}.transitionServiceLabel, .underline{ text-decoration: underline;}.transition-break{page-break-before:always;}td { padding: 10px;}th {font-weight:600;} table {width:700px;border-spacing: 0px;border:none;font-size:9pt}.module-page, span {font-size:9pt;}label{font-weight:600;font-size:9pt}.text-center{text-align:center} h3 {font-weight:400;font-size:11pt;width:100%;text-align:center;padding:8px;}p {padding-top:5px;padding-bottom:5px;font-size:9pt}.section-break {page-break-after:always;color:white;background-color:white}.funkyradio {padding-bottom:15px;}.radio-inline {font-weight:normal;}div{padding-top:10px;}.form-check {padding-left:5px;}.dont-break {margin-top:10px;page-break-inside: avoid;} .form-group{margin-bottom:8px;} div.form-group-label{padding:0;padding-top:3px;padding-bottom:3px;} .checkbox{margin:0;padding:0} .timesfont{font-size:12pt;font-family:'Times New Roman',serif} .hidden {color:white} table.accTable{width:98%;font-size:7pt;} table.servciesTable{width:98%;font-size:7pt;} p.MsoNormal, li.MsoNormal, div.MsoNormal, span.MsoNormal {font-size:11pt; font-family: 'Times New Roman',serif;} ol.IepNormal, p.IepNormal, li.IepNormal, div.IepNormal, span.IepNormal {font-size:10pt; font-family: 'Helvetica Neue, Helvetica, Arial',serif;} .radio-label { font-weight:normal;vertical-align:text-top } </style>";
                string result = "";
                if (!string.IsNullOrEmpty(HTMLContent))
                {
                    result = System.Text.RegularExpressions.Regex.Replace(HTMLContent, @"\r\n?|\n", "");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"new-line-val", "<br/>");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"not-checked", "[ &nbsp;]");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"radio-yes", "");
                }

                string cssTextResult = System.Text.RegularExpressions.Regex.Replace(cssText, @"\r\n?|\n", "");
                byte[] studentFile = null;

                if (!string.IsNullOrEmpty(StudentHTMLContent))
                {
                    string result2 = System.Text.RegularExpressions.Regex.Replace(StudentHTMLContent, @"\r\n?|\n", "");
                    result2 = System.Text.RegularExpressions.Regex.Replace(StudentHTMLContent, @"new-line-val", "<br/>");
                    result2 = System.Text.RegularExpressions.Regex.Replace(result2, @"not-checked", "[ &nbsp;]");
                    studentFile = CreatePDFBytes(cssTextResult, result2, "studentInformationPage", imgfoot, "", isDraft, false);
                }

                byte[] secondaryPageFile = null;
                if (!string.IsNullOrEmpty(HTMLContent2))
                {
                    string secondaryPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent2, @"\r\n?|\n", "");
                    secondaryPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent2, @"new-line-val", "<br/>");
                    secondaryPage = System.Text.RegularExpressions.Regex.Replace(secondaryPage, @"not-checked", "[ &nbsp;]");
                    secondaryPageFile = CreatePDFBytes(cssTextResult, secondaryPage, "module-page", imgfoot, studentName, isDraft, true);
                }

                byte[] thirdPageFile = null;
                if (!string.IsNullOrEmpty(HTMLContent3))
                {
                    string thirdPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent3, @"\r\n?|\n", "");
                    thirdPage = System.Text.RegularExpressions.Regex.Replace(HTMLContent3, @"new-line-val", "<br/>");
                    thirdPage = System.Text.RegularExpressions.Regex.Replace(thirdPage, @"not-checked", "[ &nbsp;]");
                    thirdPageFile = CreatePDFBytes(cssTextResult, thirdPage, "module-page", imgfoot, studentName, isDraft, true);
                }

                byte[] iepFile = null;
                if (!string.IsNullOrEmpty(result))
                {
                    iepFile = CreatePDFBytes(cssTextResult, result, "module-page", imgfoot, studentName, isDraft, true);
                }

                List<byte[]> pdfByteContent = new List<byte[]>();

                if (studentFile != null)
                {
                    pdfByteContent.Add(studentFile);
                }

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
                else if (secondaryPageFile != null || thirdPageFile != null)
                {
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
                {
                    formName = "Student Information";//this is just the student info page print
                }

                byte[] mergedFile = concatAndAddContent(pdfByteContent);



                if (isArchive == "1")
                {
                    try
                    {
                        string formNameValue = formName;

                        if (formName == null || formName == "IEP")
                        {
                            if (iepObj != null)
                            {
                                if (iepObj.Amendment)
                                {
                                    formNameValue = string.Format("Amendment IEP {0}", iepObj.begin_date.HasValue ? iepObj.begin_date.Value.ToShortDateString() : "");
                                }
                                else
                                {
                                    formNameValue = string.Format("Annual IEP {0}", iepObj.begin_date.HasValue ? iepObj.begin_date.Value.ToShortDateString() : "");
                                }
                            }
                        }

                        tblFormArchive archive = new tblFormArchive
                        {
                            Creator_UserID = teacher.UserID,
                            Student_UserID = id,
                            FormName = string.IsNullOrEmpty(formNameValue) ? "IEP" : formNameValue,
                            FormFile = mergedFile,
                            isActive = true,
                            ArchiveDate = DateTime.Now
                        };

                        db.tblFormArchives.Add(archive);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "";

                        if (ex is DbEntityValidationException exception)
                        {

                            if (exception.EntityValidationErrors.Any())
                            {
                                IEnumerable<DbEntityValidationResult> errors = exception.EntityValidationErrors;
                                foreach (DbEntityValidationResult failure in errors)
                                {
                                    foreach (DbValidationError error in failure.ValidationErrors)
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
            HtmlDocument doc = new HtmlDocument
            {
                OptionWriteEmptyNodes = true,
                OptionFixNestedTags = true
            };
            doc.LoadHtml(cssTextResult + "<div class='" + className + "'>" + result2 + "</div>");
            HtmlNodeCollection htmlBody = doc.DocumentNode.SelectNodes("//textarea");
            if (htmlBody != null && htmlBody.Count > 0)
            {
                foreach (HtmlNode node in htmlBody)
                {
                    node.Remove();
                }
            }

            string cleanHTML2 = doc.DocumentNode.OuterHtml;

            byte[] fileIn = null;
            byte[] printFile = null;
            using (MemoryStream cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssTextResult)))
            {
                using (MemoryStream htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cleanHTML2)))
                {
                    using (MemoryStream stream = new System.IO.MemoryStream())
                    {
                        using (MemoryStream stream2 = new System.IO.MemoryStream())
                        {

                            Document pdfDoc = new Document(PageSize.LETTER, 25, 25, 40, 50);

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

            using (MemoryStream ms = new MemoryStream())
            {
                using (Document doc = new Document())
                {
                    using (PdfSmartCopy copy = new PdfSmartCopy(doc, ms))
                    {
                        doc.Open();

                        //Loop through each byte array
                        foreach (byte[] p in pdfByteContent)
                        {

                            //Create a PdfReader bound to that byte array
                            using (PdfReader reader = new PdfReader(p))
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

        private byte[] AddPageNumber(byte[] fileIn, string studentName, iTextSharp.text.Image imgfoot, bool isDraft, bool skipSignatureDraft)
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
                        {
                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_LEFT, new Phrase(studentName, blackFont), 20f, 750f, 0);
                        }

                        if (imgfoot != null)
                        {
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
            BehaviorViewModel model = new BehaviorViewModel
            {
                StudentId = studentId,
                IEPid = iepId
            };

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
                tblBehaviorTrigger triggerOther = db.tblBehaviorTriggers.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (triggerOther != null)
                {
                    model.TriggerOther = triggerOther.OtherDescription;
                }

                model.SelectedStrategies = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorStrategyTypeID).ToList();
                tblBehaviorStrategy stratOther = db.tblBehaviorStrategies.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (stratOther != null)
                {
                    model.StrategiesOther = stratOther.OtherDescription;
                }

                model.SelectedHypothesis = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).Select(o => o.BehaviorHypothesisTypeID).ToList();
                tblBehaviorHypothesi hypoOther = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID && o.OtherDescription != "").FirstOrDefault();
                if (hypoOther != null)
                {
                    model.HypothesisOther = hypoOther.OtherDescription;
                }

                List<tblBehaviorBaseline> targetedBehaviors = db.tblBehaviorBaselines.Where(o => o.BehaviorID == BehaviorIEP.BehaviorID).ToList();
                if (targetedBehaviors.Any())
                {
                    if (targetedBehaviors[0] != null)
                    {
                        model.targetedBehavior1 = targetedBehaviors[0];
                    }

                    if (targetedBehaviors[1] != null)
                    {
                        model.targetedBehavior2 = targetedBehaviors[1];
                    }

                    if (targetedBehaviors[2] != null)
                    {
                        model.targetedBehavior3 = targetedBehaviors[2];
                    }
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

        private string GetGrade(int? value)
        {
            string studentGrade = "";

            if (value.HasValue)
            {
                switch (value)
                {
                    case -4: { studentGrade = "P3"; break; }
                    case -3: { studentGrade = "P4"; break; }
                    case -2: { studentGrade = "P5"; break; }
                    case -1: { studentGrade = "P6"; break; }
                    case 0: { studentGrade = "K"; break; }
                    default: { studentGrade = value.Value.ToString(); break; }
                }
            }

            return studentGrade;

        }

        private string GetGradeFullDescription(int? value)
        {

            string studentGrade = "";

            if (value.HasValue)
            {
                var grade = db.tblGrades.Where(o => o.gradeID == value.Value).FirstOrDefault();
                if (grade != null)
                    return grade.description;
            }

            return studentGrade;

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

        private void OutputResponse(byte[] memoryStream, string fileName, string contentType)
        {
            Response.Clear();

            Response.ContentType = contentType; //"application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);

            Response.BinaryWrite(memoryStream);
            Response.End();
        }

        private List<NotificationViewModel> GetDraftIeps(string studentIds)
        {
            return db.up_ReportDraftIEPS(null, null, null, studentIds).Where(o => o.DraftDays > 0).Select(u => new NotificationViewModel()
            {
                StudentId = u.UserID,
                StudentFirstName = u.StudentFirstName,
                StudentLastName = u.StudentLastName,
                Days = u.DraftDays ?? 0
            }).ToList();

        }

        private List<NotificationViewModel> GetEvalsDue(string studentIds)
        {
            return db.up_ReportProceduralDates(null, null, null, studentIds, 60).Where(o => o.DateType == "3 YEAR").OrderBy(o => o.EvalDate).Select(u => new NotificationViewModel()
            {
                StudentId = u.UserID,
                StudentFirstName = u.StudentFirstName,
                StudentLastName = u.StudentLastName,
                EvalType = u.DateType
            }).ToList();

        }

        private List<NotificationViewModel> GetIepsDue(string studentIds)
        {
            return db.up_ReportIEPSDue(null, null, null, studentIds, 60).Select(u => new NotificationViewModel()
            {
                StudentId = u.UserID,
                StudentFirstName = u.StudentFirstName,
                StudentLastName = u.StudentLastName,
                Days = u.NumberOfDays.HasValue ? u.NumberOfDays.Value : 0
            }).ToList();

        }

        private string SpedProEndDateCheck(DateTime? meetingDate, tblService service)
        {
            string serviceEndDateOverride = "";

            if (meetingDate != null)
            {
                //we have a problem, we need need to end 1 day prior to the new annual beginning date, if it is a valid date
                int endDayCount = 1;
                while (endDayCount < 150)
                {
                    var newEndDate = meetingDate.Value.AddDays(-endDayCount);
                    var isValidEndDate = db.tblCalendars.Any(c => c.BuildingID == service.BuildingID && (c.canHaveClass == true && c.NoService == false) && c.calendarDate == newEndDate);
                    if (isValidEndDate)
                    {
                        serviceEndDateOverride = newEndDate.ToShortDateString();
                        break;
                    }
                    endDayCount++;
                }
            }

            return serviceEndDateOverride;

        }

        private bool ArchiveIEPPrint(int studentId, IEP theIEP, bool includeProgressReport)
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
                    theIEP.studentDetails.printContingencyPlan = true;
                    theIEP.studentDetails.isArchive = true;
                    theIEP.isServerRender = true;
                }

                string data = RenderRazorViewToString("~/Views/Home/Print/_PrintPartial.cshtml", theIEP);

                string result = System.Text.RegularExpressions.Regex.Replace(data, @"\r\n?|\n|\t", "");
                result = System.Text.RegularExpressions.Regex.Replace(result, @"break-line-val", "<br/>");
                HtmlDocument doc = new HtmlDocument
                {
                    OptionWriteEmptyNodes = true,
                    OptionFixNestedTags = true
                };
                doc.LoadHtml(result);

                string fileName = string.Format("{0} {1}", theIEP.current.Amendment ? "Amendment IEP" : "Annual IEP", theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString());

                HtmlNode studentInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("studentInformationPage")).FirstOrDefault();
                HtmlNode moduleInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("module-page")).FirstOrDefault();
                byte[] mergedFile = CreateIEPPdf(studentInfo.InnerHtml, moduleInfo.InnerHtml, "", "", "", studentId.ToString(), "1", theIEP.current.IEPid.ToString(), "1", string.Format("{0}", fileName));

                if (includeProgressReport)
                {
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
                        theIEP.studentDetails.printContingencyPlan = false;
                        theIEP.studentDetails.isArchive = true;
                        theIEP.isServerRender = true;
                    }

                    data = RenderRazorViewToString("~/Views/Home/Print/_PrintPartial.cshtml", theIEP);

                    result = System.Text.RegularExpressions.Regex.Replace(data, @"\r\n?|\n|\t", "");
                    result = System.Text.RegularExpressions.Regex.Replace(result, @"break-line-val", "<br/>");
                    doc = new HtmlDocument
                    {
                        OptionWriteEmptyNodes = true,
                        OptionFixNestedTags = true
                    };
                    doc.LoadHtml(result);

                    HtmlNode progressModuleInfo = doc.DocumentNode.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("module-page")).FirstOrDefault();
                    byte[] progressReport = CreateIEPPdf("", progressModuleInfo.InnerHtml, "", "", "", studentId.ToString(), "1", theIEP.current.IEPid.ToString(), "1", string.Format("Progress Report {0}", theIEP.current.begin_date.HasValue ? theIEP.current.begin_date.Value.ToShortDateString() : DateTime.Now.ToShortDateString()));
                }

                success = true;

            }
            catch (Exception e)
            {
                success = false;
                Console.Write(e.InnerException.ToString());
            }

            return success;
        }

      
    }
}