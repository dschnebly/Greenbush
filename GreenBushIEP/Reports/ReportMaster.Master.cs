using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Data;

namespace GreenBushIEP.Report
{	


	public partial class ReportMaster : System.Web.UI.MasterPage
	{
		public const string owner = "1";
		public const string mis = "2";
		public const string admin = "3";
		public const string teacher = "4";
		public const string student = "5";
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

				//teachers = (from org in db.tblOrganizationMappings
				//			join user in db.tblUsers
				//				on org.UserID equals user.UserID
				//			where (org.AdminID == usr.UserID) && !(user.Archive ?? false)
				//			select user).Distinct().AsEnumerable();

				teachers = GetTeacherRecursive(null, usr.UserID);
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
			var teachers = (from org in db.tblOrganizationMappings
							join user in db.tblUsers
								on org.UserID equals user.UserID
							where (org.AdminID == userId) && !(user.Archive ?? false) && (user.RoleID == teacher || user.RoleID == admin)
							select user).Distinct().ToList();

			list.AddRange(teachers.Where(i => i.RoleID == teacher));
			foreach (tblUser teach in teachers)
			{
				var childList = GetTeacherRecursive(teachers, teach.UserID);
				list.AddRange(childList);
				
			}

			return list;

			
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
			return db.tblServiceTypes.ToList();
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

		public static List<BuildingsViewModel> GetBuildings(string userName)
		{
			tblUser user = GreenBushIEP.Report.ReportMaster.db.tblUsers.SingleOrDefault(o => o.Email == userName);


			var buildingList = (from bm in db.tblBuildingMappings
							   join b in db.tblBuildings on bm.USD equals b.USD
							   where bm.UserID == user.UserID && b.Active == 1 && bm.BuildingID == b.BuildingID
							   select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();



			//var buildingList = (from buildingMaps in db.tblBuildingMappings
			//					join buildings in db.tblBuildings
			//					   on buildingMaps.BuildingID equals buildings.BuildingID
			//					where buildingMaps.UserID == user.UserID
			//					select new BuildingView { BuildingID = buildings.BuildingID, BuildingName = buildings.BuildingName }).Distinct().OrderBy(b => b.BuildingID).ToList();

			return buildingList;
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