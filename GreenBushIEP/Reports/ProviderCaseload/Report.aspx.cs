using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProviderCaseload
{
	public partial class Report1 : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				
				var providerList = GreenBushIEP.Report.ReportMaster.GetProviders(User.Identity.Name);
				this.providerDD.DataSource = providerList;
				this.providerDD.DataTextField = "Name";
				this.providerDD.DataValueField = "ProviderID";
				this.providerDD.DataBind();

				var buildingList = GreenBushIEP.Report.ReportMaster.GetBuildings(User.Identity.Name);
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
			var user = GreenBushIEP.Report.ReportMaster.GetUser(User.Identity.Name);
			string providerIds = "";
			string providerNames = "";
			string fiscalYears = "";
			string fiscalYearsNames = "";
			string buildingID = this.buildingDD.Value;
			string teacher = "";

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


			if (string.IsNullOrEmpty(providerIds))
			{
				//get all, but limit list
				var providerList = GreenBushIEP.Report.ReportMaster.GetProviders(User.Identity.Name);
				providerIds = string.Join(",", providerList.Select(o => o.ProviderID));
			}

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				teacher = user.UserID.ToString();
			}

			DataTable dt = GetData(providerIds, fiscalYears, teacher, buildingID);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingID);
			ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2);
			ReportParameter p1 = new ReportParameter("ProviderNames", providerNames.Trim().Trim(','));
			ReportParameter p2 = new ReportParameter("FiscalYears", fiscalYearsNames.Trim().Trim(','));
			ReportParameter p3 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProviderCaseload/rptProviderCaseload.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string providerId, string fiscalYear, string teacher, string buildingID)
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
				var list = ctx.up_ReportProviderCaseload(providerId, fiscalYear, teacher, buildingID);

				foreach (var cs in list)
					dt.Rows.Add(cs.LastName, cs.FirstName, cs.ProviderName, cs.GoalTitle, cs.Location, cs.Minutes, cs.DaysPerWeek, cs.Frequency, cs.ServiceType);
			}

			return dt;
		}

		
	}
}