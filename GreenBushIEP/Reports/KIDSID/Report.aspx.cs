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

namespace GreenBushIEP.Reports.KIDSID
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
				//GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				//GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
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


			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);

			string teacherIds = "-1";

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				teacherIds = user.UserID.ToString();
			}

			DataTable dt = GetData(districtFilter, buildingFilter, teacherIds);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/KIDSID/rptKIDSID.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtIds, string buildingID, string teacherIds)
		{
			DataSet ds = new DataSet();


			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportKIDSID", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtIds;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingID;				
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;

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