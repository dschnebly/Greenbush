using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ExcessCostReport
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{	

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
			
			string buildingID = this.buildingDD.Value;
		
			
			DataTable dt = GetData(buildingID);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingID);
			ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2);
			ReportParameter p1 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
			

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ExcessCostReport/rptExcessCostReport.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string buildingID)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("DateOfBirth", typeof(DateTime));
			dt.Columns.Add("KIDSID", typeof(string));
			
			//using (var ctx = new IndividualizedEducationProgramEntities())
			//{
			//	//Execute stored procedure as a function
			//	var list = ctx.up_ReportExcessCost(buildingID);

			//	foreach (var cs in list)
			//		dt.Rows.Add(cs.StudentFirstName, cs.StudentLastName,cs.DateOfBirth, cs.KIDSID);
			//}

			return dt;
		}
	}
}