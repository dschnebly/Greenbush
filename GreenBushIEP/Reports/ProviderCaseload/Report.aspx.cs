using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Data.SqlClient;

namespace GreenBushIEP.Reports.ProviderCaseload
{
	public partial class Report : System.Web.UI.Page
	{
		private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();
		
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{

				var providerList = GetProviders(0);
				this.providerDD.DataSource = providerList;
				this.providerDD.DataTextField = "Name";
				this.providerDD.DataValueField = "ProviderID";
				this.providerDD.DataBind();

				var buildingList = GetBuildings();
				this.buildingDD.DataSource = buildingList;
				this.buildingDD.DataTextField = "BuildingName";
				this.buildingDD.DataValueField = "BuildingID";
				this.buildingDD.DataBind();
				
			}
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			ShowReport();
		}

		private void ShowReport()
		{
			ReportViewer MReportViewer = this.Master.FindControl("ReportViewer1") as ReportViewer;
			MReportViewer.Reset();

			string providerIds = "";
			string providerNames = "";
			string fiscalYears = "";
			string fiscalYearsNames = "";
			string buildingID = this.buildingDD.Value;


			foreach (ListItem li in providerDD.Items)
			{
				if (li.Selected)
				{
					providerNames += string.Format("{0}, ", li.Text);
				}
			}

			foreach (ListItem li in providerDD.Items)
			{
				if (li.Selected)
				{
					providerIds += string.Format("{0},", li.Value);
				}
			}

			foreach (ListItem li in fiscalYear.Items)
			{
				if (li.Selected)
				{
					fiscalYearsNames += string.Format("{0}, ", li.Text);
				}
			}

			foreach (ListItem li in fiscalYear.Items)
			{
				if (li.Selected)
				{
					fiscalYears += string.Format("{0},", li.Value);
				}
			}

			DataTable dt = GetData(providerIds, fiscalYears);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			DataTable dt2 = GetBuildingData(buildingID);
			ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2);
			ReportParameter p1 = new ReportParameter("ProviderNames", providerNames.Trim().Trim(','));
			ReportParameter p2 = new ReportParameter("FiscalYears", fiscalYearsNames.Trim().Trim(','));
			ReportParameter p3 = new ReportParameter("PrintedBy", CurrentUser());
			
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProviderCaseload/rptProviderCaseload.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private string CurrentUser()
		{
			var user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
			return string.Format("{0} {1}", user.FirstName, user.LastName);
		}

		private DataTable GetData(string providerId, string fiscalYear)
		{			
			DataTable dt = new DataTable();
			dt.Columns.Add("LastName", typeof(string));
			dt.Columns.Add("FirstName", typeof(string));
			dt.Columns.Add("ProviderName", typeof(string));
			dt.Columns.Add("GoalTitle", typeof(string));
			dt.Columns.Add("Location", typeof(string));
			dt.Columns.Add("Minutes", typeof(string));
			dt.Columns.Add("DaysPerWeek", typeof(string));
			dt.Columns.Add("Frequency", typeof(string));
			dt.Columns.Add("ServiceType", typeof(string));
			
			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProviderCaseload(providerId, fiscalYear);

				foreach (var cs in list)
					dt.Rows.Add(cs.LastName, cs.FirstName, cs.ProviderName, cs.GoalTitle, cs.Location, cs.Minutes, cs.DaysPerWeek, cs.Frequency, cs.ServiceType);
			}
			
			return dt;
		}

		private DataTable GetBuildingData(string id)
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

		private List<tblProvider> GetProviders(int? providerUSD)
		{
		     var providers = (from p in db.tblProviders
							 join d in db.tblProviderDistricts on p.ProviderID equals d.ProviderID
							 //where d.USD == studentInfo.AssignedUSD
							 select p).ToList();

			return providers;
		}

		private List<tblBuilding> GetBuildings()
		{
			tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

			var buildingList = (from buildingMaps in db.tblBuildingMappings
								   join buildings in db.tblBuildings
									  on buildingMaps.BuildingID equals buildings.BuildingID
								   where buildingMaps.UserID == user.UserID
								   select buildings).Distinct().OrderBy(b => b.BuildingID).ToList();

			return buildingList;
		}

		private tblBuilding GetBuilding(string id)
		{	

			var building = (from buildingMaps in db.tblBuildingMappings
								join buildings in db.tblBuildings
								   on buildingMaps.BuildingID equals buildings.BuildingID
								where buildings.BuildingID == id && buildings.Active == 1
								select buildings).Distinct().OrderBy(b => b.BuildingID).FirstOrDefault();

			return building;
		}
	}
}