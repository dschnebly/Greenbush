using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.Owner
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
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD);
				GreenBushIEP.Report.ReportMaster.StudentStatusList(this.statusDD);
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

			string statusCodes = "";

			if (statusDD.Value != "All")
			{
				foreach (ListItem li in statusDD.Items)
				{
					if (li.Selected)
					{
						statusCodes += string.Format("{0},", li.Text);
					}
				}
			}
			
			DateTime? startDate = null;
			DateTime? endDate = null;

			if (!string.IsNullOrEmpty(this.startDate.Value))
				startDate = DateTime.Parse(this.startDate.Value);
			
			if (!string.IsNullOrEmpty(this.endDate.Value))
				endDate = DateTime.Parse(this.endDate.Value);

			DataTable dt = GetData(districtFilter, buildingFilter, startDate, endDate, statusCodes);
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

		private DataTable GetData(string districtIds, string buildingID, DateTime? startDate, DateTime? endDate, string statusCodes)
		{
			DataSet ds = new DataSet();


			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportStudentsByBuilding", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					
					cmd.Parameters.Add("@Usd", SqlDbType.VarChar, 8000).Value = districtIds;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingID;
					cmd.Parameters.Add("@ReportStartDate", SqlDbType.DateTime).Value = startDate;
					cmd.Parameters.Add("@ReportEndDate", SqlDbType.DateTime).Value = endDate;					
					cmd.Parameters.Add("@StatusCode", SqlDbType.VarChar, 8000).Value = statusCodes;


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
