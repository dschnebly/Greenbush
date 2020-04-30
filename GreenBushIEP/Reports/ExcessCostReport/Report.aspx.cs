using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Web.UI;

namespace GreenBushIEP.Reports.ExcessCostReport
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
                GreenBushIEP.Report.ReportMaster.DistrictList(districtDD);
                GreenBushIEP.Report.ReportMaster.BuildingList(buildingDD);
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            ShowReport();
        }

        private void ShowReport()
        {
            ReportViewer MReportViewer = Master.FindControl("ReportViewer1") as ReportViewer;
            MReportViewer.Reset();
            tblUser user = GreenBushIEP.Report.ReportMaster.GetUser(User.Identity.Name);

            string districtID = districtDD.Value;
            string districtName = districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

            string buildingID = buildingDD.Value;
            string buildingName = buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;

            string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(districtDD, districtID);
            string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(districtDD, buildingID, districtID);

            DataTable dt = GetData(districtFilter, buildingFilter);
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            ReportDataSource rds2 = null;
            if (buildingDD.Value != "-1")
            {
                DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingFilter);
                rds2 = new ReportDataSource("DataSet2", dt2);
            }
            else
            {
                DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData("-1");
                rds2 = new ReportDataSource("DataSet2", dt2);
            }
            ReportParameter p1 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
            ReportParameter p2 = new ReportParameter("Building", buildingName);

            MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ExcessCostReport/rptExcessCostReport.rdlc");
            MReportViewer.LocalReport.DataSources.Add(rds);
            MReportViewer.LocalReport.DataSources.Add(rds2);
            MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2 });
            MReportViewer.LocalReport.Refresh();
        }

        private DataTable GetData(string districtFilter, string buildingID)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("StudentFirstName", typeof(string));
            dt.Columns.Add("StudentLastName", typeof(string));
            dt.Columns.Add("DateOfBirth", typeof(DateTime));
            dt.Columns.Add("KIDSID", typeof(string));
            dt.Columns.Add("USD", typeof(string));
            dt.Columns.Add("BuildingName", typeof(string));

            using (IndividualizedEducationProgramEntities ctx = new IndividualizedEducationProgramEntities())
            {
                //Execute stored procedure as a function
                System.Data.Entity.Core.Objects.ObjectResult<up_ReportExcessCost_Result> list = ctx.up_ReportExcessCost(districtFilter, buildingID);

                foreach (up_ReportExcessCost_Result cs in list)
                {
                    dt.Rows.Add(cs.StudentFirstName, cs.StudentLastName, cs.DateOfBirth, cs.KIDSID, cs.USD, cs.BuildingName);
                }
            }

            return dt;
        }
    }
}