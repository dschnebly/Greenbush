using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.StudentInfo
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
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD);
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
			foreach (ListItem li in providerDD.Items)
			{
				if (li.Selected)
				{
					providerIds += string.Format("{0},", li.Value);
				}
			}

			string buildingID = this.buildingDD.Value;
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.districtDD, buildingID, districtID);
			
			DataTable dt = GetData(districtFilter, buildingFilter, providerIds);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
						
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/StudentInfo/rptStudentInfo.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtIds, string buildingID, string providerIds)
		{
			DataSet ds = new DataSet();


			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportStudentInfo", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtIds;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingID;
					cmd.Parameters.Add("@ProviderId", SqlDbType.VarChar, 8000).Value = providerIds;

					using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
					{					
						
						sda.Fill(ds);
					}
				}
			}


			if (!string.IsNullOrEmpty(providerIds))
			{
			}

			return ds.Tables[0];

		}
	}
}