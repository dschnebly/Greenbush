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
				var statusList = GreenBushIEP.Report.ReportMaster.GetIEPStatuses();
				this.statusDD.DataSource = statusList;
				this.statusDD.DataTextField = "Text";
				this.statusDD.DataValueField = "Value";
				this.statusDD.DataBind();


				var providers = GreenBushIEP.Report.ReportMaster.GetProviders(User.Identity.Name);
				this.providerDD.DataSource = providers;
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
			string buildingID = this.buildingDD.Value;			
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string status = this.statusDD.Value;
			bool cbPrintGoal = this.cbPrintGoal.Checked;
			bool cbPrintGoalBenchmarks = this.cbPrintGoalBenchmarks.Checked;
			string quarter = this.quarters.Value;

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
						

			if (buildingID == "-1")
			{
				buildingID = "";

				foreach (ListItem buildingItem in buildingDD.Items)
				{
					buildingID += string.Format("{0},", buildingItem.Value);
				}
			}


			DataTable dt = GetData(providerIds, buildingID, status);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			
			ReportParameter p1 = new ReportParameter("pPrintGoal", cbPrintGoal.ToString());
			ReportParameter p2 = new ReportParameter("pPrintGoalBenchmark", cbPrintGoalBenchmarks.ToString());
			ReportParameter p3 = new ReportParameter("pQuarter", quarter);


			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Progress/rptProgressReport.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string providerIds, string buildingID, string status)
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


			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProgress(status, buildingID, providerIds);

				foreach (var cs in list)
					dt.Rows.Add(cs.begin_date, cs.end_Date, cs.StudentFirstName, cs.StudentLastName
						, cs.Grade, cs.goalID, cs.GoalDescription, cs.StateStandards, cs.Baseline
						, cs.goalBenchmarkID, cs.ObjectiveBenchmark, cs.Method
						, cs.BencharkDateQ1, cs.BencharkDateQ2, cs.BencharkDateQ3, cs.BencharkDateQ4
						, cs.ProgressDateQ1, cs.ProgressDateQ3, cs.ProgressDateQ3, cs.ProgressDateQ4
						, cs.ProgressNotes1, cs.ProgressNotes2, cs.ProgressNotes3, cs.ProgressNotes4
						, cs.Progress_Quarter1, cs.Progress_Quarter2, cs.Progress_Quarter3, cs.Progress_Quarter4
						, cs.StudentId);
			}

			return dt;
		}
	}
}


