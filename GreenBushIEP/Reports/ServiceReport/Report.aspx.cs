using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ServiceReport
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				GreenBushIEP.Report.ReportMaster.ServiceList(this.ServiceType);
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD);
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
			string serviceIds = GreenBushIEP.Report.ReportMaster.GetServiceFilter(this.ServiceType);

			string buildingID = this.buildingDD.Value;
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.districtDD, buildingID, districtID);
			
			DateTime startDate = DateTime.Parse(this.startDate.Value);
			DateTime endDate = DateTime.Parse(this.endDate.Value);

			serviceIds = serviceIds.Trim().Trim(',');
			DataTable dt = GetData(districtFilter, serviceIds, buildingFilter, startDate, endDate);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			ReportDataSource rds2 = null;
			if (this.buildingDD.Value != "-1")
			{
				DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingID);
				rds2 = new ReportDataSource("DataSet2", dt2);
			}
			else
			{
				DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData("-1");
				rds2 = new ReportDataSource("DataSet2", dt2);
			}
			ReportParameter p1 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			ReportParameter p2 = new ReportParameter("StartDate", this.startDate.Value);
			ReportParameter p3 = new ReportParameter("EndDate", this.endDate.Value);
			ReportParameter p4 = new ReportParameter("ServiceCode", serviceIds);
			ReportParameter p5 = new ReportParameter("Building", buildingName);
			ReportParameter p6 = new ReportParameter("District", districtName);
			
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ServiceReport/rptServices.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5, p6 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string serviceIds, string buildingID, DateTime startDate, DateTime endDate)
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
			dt.Columns.Add("USD", typeof(string));
			dt.Columns.Add("BuildingName", typeof(string));
			dt.Columns.Add("FrequencyDesc", typeof(string));

			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportServices(districtFilter, serviceIds, buildingID, startDate, endDate);

				foreach (var cs in list)
					dt.Rows.Add(cs.StudentFirstName, cs.StudentLastName, cs.ServiceType, cs.Provider
						, cs.Frequency, cs.Location, cs.DaysPerWeek, cs.Minutes, cs.USD, cs.BuildingName
						, cs.FrequencyDesc);
			}

			return dt;
		}
	}
}