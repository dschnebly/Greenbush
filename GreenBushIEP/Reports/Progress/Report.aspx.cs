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

namespace GreenBushIEP.Reports.ProgressReport
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
				GreenBushIEP.Report.ReportMaster.StatusList(this.statusDD);				
				GreenBushIEP.Report.ReportMaster.DistrictList(this.districtDD);
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
				GreenBushIEP.Report.ReportMaster.StudentListByProvider(this.studentDD, this.districtDD.Value, this.buildingDD.Value, this.providerVals.Value, this.studentVals);
				
				var sid =  Request.QueryString["sid"];
				
				if (!string.IsNullOrEmpty(sid))
				{
					int studentId = Convert.ToInt32(sid);

				}
			}
			else
			{
				GreenBushIEP.Report.ReportMaster.BuildingList(this.buildingDD, this.districtDD.Value);
				GreenBushIEP.Report.ReportMaster.ProviderList(this.providerDD, this.districtDD.Value, this.providerVals);
				GreenBushIEP.Report.ReportMaster.StudentListByProvider(this.studentDD, this.districtDD.Value, this.buildingDD.Value, this.providerVals.Value, this.studentVals);
				
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
			string teacherIds = "-1";

			string status = this.statusDD.Value;

			bool cbPrintGoal = this.cbPrintGoal.Checked;
			bool cbPrintGoalBenchmarks = this.cbPrintGoalBenchmarks.Checked;
			string quarter = this.quarters.Value;

			string districtID = this.districtDD.Value;
			string districtName = this.districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

			string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(this.districtDD, districtID);
			string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(this.buildingDD, User.Identity.Name);			
			string studentFilter = this.studentVals.Value == "-1" ? "" : studentVals.Value;

            providerIds = this.providerVals.Value;			

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				teacherIds = user.UserID.ToString();

				if(!string.IsNullOrEmpty(user.TeacherID))
                {
					var providerObj = GreenBushIEP.Report.ReportMaster.GetProviderByProviderCode(user.TeacherID);
					providerIds = providerObj != null ? providerObj.ProviderID.ToString() : "";
				}
			}			

			DataTable dt = GetData(districtFilter, providerIds, buildingFilter, status, teacherIds, studentFilter);
			
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);		
			
			ReportParameter p1 = new ReportParameter("pPrintGoal", cbPrintGoal.ToString());
			ReportParameter p2 = new ReportParameter("pPrintGoalBenchmark", cbPrintGoalBenchmarks.ToString());
			ReportParameter p3 = new ReportParameter("pQuarter", quarter);
			ReportParameter p4 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Progress/rptProgressReport.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string districtFilter, string providerIds, string buildingID, string status, string teacherIds, string studentIds)
		{
			DataSet ds = new DataSet();

			using (var context = new IndividualizedEducationProgramEntities())
			{
				string connStr = context.Database.Connection.ConnectionString.ToString();
				using (SqlConnection conn = new SqlConnection(connStr))
				using (SqlCommand cmd = new SqlCommand("up_ReportProgress", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("@DistrictId", SqlDbType.VarChar, 8000).Value = districtFilter;
					cmd.Parameters.Add("@Status", SqlDbType.VarChar, 8000).Value = status;
					cmd.Parameters.Add("@BuildingId", SqlDbType.VarChar, 8000).Value = buildingID;
					cmd.Parameters.Add("@ProviderId", SqlDbType.VarChar, 8000).Value = providerIds;					
					cmd.Parameters.Add("@TeacherId", SqlDbType.VarChar, 8000).Value = teacherIds;
					cmd.Parameters.Add("@StudentId", SqlDbType.VarChar, 8000).Value = studentIds;

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


