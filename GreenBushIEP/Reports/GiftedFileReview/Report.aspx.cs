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

namespace GreenBushIEP.Reports.GiftedFileReview
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
			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string teacherIds = "-1";

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
			{
				teacherIds = user.UserID.ToString();
			}

			DateTime? startDate = null;
			DateTime? endDate = null;

			if (!string.IsNullOrEmpty(this.startDate.Value))
				startDate = DateTime.Parse(this.startDate.Value);

			if (!string.IsNullOrEmpty(this.endDate.Value))
				endDate = DateTime.Parse(this.endDate.Value);

			DataTable dt = GetData(districtFilter, teacherIds, startDate, endDate);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);		
				
		
			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/GiftedFileReview/rptGiftedFileReview.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
		
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string teacherIds, DateTime? startDate, DateTime? endDate)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportGiftedFileReview", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;					
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