using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Linq;

namespace GreenBushIEP.Reports.ServiceReport
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
				GreenBushIEP.Report.ReportMaster.ServiceList(this.ServiceType);
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
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

			string teacherIds = "-1";
			string providerIds = "0";

			string serviceIds = GreenBushIEP.Report.ReportMaster.GetServiceFilter(this.ServiceType);

			string buildingID = this.buildingDD.Value;
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);

			DateTime startDate = DateTime.Parse(this.startDate.Value);
			DateTime endDate = DateTime.Parse(this.endDate.Value);

			serviceIds = serviceIds.Trim().Trim(',');

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				teacherIds = user.UserID.ToString();
			}

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{				
				var providerObj = GreenBushIEP.Report.ReportMaster.GetProviderByProviderCode(user.TeacherID);
				if (providerObj != null)
				{
					providerIds = providerObj.ProviderID.ToString();
				}
			}
			else
			{

				var providers = GreenBushIEP.Report.ReportMaster.GetProviders(districtFilter);
				if (providers != null)
				{
					providerIds = string.Join(",", providers.Select(p => p.ProviderID.ToString()));
				}
			}
			
			DataTable dt = GetData(districtFilter, serviceIds, buildingFilter, startDate, endDate, teacherIds, providerIds);
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

		private DataTable GetData(string districtFilter, string serviceIds, string buildingID,
			DateTime startDate, DateTime endDate, string teacherIds, string providerIds)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportServices", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingID;
					cmd.Parameters.Add("@ProviderId", SqlDbType.VarChar, 8000).Value = providerIds;
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;
					cmd.Parameters.Add("@ServiceId", SqlDbType.VarChar, 8000).Value = serviceIds;
					cmd.Parameters.Add("@ReportStartDate", SqlDbType.DateTime).Value = startDate;
					cmd.Parameters.Add("@ReportEndDate", SqlDbType.DateTime).Value = endDate;
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