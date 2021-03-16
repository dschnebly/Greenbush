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

namespace GreenBushIEP.Reports.HealthPlan
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

			//string teacherNames = "";

			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);
			string teacherIds = GreenBushIEP.Report.ReportMaster.GetTeacherFilter(this.teacherDD, user, buildingFilter, districtFilter, this.teacherVals);
			string fiscalYears = "";
			
			foreach (ListItem li in fiscalYear.Items)
			{
				if (li.Selected)
				{
					fiscalYears += string.Format("{0},", li.Value);
				}
			}

			DataTable dt = GetData(districtFilter, teacherIds, buildingFilter, fiscalYears);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);					
			ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
		
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/HealthPlan/rptHealthPlan.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p2 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string teacherIds, string buildingFilter, string fiscalYear)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportHealthPlan", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingFilter;					
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;
					cmd.Parameters.Add("@FiscalYear", SqlDbType.VarChar, 8000).Value = fiscalYear;

					using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
					{
						sda.Fill(ds);
					}
				}
			}

			return ds.Tables[0];

		}


	}
}