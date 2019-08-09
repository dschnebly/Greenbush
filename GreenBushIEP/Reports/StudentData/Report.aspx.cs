using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.Owner
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
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
			
			string buildingID = this.buildingDD.Value;
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.districtDD, buildingID, districtID);


			DateTime? startDate = null;
			DateTime? endDate = null;

			if (!string.IsNullOrEmpty(this.startDate.Value))
				startDate = DateTime.Parse(this.startDate.Value);
			
			if (!string.IsNullOrEmpty(this.endDate.Value))
				endDate = DateTime.Parse(this.endDate.Value);

			DataTable dt = GetData(districtFilter, buildingFilter, startDate, endDate);
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
			ReportParameter p5 = new ReportParameter("Building", buildingName);
			ReportParameter p6 = new ReportParameter("District", districtName);

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/StudentData/rptStudents.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);

			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p5, p6 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtIds, string buildingID, DateTime? startDate, DateTime? endDate)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("DateCreated", typeof(DateTime));
			dt.Columns.Add("BuildingID", typeof(string));
			dt.Columns.Add("UserID", typeof(int));
			dt.Columns.Add("BuildingName", typeof(string));
			dt.Columns.Add("USD", typeof(string));
			dt.Columns.Add("DistrictName", typeof(string));
			dt.Columns.Add("ContactName", typeof(string));
			dt.Columns.Add("Email", typeof(string));
			dt.Columns.Add("AddressLine", typeof(string));
			dt.Columns.Add("City", typeof(string));
			dt.Columns.Add("StudentState", typeof(string));
			dt.Columns.Add("Zip", typeof(string));


			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportStudentsByBuilding(districtIds, buildingID, startDate, endDate);

				foreach (var cs in list)
					dt.Rows.Add(cs.StudentFirstName, cs.StudentLastName, cs.DateCreated, cs.BuildingID
						, cs.UserID, cs.BuildingName, cs.USD, cs.DistrictName, cs.ContactName, cs.Email, cs.AddressLine, 
						cs.City, cs.StudentState, cs.Zip);
			}

			return dt;
		}
	}
}
