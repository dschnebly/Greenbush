using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProgressReport
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				GreenBushIEP.Report.ReportMaster.StatusList(this.statusDD);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD);
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD);
				GreenBushIEP.Report.ReportMaster.StudentList(this.studentDD);

				var sid =  Request.QueryString["sid"];
				
				if (!string.IsNullOrEmpty(sid))
				{
					int studentId = Convert.ToInt32(sid);

				}
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

			//string studentIds = "";
			string teacherIds = "";
			string providerIds = "";
			string providerNames = "";
			string buildingID = this.buildingDD.Value;			
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string status = this.statusDD.Value;
			bool cbPrintGoal = this.cbPrintGoal.Checked;
			bool cbPrintGoalBenchmarks = this.cbPrintGoalBenchmarks.Checked;
			string quarter = this.quarters.Value;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.districtDD, buildingID, districtID);

			string studentFilter = this.studentDD.Value == "-1" ? "" : studentDD.Value;

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

			var teacherList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
			

			foreach (var teacher in teacherList)
			{

				teacherIds += string.Format("{0},", teacher.UserID);				
			}

			DataTable dt = GetData(districtFilter, providerIds, buildingFilter, status, teacherIds, studentFilter);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			
			ReportParameter p1 = new ReportParameter("pPrintGoal", cbPrintGoal.ToString());
			ReportParameter p2 = new ReportParameter("pPrintGoalBenchmark", cbPrintGoalBenchmarks.ToString());
			ReportParameter p3 = new ReportParameter("pQuarter", quarter);


			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Progress/rptProgressReport.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string providerIds, string buildingID, string status, string teacherIds, string studentIds)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("begin_date", typeof(string));
			dt.Columns.Add("end_Date", typeof(string));
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			//dt.Columns.Add("ProviderName", typeof(string));
			dt.Columns.Add("Grade", typeof(string));
			dt.Columns.Add("goalID", typeof(int));
			dt.Columns.Add("GoalDescription", typeof(string));
			dt.Columns.Add("StateStandards", typeof(string));
			dt.Columns.Add("Baseline", typeof(string));
			dt.Columns.Add("goalBenchmarkID", typeof(int));
			dt.Columns.Add("ObjectiveBenchmark", typeof(string));
			dt.Columns.Add("Method", typeof(string));
			dt.Columns.Add("BencharkDateQ1", typeof(string));
			dt.Columns.Add("BencharkDateQ2", typeof(string));
			dt.Columns.Add("BencharkDateQ3", typeof(string));
			dt.Columns.Add("BencharkDateQ4", typeof(string));
			dt.Columns.Add("ProgressDateQ1", typeof(string));
			dt.Columns.Add("ProgressDateQ2", typeof(string));
			dt.Columns.Add("ProgressDateQ3", typeof(string));
			dt.Columns.Add("ProgressDateQ4", typeof(string));
			dt.Columns.Add("ProgressNotes1", typeof(string));
			dt.Columns.Add("ProgressNotes2", typeof(string));
			dt.Columns.Add("ProgressNotes3", typeof(string));
			dt.Columns.Add("ProgressNotes4", typeof(string));			
			dt.Columns.Add("Progress_Quarter1", typeof(string));
			dt.Columns.Add("Progress_Quarter2", typeof(string));
			dt.Columns.Add("Progress_Quarter3", typeof(string));
			dt.Columns.Add("Progress_Quarter4", typeof(string));
			dt.Columns.Add("StudentId", typeof(string));
			dt.Columns.Add("GoalModule", typeof(string));
			dt.Columns.Add("AnnualGoal", typeof(string));
			dt.Columns.Add("BenchmarkNotes1", typeof(string));
			dt.Columns.Add("BenchmarkNotes2", typeof(string));
			dt.Columns.Add("BenchmarkNotes3", typeof(string));
			dt.Columns.Add("BenchmarkNotes4", typeof(string));
			dt.Columns.Add("BenchmarkProgress_Quarter1", typeof(string));
			dt.Columns.Add("BenchmarkProgress_Quarter2", typeof(string));
			dt.Columns.Add("BenchmarkProgress_Quarter3", typeof(string));
			dt.Columns.Add("BenchmarkProgress_Quarter4", typeof(string));


			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProgress(districtFilter, status, buildingID, providerIds, teacherIds, studentIds);

				foreach (var cs in list)
					dt.Rows.Add(cs.begin_date, cs.end_Date, cs.StudentFirstName, cs.StudentLastName
						, cs.Grade, cs.goalID, cs.GoalDescription, cs.StateStandards, cs.Baseline
						, cs.goalBenchmarkID, cs.ObjectiveBenchmark, cs.Method
						, cs.BencharkDateQ1, cs.BencharkDateQ2, cs.BencharkDateQ3, cs.BencharkDateQ4
						, cs.ProgressDateQ1, cs.ProgressDateQ2, cs.ProgressDateQ3, cs.ProgressDateQ4
						, cs.ProgressNotes1, cs.ProgressNotes2, cs.ProgressNotes3, cs.ProgressNotes4
						, cs.Progress_Quarter1, cs.Progress_Quarter2, cs.Progress_Quarter3, cs.Progress_Quarter4
						, cs.StudentId, cs.GoalModule, cs.AnnualGoal
						, cs.BenchmarkNotes1, cs.BenchmarkNotes2, cs.BenchmarkNotes3, cs.BenchmarkNotes4
						, cs.BenchmarkProgress_Quarter1, cs.BenchmarkProgress_Quarter2, cs.BenchmarkProgress_Quarter3, cs.BenchmarkProgress_Quarter4
						);
			}

			return dt;
		}
	}
}


