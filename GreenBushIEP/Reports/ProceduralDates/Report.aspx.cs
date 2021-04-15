using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProceduralDates
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.User.Identity.IsAuthenticated)
			{
				Response.Redirect("~/Account/Login");
			}

			if (!IsPostBack)
			{				
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.TeacherList(this.teacherDD, this.districtDD.Value, this.buildingDD.Value, this.teacherVals);				
			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.TeacherList(this.teacherDD, this.districtDD.Value, this.buildingDD.Value, this.teacherVals);
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
			
			string teacherNames = "";

			//string buildingID = this.buildingDD.Value;			
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);

			string teacherIds = GreenBushIEP.Report.ReportMaster.GetTeacherFilter(this.teacherDD, user, buildingFilter, districtFilter, this.teacherVals);

			//if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			//{
			//	teacherIds = user.UserID.ToString();
			//}
			//else
			//{
			//	foreach (ListItem li in teacherDD.Items)
			//	{
			//		if (li.Selected)
			//		{
			//			teacherIds += string.Format("{0},", li.Value);
			//		}
			//	}
			//}

			DataTable dt = GetData(districtFilter, teacherIds, buildingFilter);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			ReportDataSource rds2 = null;
			if (this.buildingDD.Value != "-1")
			{
				DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingFilter);
				rds2 = new ReportDataSource("DataSet2", dt2);
			}
			else
			{
				DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData("-1");
				rds2 = new ReportDataSource("DataSet2", dt2);
			}
			ReportParameter p1 = new ReportParameter("TeacherNames", teacherNames.Trim().Trim(','));
			ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			ReportParameter p3 = new ReportParameter("Building", buildingName);
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProceduralDates/rptProceduralDates.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string teacherIds, string buildingID)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("DateType", typeof(string));
			dt.Columns.Add("EvalDate", typeof(string));
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("Teachers", typeof(string));			
			dt.Columns.Add("USD", typeof(string));
			dt.Columns.Add("BuildingName", typeof(string));

			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProceduralDates(districtFilter, teacherIds, buildingID, null, null);

				foreach (var cs in list)
					dt.Rows.Add(cs.DateType, cs.EvalDate, cs.StudentFirstName, cs.StudentLastName, cs.Teachers, cs.USD, cs.BuildingName);
			}

			return dt;
		}

	}
}