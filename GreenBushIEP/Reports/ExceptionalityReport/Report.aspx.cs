using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ExceptionalityReport
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{

				var services = GreenBushIEP.Report.ReportMaster.GetServices();
				this.ServiceType.DataSource = services;
				this.ServiceType.DataTextField = "Name";
				this.ServiceType.DataValueField = "ServiceCode";
				this.ServiceType.DataBind();

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
			string serviceIds = "";			
			string buildingID = this.buildingDD.Value;
			
			foreach (ListItem li in ServiceType.Items)
			{
				if (li.Selected)
				{
					serviceIds += string.Format("{0},", li.Value);
				}
			}

			DateTime startDate = DateTime.Parse(this.startDate.Value);
			DateTime endDate = DateTime.Parse(this.endDate.Value);

			serviceIds = serviceIds.Trim().Trim(',');
			DataTable dt = GetData(serviceIds, buildingID, startDate, endDate);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingID);
			ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2);
			ReportParameter p1 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			ReportParameter p2 = new ReportParameter("StartDate", this.startDate.Value);
			ReportParameter p3 = new ReportParameter("EndDate", this.endDate.Value);
			ReportParameter p4 = new ReportParameter("ServiceCode", serviceIds);

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ExceptionalityReport/rptExceptionalityReport.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string serviceIds, string buildingID, DateTime startDate, DateTime endDate)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("ServiceType", typeof(string));
			dt.Columns.Add("Provider", typeof(string));			
			dt.Columns.Add("Frequency", typeof(int));
			dt.Columns.Add("Location", typeof(string));
			dt.Columns.Add("DaysPerWeek", typeof(byte));
			dt.Columns.Add("Minutes", typeof(short));
			dt.Columns.Add("DateOfBirth", typeof(DateTime));
			dt.Columns.Add("PrimaryExceptionality", typeof(string));
			dt.Columns.Add("SecondaryExceptionality", typeof(string));

			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportServices(serviceIds, buildingID, startDate, endDate);

				foreach (var cs in list)
					dt.Rows.Add(cs.StudentFirstName, cs.StudentLastName, cs.ServiceType, cs.Provider,
						cs.Frequency, cs.Location, cs.DaysPerWeek, cs.Minutes,
						cs.DateOfBirth, cs.PrimaryExceptionality, cs.SecondaryExceptionality);
			}

			return dt;
		}
	}
}