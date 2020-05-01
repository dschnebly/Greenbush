﻿using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Report
{


    public partial class ReportMaster : System.Web.UI.MasterPage
    {
        public const string owner = "1"; //level 5
        public const string mis = "2"; //level 4
        public const string admin = "3"; //level 3
        public const string teacher = "4"; //level 2
        public const string student = "5";
        public const string nurse = "6"; //level 1
        public static IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();
        protected string DisplayName { get; set; }
        protected string UserLevel { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account/Login");
            }

            tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == HttpContext.Current.User.Identity.Name);
            if (user.RoleID == student)
            {
                Server.Transfer("Error", true);
            }
            UserLevel = user.RoleID;
            DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName);
        }

        public string GetUserLevel => UserLevel;

        public static void StudentStatusList(HtmlSelect statusDD)
        {

            List<tblStatusCode> statusList = db.tblStatusCodes.ToList();
            //statusList.Insert(0, new tblStatusCode() { StatusCode = "All" });
            statusDD.DataSource = statusList;
            statusDD.DataTextField = "StatusCode";
            statusDD.DataValueField = "StatusCode";
            statusDD.DataBind();
        }

        public static void StatusList(HtmlSelect statusDD)
        {

            List<SelectListItem> statusList = GreenBushIEP.Report.ReportMaster.GetIEPStatuses();
            statusDD.DataSource = statusList;
            statusDD.DataTextField = "Text";
            statusDD.DataValueField = "Value";
            statusDD.DataBind();
        }

        public static void TeacherList(HtmlSelect teacherDD)
        {
            List<TeacherView> teacherList = GreenBushIEP.Report.ReportMaster.GetTeachers(HttpContext.Current.User.Identity.Name);
            teacherDD.DataSource = teacherList;
            teacherDD.DataTextField = "Name";
            teacherDD.DataValueField = "UserID";
            teacherDD.DataBind();
        }

        public static void DistrictList(HtmlSelect districtDD)
        {
            List<tblDistrict> districts = GreenBushIEP.Report.ReportMaster.GetDistricts(HttpContext.Current.User.Identity.Name);
            districtDD.DataSource = districts;
            districtDD.DataTextField = "DistrictName";
            districtDD.DataValueField = "USD";
            districtDD.DataBind();
        }

        public static void BuildingList(HtmlSelect buildingDD)
        {

            List<BuildingsViewModel> buildingList = GreenBushIEP.Report.ReportMaster.GetBuildings(HttpContext.Current.User.Identity.Name);
            buildingDD.DataSource = buildingList;
            buildingDD.DataTextField = "BuildingName";
            buildingDD.DataValueField = "BuildingID";
            buildingDD.DataBind();
        }

        public static void StudentList(HtmlSelect studentDD)
        {
            // List<Student> GetStudents(string userName)
            List<Student> studentList = GreenBushIEP.Report.ReportMaster.GetStudents(HttpContext.Current.User.Identity.Name);
            studentDD.DataSource = studentList;
            studentDD.DataTextField = "FormattedName";
            studentDD.DataValueField = "UserID";
            studentDD.DataBind();
        }


        public static void ServiceList(HtmlSelect serviceDD)
        {
            List<tblServiceType> services = GreenBushIEP.Report.ReportMaster.GetServices();
            serviceDD.DataSource = services;
            serviceDD.DataTextField = "Name";
            serviceDD.DataValueField = "ServiceCode";
            serviceDD.DataBind();
        }

        public static void ProviderList(HtmlSelect providerDD)
        {
            List<ProviderViewModel> providerList = GreenBushIEP.Report.ReportMaster.GetProviders(HttpContext.Current.User.Identity.Name);
            providerDD.DataSource = providerList;
            providerDD.DataTextField = "Name";
            providerDD.DataValueField = "ProviderID";
            providerDD.DataBind();
        }

        public static List<TeacherView> GetTeachers(string userName)
        {
            //List<tblUser> teachers = new List<tblUser>();
            List<TeacherView> teacherList = new List<TeacherView>();
            tblUser usr = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);
            if (usr.RoleID == GreenBushIEP.Report.ReportMaster.teacher || usr.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
            {
                //just add themselves to the list

                TeacherView tv = new TeacherView() { Name = string.Format("{0}, {1}", usr.LastName, usr.FirstName), UserID = usr.UserID };
                teacherList.Add(tv);

            }
            else
            {
                teacherList = GetTeacherByRole(usr.UserID, usr.RoleID);
            }


            return teacherList.OrderBy(o => o.Name).ToList();
        }

        private static List<TeacherView> GetTeacherByRole(int userId, string userRoleId)
        {
            List<TeacherView> list = new List<TeacherView>();

            try
            {

                List<string> myDistricts = new List<string>();
                List<string> myBuildings = new List<string>();
                List<string> myRoles = new List<string>() { "3", "4", "6" };
                List<vw_UserList> teachers = new List<vw_UserList>();

                var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == userId select new { district.USD, district.DistrictName }).Distinct().ToList();
                myDistricts = districts.Select(d => d.USD).ToList();

                if (userRoleId == "2" || userRoleId == "1")
                {
                    myRoles.Add("2");
                    teachers = db.vw_UserList
                                .Where(ul => myRoles.Contains(ul.RoleID) && myDistricts.Contains(ul.USD))
                                .GroupBy(u => u.UserID)
                                .Select(u => u.FirstOrDefault()).ToList();

                }
                else
                {
                    List<tblBuilding> buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == userId && myDistricts.Contains(buildingMap.USD) select building).Distinct().ToList();
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");

                    teachers = db.vw_UserList
                            .Where(ul => myRoles.Contains(ul.RoleID) && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID))
                            .GroupBy(u => u.UserID)
                            .Select(u => u.FirstOrDefault()).ToList();
                }

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

        public static List<SelectListItem> GetIEPStatuses()
        {
            List<SelectListItem> statuses = new List<SelectListItem>
            {
                new SelectListItem() { Value = IEPStatus.ACTIVE, Text = IEPStatus.ACTIVE },
                new SelectListItem() { Value = "AMENDED", Text = "AMENDED" },
                new SelectListItem() { Value = IEPStatus.AMENDMENT, Text = IEPStatus.AMENDMENT },
                new SelectListItem() { Value = IEPStatus.ANNUAL, Text = IEPStatus.ANNUAL },
                new SelectListItem() { Value = IEPStatus.ARCHIVE, Text = IEPStatus.ARCHIVE },
                new SelectListItem() { Value = IEPStatus.DELETED, Text = IEPStatus.DELETED },
                new SelectListItem() { Value = IEPStatus.DRAFT, Text = IEPStatus.DRAFT },
                new SelectListItem() { Value = IEPStatus.PLAN, Text = IEPStatus.PLAN }
            };


            return statuses;


        }

        public static List<ProviderViewModel> GetProviders(string userName)
        {
            List<ProviderViewModel> providerList = new List<ProviderViewModel>();
            tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

            List<tblDistrict> MISDistrictList = (from buildingMaps in db.tblBuildingMappings
                                                 join districts in db.tblDistricts
                                                      on buildingMaps.USD equals districts.USD
                                                 where buildingMaps.UserID == user.UserID
                                                 select districts).Distinct().ToList();

            List<string> listOfUSD = MISDistrictList.Select(d => d.USD).ToList();

            List<tblProvider> listOfProviders = new List<tblProvider>();
            listOfProviders = (from providers in db.tblProviders
                               join districts in db.tblProviderDistricts
                                    on providers.ProviderID equals districts.ProviderID
                               where listOfUSD.Contains(districts.USD)
                               select providers).Distinct().ToList();

            foreach (tblProvider provider in listOfProviders)
            {
                providerList.Add(new ProviderViewModel() { Name = string.Format("{0}, {1}", provider.LastName, provider.FirstName), ProviderCode = provider.ProviderCode, ProviderID = provider.ProviderID });
            }

            return providerList.OrderBy(o => o.Name).ToList();
        }

        public static List<tblServiceType> GetServices()
        {
            List<tblServiceType> services = new List<tblServiceType>
            {
                new tblServiceType { Name = "All", ServiceCode = "-1" }
            };

            services.AddRange(db.tblServiceTypes.ToList());

            return services;
        }

        public static string CurrentUser(string userName)
        {
            tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == userName);
            return string.Format("{0} {1}", user.FirstName, user.LastName);
        }

        public static tblUser GetUser(string userName)
        {
            return db.tblUsers.SingleOrDefault(o => o.Email == userName);

        }

        public static List<tblDistrict> GetDistricts(string userName)
        {
            tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

            List<tblDistrict> districtList = (from org in db.tblOrganizationMappings
                                              join district in db.tblDistricts on org.USD equals district.USD
                                              where org.UserID == user.UserID
                                              orderby district.DistrictName
                                              select district).Distinct().ToList();

            List<tblDistrict> districts = new List<tblDistrict>
            {
                new tblDistrict { DistrictName = "All", USD = "-1" }
            };

            districts.AddRange(districtList);

            return districts;
        }

        public static List<BuildingsViewModel> GetBuildings(string userName)
        {
            tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

            List<BuildingsViewModel> buildings = new List<BuildingsViewModel>();

            BuildingsViewModel allOption = new BuildingsViewModel() { BuildingName = "All", BuildingID = "-1", BuildingUSD = "-1" };
            buildings.Add(allOption);

            List<BuildingsViewModel> buildingList = (from bm in db.tblBuildingMappings
                                                     join b in db.tblBuildings on bm.USD equals b.USD
                                                     where b.Active == 1 && bm.BuildingID == b.BuildingID && bm.UserID == user.UserID
                                                     orderby b.BuildingName
                                                     select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

            buildings.AddRange(buildingList);

            return buildings;
        }

        public static List<Student> GetStudents(string userName)
        {
            tblUser teacherObj = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);
            List<Student> studentList = new List<Student>();

            Student allOption = new Student() { FirstName = "All", UserID = -1 };


            studentList.Add(allOption);

            if (teacherObj.RoleID == teacher || teacherObj.RoleID == nurse)
            {
                IQueryable<Student> list = (from u in db.tblUsers
                                            join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                                            where o.AdminID == teacherObj.UserID && u.RoleID == student
                                            select new Student()
                                            {
                                                UserID = u.UserID,
                                                FirstName = u.FirstName,
                                                MiddleName = u.MiddleName,
                                                LastName = u.LastName,
                                            }).Distinct();

                studentList.AddRange(list.OrderBy(o => o.LastName).ThenBy(o => o.FirstName));
            }
            else
            {

                List<TeacherView> teachers = (from u in db.tblUsers
                                              join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                                              where o.AdminID == teacherObj.UserID && (u.RoleID != student)
                                              select new TeacherView()
                                              {
                                                  UserID = u.UserID,
                                              }).Distinct().ToList();

                List<int> teacherIds = teachers.Select(d => d.UserID).ToList();


                IQueryable<Student> list = (from u in db.tblUsers
                                            join o in db.tblOrganizationMappings on u.UserID equals o.UserID
                                            where (teacherIds.Contains(o.AdminID) || o.AdminID == teacherObj.UserID) && u.RoleID == student && (!u.Archive.HasValue || u.Archive.Value)
                                            select new Student()
                                            {
                                                UserID = u.UserID,
                                                FirstName = u.FirstName,
                                                MiddleName = u.MiddleName,
                                                LastName = u.LastName,
                                            }).Distinct();

                studentList.AddRange(list.OrderBy(o => o.LastName).ThenBy(o => o.FirstName));

            }


            return studentList;

        }

        public static List<BuildingsViewModel> GetBuildingsByDistrict(string userName, string usd)
        {
            tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

            List<BuildingsViewModel> buildings = new List<BuildingsViewModel>();

            List<BuildingsViewModel> buildingList = (from bm in db.tblBuildingMappings
                                                     join b in db.tblBuildings on bm.USD equals b.USD
                                                     where bm.UserID == user.UserID && b.Active == 1 && bm.BuildingID == b.BuildingID && b.USD == usd
                                                     select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

            buildings.AddRange(buildingList);

            return buildings;
        }

        public static DataTable GetBuildingData(string id)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BuildingName", typeof(string));
            dt.Columns.Add("Address_Mailing", typeof(string));
            dt.Columns.Add("Zip", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("StateName", typeof(string));

            using (IndividualizedEducationProgramEntities ctx = new IndividualizedEducationProgramEntities())
            {
                //Execute stored procedure as a function
                int.TryParse(id, out int buildingID);
                System.Data.Entity.Core.Objects.ObjectResult<up_ReportBuildings_Result> list = ctx.up_ReportBuildings(buildingID);

                foreach (up_ReportBuildings_Result cs in list)
                {
                    dt.Rows.Add(cs.BuildingName, cs.Address_Mailing, cs.Zip, cs.City, cs.Phone, cs.StateName);
                }
            }

            return dt;
        }

        public static string GetTeacherFilter(HtmlSelect teacherDD, string teacherId)
        {
            string teacherList = "";

            if (teacherId == "")
            {
                foreach (ListItem li in teacherDD.Items)
                {
                    if (li.Selected)
                    {
                        teacherList += string.Format("{0},", li.Value);
                    }
                }

                if (string.IsNullOrEmpty(teacherList))
                {
                    //get all, but limit list
                    List<TeacherView> providerList = GreenBushIEP.Report.ReportMaster.GetTeachers(HttpContext.Current.User.Identity.Name);
                    teacherList = string.Join(",", providerList.Select(o => o.UserID));
                }
            }
            else
            {
                teacherList = teacherId;
            }

            return teacherList;
        }

        public static string GetServiceFilter(HtmlSelect ServiceType)
        {
            string serviceIds = "";
            foreach (ListItem li in ServiceType.Items)
            {
                if (li.Selected)
                {
                    serviceIds += string.Format("{0},", li.Value);
                }
            }
            return serviceIds;
        }

        public static string GetDistrictFilter(HtmlSelect districtDD, string districtID)
        {
            string districtList = "";
            if (districtID == "-1")
            {
                foreach (ListItem districtItem in districtDD.Items)
                {
                    districtList += string.Format("{0},", districtItem.Value);
                }

            }
            else
            {
                districtList = districtID;
            }

            return districtList;
        }





        public static string GetBuildingFilter(HtmlSelect districtDD, string buildingID, string districtID)
        {
            string buildingList = "";
            if (buildingID == "-1")
            {
                if (districtID == "-1")
                {
                    foreach (ListItem districtItem in districtDD.Items)
                    {
                        List<BuildingsViewModel> selectedBuildings = GreenBushIEP.Report.ReportMaster.GetBuildingsByDistrict(HttpContext.Current.User.Identity.Name, districtItem.Value);
                        foreach (BuildingsViewModel b in selectedBuildings)
                        {
                            buildingList += string.Format("{0},", b.BuildingID);
                        }
                    }
                }
                else
                {
                    List<BuildingsViewModel> selectedBuildings = GreenBushIEP.Report.ReportMaster.GetBuildingsByDistrict(HttpContext.Current.User.Identity.Name, districtID);
                    foreach (BuildingsViewModel b in selectedBuildings)
                    {
                        buildingList += string.Format("{0},", b.BuildingID);
                    }
                }
            }
            else
            {
                buildingList = buildingID;
            }

            return buildingList;
        }



        //public static bool IsStudent(string userName)
        //{
        //	tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == userName);
        //	if (MIS != null)
        //	{


        //		var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == MIS.UserID select district).Distinct();
        //		var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == MIS.UserID select building).Distinct();
        //		var students = (from buildingMap in db.tblBuildingMappings join user in db.tblUsers on buildingMap.UserID equals user.UserID where ( ) lect new StudentIEPViewModel() { UserID = user.UserID, FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, RoleID = user.RoleID }).Distinct().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).Any(m => m.RoleID == student && m.user);

        //		foreach (var student in model.members.Where(m => m.RoleID == student))
        //		{
        //			student.hasIEP = db.tblIEPs.Where(i => i.UserID == student.UserID && i.IsActive && i.IepStatus != IEPStatus.ARCHIVE).Any();
        //		}

        //		// show the latest updated version changes
        //		ViewBag.UpdateCount = VersionCompare.GetVersionCount(MIS);

        //		return View("MISPortal", model);
        //	}
        //}

    }
}