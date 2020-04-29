using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProceduralDates
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
                GreenBushIEP.Report.ReportMaster.TeacherList(teacherDD);
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
            string teacherIds = "";
            string teacherNames = "";
            string buildingID = buildingDD.Value;
            string teacher = "";
            string buildingName = buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
            string districtID = districtDD.Value;
            string districtName = districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

            string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(districtDD, districtID);
            string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(districtDD, buildingID, districtID);

            foreach (ListItem li in teacherDD.Items)
            {
                if (li.Selected)
                {
                    teacherNames += string.Format("{0},", li.Text);
                }
            }

            foreach (ListItem li in teacherDD.Items)
            {
                if (li.Selected)
                {
                    teacherIds += string.Format("{0},", li.Value);
                }
            }

            if (string.IsNullOrEmpty(teacherIds))
            {
                //get all, but limit list
                System.Collections.Generic.List<TeacherView> providerList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
                teacherIds = string.Join(",", providerList.Select(o => o.UserID));
            }

            if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
            {
                teacher = user.UserID.ToString();
            }


            DataTable dt = GetData(districtFilter, teacherIds, buildingFilter);
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
            ReportParameter p1 = new ReportParameter("TeacherNames", teacherNames.Trim().Trim(','));
            ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
            ReportParameter p3 = new ReportParameter("Building", buildingName);
            MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProceduralDates/rptProceduralDates.rdlc");
            MReportViewer.LocalReport.DataSources.Add(rds);
            MReportViewer.LocalReport.DataSources.Add(rds2);
            MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
            MReportViewer.LocalReport.Refresh();
        }

        private DataTable GetData(string districtFilter, string teacherIds, string buildingID)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("DateType", typeof(string));
            dt.Columns.Add("EvalDate", typeof(string));
            dt.Columns.Add("StudentFirstName", typeof(string));
            dt.Columns.Add("StudentLastName", typeof(string));
            dt.Columns.Add("Teachers", typeof(string));
            dt.Columns.Add("USD", typeof(string));
            dt.Columns.Add("BuildingName", typeof(string));

            using (IndividualizedEducationProgramEntities ctx = new IndividualizedEducationProgramEntities())
            {
                //Execute stored procedure as a function
                System.Data.Entity.Core.Objects.ObjectResult<up_ReportProceduralDates_Result> list = ctx.up_ReportProceduralDates(districtFilter, teacherIds, buildingID);

                foreach (up_ReportProceduralDates_Result cs in list)
                {
                    dt.Rows.Add(cs.DateType, cs.EvalDate, cs.StudentFirstName, cs.StudentLastName, cs.Teachers, cs.USD, cs.BuildingName);
                }
            }

            return dt;
        }

    }
}