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

namespace GreenBushIEP.Report
{	


	public partial class ReportMaster : System.Web.UI.MasterPage
	{
		public const string owner = "1";
		public const string mis = "2";
		public const string admin = "3";
		public const string teacher = "4";
		public const string student = "5";
		public const string nurse = "6";
		public static IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

		protected void Page_Load(object sender, EventArgs e)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == HttpContext.Current.User.Identity.Name);
			if (user.RoleID == student)
			{	
				Server.Transfer("Error", true);
			}
			
		}

		public static List<TeacherView> GetTeachers(string userName)
		{
			List<tblUser> teachers = new List<tblUser>();
			List<TeacherView> teacherList = new List<TeacherView>();
 			tblUser usr = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);
			if (usr.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				//teacher, get providers by students?
				teachers = (from u in db.tblUsers
							where u.UserID == usr.UserID && !u.Archive.HasValue
							select u).Distinct().ToList();
			}
			else
			{
				teachers = GetTeacherRecursive(null, usr.UserID);

				//add person running report, if admin, can an admin have students?
				if((usr.RoleID == admin || usr.RoleID == mis) && !teachers.Contains(usr))
					teachers.Add(usr);
			}

			foreach (var item in teachers)
			{
				TeacherView tv = new TeacherView() { Name = string.Format("{0}, {1}", item.LastName, item.FirstName), UserID = item.UserID };
				teacherList.Add(tv);
			}

			return teacherList.OrderBy(o => o.Name).ToList();
		}

		private static List<tblUser> GetTeacherRecursive(List<tblUser> children, int userId)
		{
			List<tblUser> list = new List<tblUser>();

			try
			{
				var teachers = (from org in db.tblOrganizationMappings
								join user in db.tblUsers
									on org.UserID equals user.UserID
								where (org.AdminID == userId) && !(user.Archive ?? false) && (user.RoleID == teacher || user.RoleID == admin) && (user.UserID != userId)
								select user).Distinct().ToList();

				list.AddRange(teachers.Where(i => i.RoleID == teacher));
				foreach (tblUser teach in teachers)
				{
					var childList = GetTeacherRecursive(teachers, teach.UserID);
					list.AddRange(childList);

				}
			}
			catch (Exception)
			{
			}

			return list;

			
		}

		public static List<SelectListItem> GetIEPStatuses()
		{
			var statuses = new List<SelectListItem>();
			statuses.Add(new SelectListItem() { Value = IEPStatus.ACTIVE, Text = IEPStatus.ACTIVE });
			statuses.Add(new SelectListItem() { Value = IEPStatus.AMENDMENT, Text = IEPStatus.AMENDMENT });
			statuses.Add(new SelectListItem() { Value = IEPStatus.ANNUAL, Text = IEPStatus.ANNUAL });
			statuses.Add(new SelectListItem() { Value = IEPStatus.ARCHIVE, Text = IEPStatus.ARCHIVE });
			statuses.Add(new SelectListItem() { Value = IEPStatus.DELETED, Text = IEPStatus.DELETED });
			statuses.Add(new SelectListItem() { Value = IEPStatus.DRAFT, Text = IEPStatus.DRAFT });
			statuses.Add(new SelectListItem() { Value = IEPStatus.PLAN, Text = IEPStatus.PLAN });
			

			return statuses;


		}

		public static List<ProviderViewModel> GetProviders(string userName)
		{
			var providerList = new List<ProviderViewModel>();
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);
			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				//teacher, get providers by students?
				var providers = (from u in db.tblUsers
								 join o in db.tblOrganizationMappings on u.UserID equals o.UserID
								 join info in db.tblStudentInfoes on u.UserID equals info.UserID
								 join pd in db.tblProviderDistricts on info.USD equals pd.USD
								 join p in db.tblProviders on pd.ProviderID equals p.ProviderID
								 where o.AdminID == user.UserID && !u.Archive.HasValue
								 select p).Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();

				foreach (var provider in providers)
					providerList.Add(new ProviderViewModel() { Name = string.Format("{0}, {1}", provider.LastName, provider.FirstName), ProviderCode = provider.ProviderCode, ProviderID = provider.ProviderID});
			}
			else
			{
				var providers = db.tblProviders.Where(p => p.UserID == user.UserID).ToList();
				foreach (var provider in providers)
					providerList.Add(new ProviderViewModel() { Name = string.Format("{0}, {1}", provider.LastName, provider.FirstName), ProviderCode = provider.ProviderCode, ProviderID = provider.ProviderID });
							
			}

			return providerList;

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

			var districtList = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == user.UserID select district).Distinct().ToList();

			List<tblDistrict> districts = new List<tblDistrict>();

			districts.Add(new tblDistrict {  DistrictName = "All",  USD = "-1" });

			districts.AddRange(districtList);

			return districts;
		}

		public static List<BuildingsViewModel> GetBuildings(string userName)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			var buildings = new List<BuildingsViewModel>();

			var allOption = new BuildingsViewModel() { BuildingName = "All", BuildingID = "-1", BuildingUSD = "-1" };
			buildings.Add(allOption);

			var buildingList = (from bm in db.tblBuildingMappings
								join b in db.tblBuildings on bm.USD equals b.USD
								where b.Active == 1 && bm.BuildingID == b.BuildingID && bm.UserID == user.UserID
								select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

			buildings.AddRange(buildingList);
			
			return buildings;
		}

		public static List<BuildingsViewModel> GetBuildingsByDistrict(string userName, string usd)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);

			var buildings = new List<BuildingsViewModel>();

			var buildingList = (from bm in db.tblBuildingMappings
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

	}
}