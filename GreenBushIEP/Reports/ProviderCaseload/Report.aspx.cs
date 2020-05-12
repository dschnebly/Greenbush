using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
			if (!Page.User.Identity.IsAuthenticated)
			{
				Response.Redirect("~/Account/Login");
			}

			if (!IsPostBack)
			{				
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.TeacherList(this.teacherDD, this.districtDD.Value, this.buildingDD.Value, this.teacherVals);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);				
				GreenBushIEP.Report.ReportMaster.StudentList(this.studentDD, this.districtDD.Value, this.buildingDD.Value, this.teacherDD.Value, studentVals);
				
				
			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
				GreenBushIEP.Report.ReportMaster.TeacherList(this.teacherDD, this.districtDD.Value, this.buildingDD.Value, this.teacherVals);
				GreenBushIEP.Report.ReportMaster.StudentList(this.studentDD, this.districtDD.Value, this.buildingDD.Value, this.teacherDD.Value, studentVals);
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
			//string buildingID = this.buildingDD.Value;
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			
			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);

			string teacherIds = GreenBushIEP.Report.ReportMaster.GetTeacherFilter(this.teacherDD, user, buildingFilter, districtFilter, this.teacherVals);

			string studentFilter = this.studentVals.Value == "-1" ? "" : studentVals.Value;


			foreach (ListItem li in fiscalYear.Items)
			{
				if (li.Selected)
				{
					fiscalYearsNames += string.Format("{0},", li.Text);
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
				providerIds = GreenBushIEP.Report.ReportMaster.GetProviderFilter(this.providerDD, districtFilter, this.providerVals);				
			}												

			DataTable dt = GetData(districtFilter, providerIds, fiscalYears, teacherIds, buildingFilter, studentFilter);
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
			ReportParameter p1 = new ReportParameter("ProviderNames", providerNames.Trim().Trim(','));
			ReportParameter p2 = new ReportParameter("FiscalYears", fiscalYearsNames.Trim().Trim(','));
			ReportParameter p3 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			ReportParameter p4 = new ReportParameter("Building", buildingName);
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProviderCaseload/rptProviderCaseload.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string providerIds, string fiscalYear, string teacherIds, string buildingFilter, string studentIds)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportProviderCaseload", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingFilter;
					cmd.Parameters.Add("@ProviderId", SqlDbType.VarChar, 8000).Value = providerIds;
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;
					cmd.Parameters.Add("@FiscalYear", SqlDbType.VarChar, 8000).Value = fiscalYear;
					cmd.Parameters.Add("@StudentId", SqlDbType.VarChar, 8000).Value = studentIds;

					using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
					{
						sda.Fill(ds);
					}
				}
			}

			return ds.Tables[0];

		}

		//private DataTable GetData(string districtFilter, string providerId, string fiscalYear, string teacher, string buildingID, string studentIds)
		//{
		//	DataTable dt = new DataTable();
		//	dt.Columns.Add("LastName", typeof(string));
		//	dt.Columns.Add("FirstName", typeof(string));
		//	dt.Columns.Add("ProviderName", typeof(string));
		//	dt.Columns.Add("GoalTitle", typeof(string));
		//	dt.Columns.Add("Location", typeof(string));
		//	dt.Columns.Add("Minutes", typeof(string));
		//	dt.Columns.Add("DaysPerWeek", typeof(string));
		//	dt.Columns.Add("Frequency", typeof(string));
		//	dt.Columns.Add("ServiceType", typeof(string));
		//	dt.Columns.Add("USD", typeof(string));
		//	dt.Columns.Add("BuildingName", typeof(string));
		//	dt.Columns.Add("FrequencyDesc", typeof(string));
		//	using (var ctx = new IndividualizedEducationProgramEntities())
		//	{
		//		//Execute stored procedure as a function
		//		var list = ctx.up_ReportProviderCaseload(districtFilter,providerId, fiscalYear, teacher, buildingID, studentIds);

		//		foreach (var cs in list)
		//			dt.Rows.Add(cs.LastName, cs.FirstName, cs.ProviderName, cs.GoalTitle
		//				,cs.Location, cs.Minutes, cs.DaysPerWeek, cs.Frequency, cs.ServiceType
		//				,cs.USD, cs.BuildingName, cs.FrequencyDesc);
		//	}

		//	return dt;
		//}

		
	}
}