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

namespace GreenBushIEP.Reports.ESY
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
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
				


			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);				
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
						
			string buildingName = this.buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;
			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);
			string teacherIds = "";
			
			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				teacherIds = user.UserID.ToString();
			}

			if (string.IsNullOrEmpty(providerIds) && this.buildingDD.Value == "-1")
			{
				providerIds = GreenBushIEP.Report.ReportMaster.GetProviderFilter(this.providerDD, districtFilter, this.providerVals);
			}

			DateTime? startDate = null;
			if(this.startDate.Value != "")
				startDate= DateTime.Parse(this.startDate.Value);

			DateTime? endDate = null;
			if (this.endDate.Value != "")
				endDate = DateTime.Parse(this.endDate.Value);
			
			DataTable dt = GetData(districtFilter, providerIds, teacherIds, buildingFilter, startDate, endDate);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			ReportParameter p3 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ESY/rptESY.rdlc");
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] {  p3 });
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string providerIds, string teacherIds, string buildingFilter, DateTime? startDate, DateTime? endDate)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportESY", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingFilter;
					cmd.Parameters.Add("@ProviderId", SqlDbType.VarChar, 8000).Value = providerIds;
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;
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