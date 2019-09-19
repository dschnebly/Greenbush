using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProceduralDatesTracking
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD);
				GreenBushIEP.Report.ReportMaster.TeacherList(this.teacherDD);
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
			string teacherId = this.teacherDD.Value;			
			string buildingID = this.buildingDD.Value;
			string teacher = "";
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.districtDD, buildingID, districtID);
			string teacherIds = GreenBushIEP.Report.ReportMaster.GetTeacherFilter(this.teacherDD, teacherId);
			DateTime startDate = DateTime.Parse(this.startDate.Value);
			DateTime endDate = DateTime.Parse(this.endDate.Value);


			//foreach (ListItem li in teacherDD.Items)
			//{
			//	if (li.Selected)
			//	{
			//		teacherNames += string.Format("{0}, ", li.Text);
			//	}
			//}

			//foreach (ListItem li in teacherDD.Items)
			//{
			//	if (li.Selected)
			//	{
			//		teacherIds += string.Format("{0},", li.Value);
			//	}
			//}

			//if (string.IsNullOrEmpty(teacherIds))
			//{
			//	//get all, but limit list
			//	var providerList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
			//	teacherIds = string.Join(",", providerList.Select(o => o.UserID));
			//}

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				teacher = user.UserID.ToString();
			}


			DataTable dt = GetData(districtFilter, teacherIds, buildingFilter, startDate, endDate);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			
			ReportParameter p1 = new ReportParameter("TeacherNames", "");
			ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			ReportParameter p3 = new ReportParameter("Building", buildingName);
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProceduralDatesTracking/rptProceduralDatesTracking.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string teacherIds, string buildingID, DateTime startDate, DateTime endDate)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("ReEvalConsentSigned", typeof(DateTime));
			dt.Columns.Add("InitialEvalDetermination", typeof(DateTime));
			dt.Columns.Add("InitialEvalConsentSigned", typeof(DateTime));
			dt.Columns.Add("DaysSinceSigned", typeof(int));
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("StudentMiddleName", typeof(string));
			dt.Columns.Add("TeacherFirstName", typeof(string));
			dt.Columns.Add("TeacherLastName", typeof(string));
			dt.Columns.Add("TeacherID", typeof(string));
			dt.Columns.Add("USD", typeof(string));
			dt.Columns.Add("BuildingName", typeof(string));
			dt.Columns.Add("Teachers", typeof(string));



			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProceduralDatesTracking(districtFilter, teacherIds, buildingID, startDate, endDate);

				foreach (var cs in list)
					dt.Rows.Add(cs.ReEvalConsentSigned, cs.InitialEvalDetermination, cs.InitialEvalConsentSigned, cs.DaysSinceSigned
						, cs.StudentFirstName, cs.StudentLastName, cs.StudentMiddleName, cs.TeacherFirstName, cs.TeacherLastName
						, cs.TeacherID, cs.USD, cs.BuildingName, cs.Teachers);
			}

			return dt;
		}
	}
}