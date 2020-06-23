using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Data;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using System.Web.Services;

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
			this.UserLevel = user.RoleID;
			this.DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName);
		}

		public string GetUserLevel
		{
			get
			{
				return UserLevel;
			}

		}

		public static void StudentStatusList(HtmlSelect statusDD)
		{

			var statusList = db.tblStatusCodes;
			var statuses = new List<SelectListItem>();

			//statusList.Insert(0, new tblStatusCode() { StatusCode = "All" });

			foreach (var status in statusList)
			{
				statuses.Add(new SelectListItem() { Text = string.Format("({0}) {1}", status.StatusCode, status.Description), Value=status.StatusCode });
			}
			statusDD.DataSource = statuses;
			statusDD.DataTextField = "Text";
			statusDD.DataValueField = "Value";
			statusDD.DataBind();
		}

		public static void StatusList(HtmlSelect statusDD)
		{	

			var statusList = GreenBushIEP.Report.ReportMaster.GetIEPStatuses();
			statusDD.DataSource = statusList;
			statusDD.DataTextField = "Text";
			statusDD.DataValueField = "Value";
			statusDD.DataBind();
		}		

		public static void TeacherList(HtmlSelect teacherDD, string selectedDistrict, string selectedBuilding, HtmlInputHidden teacherValsInput)
		{
			string selectedTeacher = teacherValsInput.Value;

			var teacherList = GreenBushIEP.Report.ReportMaster.GetTeachers(HttpContext.Current.User.Identity.Name,  selectedDistrict,  selectedBuilding);


			if (teacherList.Count() > 1)
			{
				var allOption = new TeacherView() { Name = "All", UserID =-1 };
				teacherList.Insert(0, allOption);
			}


			teacherDD.DataSource = teacherList;
			teacherDD.DataTextField = "Name";
			teacherDD.DataValueField = "UserID";
			teacherDD.DataBind();

			if (!string.IsNullOrEmpty(selectedTeacher) || selectedTeacher != "-1")
			{
				var teacherIds = selectedTeacher.Split(',');

				if (teacherIds != null)
				{
					foreach (var t in teacherIds)
					{
						var item = teacherDD.Items.FindByValue(t);
						if (item != null)
							item.Selected = true;
					}
				}
			}
		}

		public static void DistrictList(HtmlSelect districtDD)
		{
			var districts = GreenBushIEP.Report.ReportMaster.GetDistricts(HttpContext.Current.User.Identity.Name);
			districtDD.DataSource = districts;
			districtDD.DataTextField = "DistrictName";
			districtDD.DataValueField = "USD";
			districtDD.DataBind();
		}

		public static void BuildingList(HtmlSelect buildingDD, string selectedDistrict)
		{
			var selectedBuilding = buildingDD.Value;

			var buildingList = GreenBushIEP.Report.ReportMaster.GetBuildings(HttpContext.Current.User.Identity.Name);

			if (!string.IsNullOrEmpty(selectedDistrict) && selectedDistrict != "-1")
				buildingList = buildingList.Where(o => o.BuildingUSD == selectedDistrict).ToList();

			if (buildingList.Count() > 1)
			{
				var allOption = new BuildingsViewModel() { BuildingName = "All", BuildingID = "-1", BuildingUSD = "-1" };
				buildingList.Insert(0,allOption);
			}

			buildingDD.DataSource = buildingList;
			buildingDD.DataTextField = "BuildingName";
			buildingDD.DataValueField = "BuildingID";
			buildingDD.DataBind();


			if (!string.IsNullOrEmpty(selectedBuilding) || selectedBuilding != "-1")
			{
				var item = buildingDD.Items.FindByValue(selectedBuilding);
				if(item != null)
					item.Selected = true;				
			}

		}

		public static void StudentList(HtmlSelect studentDD, string selectedDistrict, string selectedBuilding, string selectedTeacher, HtmlInputHidden studentValsInput)
		{
			var selectedStudent = studentValsInput.Value;

			var studentList = GreenBushIEP.Report.ReportMaster.GetStudents(HttpContext.Current.User.Identity.Name, selectedDistrict, selectedBuilding, selectedTeacher);
			studentDD.DataSource = studentList;
			studentDD.DataTextField = "LastName";
			studentDD.DataValueField = "UserID";
			studentDD.DataBind();

			if (!string.IsNullOrEmpty(selectedStudent) || selectedStudent != "-1")
			{
				var item = studentDD.Items.FindByValue(selectedStudent);
				if (item != null)
					item.Selected = true;
			}

		}

		public static void StudentListByProvider(HtmlSelect studentDD, string selectedDistrict, string selectedBuilding, string selectedProvider, HtmlInputHidden studentValsInput)
		{
			var selectedStudent = studentValsInput.Value;

			var studentList = GreenBushIEP.Report.ReportMaster.GetStudentsByProvider(HttpContext.Current.User.Identity.Name, selectedDistrict, selectedBuilding, selectedProvider);
			studentDD.DataSource = studentList;
			studentDD.DataTextField = "LastName";
			studentDD.DataValueField = "UserID";
			studentDD.DataBind();

			if (!string.IsNullOrEmpty(selectedStudent) || selectedStudent != "-1")
			{
				var item = studentDD.Items.FindByValue(selectedStudent);
				if (item != null)
					item.Selected = true;
			}

		}


		public static void ServiceList(HtmlSelect serviceDD)
		{
			var services = GreenBushIEP.Report.ReportMaster.GetServices();
			serviceDD.DataSource = services;
			serviceDD.DataTextField = "Name";
			serviceDD.DataValueField = "ServiceCode";
			serviceDD.DataBind();

		}

		public static void ProviderList(HtmlSelect providerDD, string selectedDistricts, HtmlInputHidden providerValsInput)
		{
			var selectedProvider = providerValsInput.Value;

			var providerList = GreenBushIEP.Report.ReportMaster.GetProviders(selectedDistricts);
			providerDD.DataSource = providerList;
			providerDD.DataTextField = "Name";
			providerDD.DataValueField = "ProviderID";
			providerDD.DataBind();

			if (selectedProvider != "-1")
				selectedDistricts = "";


			if (!string.IsNullOrEmpty(selectedProvider))
			{
				var providerIds = selectedProvider.Split(',');

				if (providerIds != null)
				{
					foreach (var t in providerIds)
					{
						var item = providerDD.Items.FindByValue(t);
						if (item != null)
							item.Selected = true;
					}
				}

			}
		}

		

		public static List<TeacherView> GetTeachers(string userName, string selectedDistricts, string selectedBuildings)
		{
			
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
				teacherList = GetTeacherByDistrictBuilding(usr, selectedBuildings, selectedDistricts);			
			}						

			return teacherList.OrderBy(o => o.Name).ToList();
		}


		private static List<TeacherView> GetTeacherByDistrictBuilding(tblUser user, string selectedBuildings, string selectedDistricts)
		{
			List<TeacherView> teachers = new List<TeacherView>();

			try
			{
				
				List<string> myDistricts = string.IsNullOrEmpty(selectedDistricts) ? new List<string>() : selectedDistricts.Split(',').ToList();
				List<string> myBuildings = string.IsNullOrEmpty(selectedBuildings) ? new List<string>() : selectedBuildings.Split(',').ToList();

				List<string> myRoles = new List<string>() { "3", "4", "6" };

				if (user.RoleID == mis || user.RoleID == owner)
					myRoles.Add("2");

				teachers = db.vw_UserList.Where(ul => 
				myRoles.Contains(ul.RoleID)
				&& (myBuildings.Any() && myBuildings.Contains(ul.BuildingID)
				&& (myBuildings.Any() && myDistricts.Contains(ul.USD))))
				.Select(u => new TeacherView() { UserID = u.UserID, Name = u.LastName + ", " + u.FirstName })
				.Distinct()
				.OrderBy(o => o.Name)
				.ToList();

				
			}
			catch (Exception ex)
			{
				string error = ex.ToString();
			}

			return teachers;

		}

		public static List<SelectListItem> GetIEPStatuses()
		{
			var statuses = new List<SelectListItem>();
			statuses.Add(new SelectListItem() { Value = IEPStatus.ACTIVE, Text = IEPStatus.ACTIVE });
			statuses.Add(new SelectListItem() { Value = "AMENDED", Text = "AMENDED" });
			statuses.Add(new SelectListItem() { Value = IEPStatus.AMENDMENT, Text = IEPStatus.AMENDMENT });
			statuses.Add(new SelectListItem() { Value = IEPStatus.ANNUAL, Text = IEPStatus.ANNUAL });
			statuses.Add(new SelectListItem() { Value = IEPStatus.ARCHIVE, Text = IEPStatus.ARCHIVE });
			statuses.Add(new SelectListItem() { Value = IEPStatus.DELETED, Text = IEPStatus.DELETED });
			statuses.Add(new SelectListItem() { Value = IEPStatus.DRAFT, Text = IEPStatus.DRAFT });
			statuses.Add(new SelectListItem() { Value = IEPStatus.PLAN, Text = IEPStatus.PLAN });
			

			return statuses;


		}

		public static ProviderViewModel GetProviderByProviderCode(string providerCode)
		{
			var providerObj = new ProviderViewModel();
			providerObj.ProviderID = 0;

			string providerVal = string.IsNullOrEmpty(providerCode) ? "" : providerCode.Trim();

			var provider = (from p in db.tblProviders
							where p.ProviderCode == providerVal
							select p).FirstOrDefault();

			if (provider != null)
			{
				providerObj.Name = string.Format("{0}, {1}", provider.LastName, provider.FirstName);
				providerObj.ProviderID = provider.ProviderID;
				providerObj.ProviderCode = provider.ProviderCode;
			}

			return providerObj;
		}

		public static List<ProviderViewModel> GetProviders(string selectedDistrict)
		{
			//tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == HttpContext.Current.User.Identity.Name);
			
			//if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			//{
			//	var providerList = new List<ProviderViewModel>();

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

			//	return providerList;

			//}
			
			
			if (selectedDistrict == "" || selectedDistrict == "-1")
			{

				var providerList = new List<ProviderViewModel>();

				var districts = GreenBushIEP.Report.ReportMaster.GetDistricts(HttpContext.Current.User.Identity.Name);

				foreach (var district in districts)
				{
					var districtProvider = db.uspServiceProviders(district.USD)
						.Select(u => new ProviderViewModel() { ProviderID = u.ProviderID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) })
						;

					providerList.AddRange(districtProvider);					
					
				}

				var all = providerList
					.GroupBy(o => o.ProviderID)
					.Select(u => u.FirstOrDefault())		
					.OrderBy(o => o.Name)
					.ToList();

				return all;

			}
			else
			{
				var providers = db.uspServiceProviders(selectedDistrict)
					.Select(u => new ProviderViewModel() { ProviderCode = u.ProviderCode, ProviderID = u.ProviderID, Name = string.Format("{0}, {1}", u.LastName, u.FirstName) });

				return providers.Distinct().OrderBy(o => o.Name).ToList();
			}			
		}

		public static List<tblServiceType> GetServices()
		{
			List<tblServiceType> services = new List<tblServiceType>();

			services.Add(new tblServiceType {  Name = "All", ServiceCode = "-1"});

			services.AddRange(db.tblServiceTypes.ToList());

			return services;
		}

		public static string CurrentUser(string userName)
		{
			var user = db.tblUsers.SingleOrDefault(o => o.Email == userName);
			return string.Format("{0} {1}", user.FirstName, user.LastName);
		}

		public static tblUser GetUser(string userName)
		{
			return db.tblUsers.SingleOrDefault(o => o.Email == userName);
			
		}		

		public static List<tblDistrict> GetDistricts(string userName)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			var districtList = (from org in db.tblOrganizationMappings
								join district in db.tblDistricts on org.USD equals district.USD
								where org.UserID == user.UserID
								orderby district.DistrictName
								select district).Distinct().ToList();

			List<tblDistrict> districts = new List<tblDistrict>();

			if(districtList.Count() > 1)
				districts.Add(new tblDistrict {  DistrictName = "All",  USD = "-1" });

			districts.AddRange(districtList);

			return districts;
		}

		public static List<BuildingsViewModel> GetBuildings(string userName)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			var buildings = new List<BuildingsViewModel>();

			var buildingList = (from bm in db.tblBuildingMappings
								join b in db.tblBuildings on bm.USD equals b.USD
								where b.Active == 1 && bm.BuildingID == b.BuildingID && bm.UserID == user.UserID
								orderby b.BuildingName
								select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

			buildings.AddRange(buildingList);
			
			return buildings;
		}

		public static List<Student> GetStudents(string userName, string selectedDistrict, string selectedBuilding, string selectedTeacher)
		{
			List<string> myRoles = new List<string>() { "5" };
			List<Student> studentList = new List<Student>();

			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				//can only see their own students
				var students = db.uspUserList(user.UserID, selectedDistrict, selectedBuilding, null, null)
						.Where(ul => myRoles.Contains(ul.RoleID))
						.Select(u => new Student() { UserID = u.UserID, LastName = u.LastName + ", " + u.FirstName });


				var teacherStudents = (from student in students
									   join o in db.tblOrganizationMappings on student.UserID equals o.UserID
									   where o.AdminID == user.UserID
									   select new Student()
									   {
										   UserID = student.UserID,										   
										   LastName = student.LastName,
									   }).OrderBy(o => o.LastName).ThenBy(o => o.FirstName).Distinct().ToList();

				teacherStudents.Insert(0, new Student() { LastName = "All", UserID = -1 });

				return teacherStudents;


			}
			else
			{

				if (string.IsNullOrEmpty(selectedTeacher))
				{
					//get based on user id and district and building
					var students = db.uspUserList(user.UserID, selectedDistrict, selectedBuilding, null, null)
					.Where(ul => myRoles.Contains(ul.RoleID))
					.Select(u => new Student() { UserID = u.UserID, LastName = u.LastName + ", " + u.FirstName })
					.OrderBy(o => o.LastName).ThenBy(o => o.FirstName).Distinct().ToList();

					students.Insert(0, new Student() { LastName = "All", UserID = -1 });

					return students;

				}
				else
				{
					//get based on selected teachers
					selectedTeacher = selectedTeacher.TrimEnd(',');

					List<string> myTeachers = string.IsNullOrEmpty(selectedTeacher) ? new List<string>() : selectedTeacher.Split(',').ToList();

					foreach (var teacher in myTeachers)
					{
						var teacherId = 0;
						Int32.TryParse(teacher, out teacherId);

						var students = db.uspUserList(teacherId, selectedDistrict, selectedBuilding, null, null)
						.Where(ul => myRoles.Contains(ul.RoleID))
						.Select(u => new Student() { UserID = u.UserID, LastName = u.LastName + ", " + u.FirstName });


						var teacherStudents = (from student in students
											   join o in db.tblOrganizationMappings on student.UserID equals o.UserID
											   where o.AdminID == teacherId
											   select new Student()
											   {
												   UserID = student.UserID,												   
												   LastName = student.LastName
											   }).ToList();


						studentList.AddRange(teacherStudents);
					}

					studentList.Insert(0, new Student() { LastName = "All", UserID = -1 });

					return studentList.Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

				}
			}
				
		}

		public static List<Student> GetStudentsByProvider(string userName, string selectedDistrict, string selectedBuilding, string selectedProvider)
		{
			List<string> myRoles = new List<string>() { "5" };
			List<Student> studentList = new List<Student>();

			if (selectedBuilding == "-1")
				selectedBuilding = null;

			if (selectedProvider == "-1")
				selectedProvider = null;

			if (selectedDistrict == "-1")
				selectedDistrict = null;

			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);
					

			if (string.IsNullOrEmpty(selectedProvider))
			{
				//get based on user id and district and building
				var students = db.uspUserListByProvider(user.UserID, selectedDistrict, selectedBuilding, null)
					
				.Select(u => new Student() { UserID = u.UserID, LastName = u.LastName + ", " + u.FirstName })
				.OrderBy(o => o.LastName).ThenBy(o => o.FirstName).Distinct().ToList();

				students.Insert(0, new Student() { LastName = "All", UserID = -1 });

				return students;

			}
			else
			{
				//get based on selected provider
				selectedProvider = selectedProvider.TrimEnd(',');

				List<string> myProviders = string.IsNullOrEmpty(selectedProvider) ? new List<string>() : selectedProvider.Split(',').ToList();

				foreach (var provider in myProviders)
				{
					var providerId = 0;
					Int32.TryParse(provider, out providerId);

					var students = db.uspUserListByProvider(user.UserID, selectedDistrict, selectedBuilding, providerId)
					.Select(u => new Student() { UserID = u.UserID, LastName = u.LastName + ", " + u.FirstName })
					.OrderBy(o => o.LastName);

					studentList.AddRange(students);
				}

				studentList.Insert(0, new Student() { LastName = "All", UserID = -1 });

				return studentList.Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

			}		

		}

		public static List<BuildingsViewModel> GetBuildingsByDistrict(string userName, string usd)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			var buidlingList = GetBuildings(userName);
					
			if (!string.IsNullOrEmpty(usd) && usd != "-1")
			{
				return buidlingList.Where(o => o.BuildingUSD == usd).ToList();
			}
			else
			{
				return buidlingList;
			}
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

			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				int buildingID = 0;
				Int32.TryParse(id, out buildingID);
				var list = ctx.up_ReportBuildings(buildingID);

				foreach (var cs in list)
					dt.Rows.Add(cs.BuildingName, cs.Address_Mailing, cs.Zip, cs.City, cs.Phone, cs.StateName);
			}

			return dt;
		}

		public static string GetTeacherFilter(HtmlSelect teacherDD, tblUser user, string selectedBuildings, string selectedDistricts, HtmlInputHidden teacherValsInput)
		{
			string teacherIds = "";
			string selectedTeacher = teacherValsInput.Value; 

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				teacherIds = user.UserID.ToString();
			}
			else
			{
				if (selectedTeacher == "-1" || selectedTeacher == "")
				{
					var teachers = GetTeacherByDistrictBuilding(user, selectedBuildings, selectedDistricts);
					teacherIds = string.Join(",", teachers.Select(p => p.UserID.ToString()));					
				}
				else
				{
					teacherIds = selectedTeacher;

					//fallback just in case
					if (teacherIds == "")
						teacherIds = teacherDD.Value;					
				}
			}

			return teacherIds.TrimEnd(',');
		}

		public static string GetProviderFilter(HtmlSelect providerDD, string selectedDistricts, HtmlInputHidden providerValsInput)
		{
			string providerIds = "";
			string selectedprovider = providerValsInput.Value;
			
			if (selectedprovider == "-1" || selectedprovider == "")
			{
				var providers = GetProviders(selectedDistricts);
				providerIds = string.Join(",", providers.Select(p => p.ProviderID.ToString()));				
			}
			else
			{
				providerIds = selectedprovider;

				//fallback just in case
				if (providerIds == "")
					providerIds = providerDD.Value;
				
			}			

			return providerIds.TrimEnd(',');
		}

		public static string GetServiceFilter(HtmlSelect ServiceType)
		{
			string serviceIds = ServiceType.Value;
			
			return serviceIds.TrimEnd(',');
			
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

			return districtList.TrimEnd(',');
		}
			   		 

		public static string GetBuildingFilter(HtmlSelect buildingDD, string username)
		{
			string buildingList = "";

			string buildingID = buildingDD.Value;

			if (buildingID == "-1")
			{
				var userBuildings = GetBuildings(username);
				buildingList = string.Join(",", userBuildings.Select(p => p.BuildingID.ToString()));				
			}
			else
			{
				buildingList += buildingID;
			}

			return buildingList.TrimEnd(',');
		}

			   		

	}
}