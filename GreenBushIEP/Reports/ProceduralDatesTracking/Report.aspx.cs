using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;

namespace GreenBushIEP.Reports.ProceduralDatesTracking
{
    public partial class Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
            string teacherId = teacherDD.Value;
            string buildingID = buildingDD.Value;
            string teacher = "";
            string buildingName = buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
            string districtID = districtDD.Value;
            string districtName = districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

            string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(districtDD, districtID);
            string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(districtDD, buildingID, districtID);
            string teacherIds = GreenBushIEP.Report.ReportMaster.GetTeacherFilter(teacherDD, teacherId);
            DateTime startDate = DateTime.Parse(this.startDate.Value);
            DateTime endDate = DateTime.Parse(this.endDate.Value);


            //foreach (ListItem li in teacherDD.Items)
            //{
            //	if (li.Selected)
            //	{
            //		teacherNames += string.Format("{0}, ", li.Text);
            //	}
            //}

            //foreach (ListItem li in teacherDD.Items)
            //{
            //	if (li.Selected)
            //	{
            //		teacherIds += string.Format("{0},", li.Value);
            //	}
            //}

            //if (string.IsNullOrEmpty(teacherIds))
            //{
            //	//get all, but limit list
            //	var providerList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
            //	teacherIds = string.Join(",", providerList.Select(o => o.UserID));
            //}

            if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
            {
                teacher = user.UserID.ToString();
            }


            DataTable dt = GetData(districtFilter, teacherIds, buildingFilter, startDate, endDate);
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);

            ReportParameter p1 = new ReportParameter("TeacherNames", "");
            ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
            ReportParameter p3 = new ReportParameter("Building", buildingName);
            MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProceduralDatesTracking/rptProceduralDatesTracking.rdlc");
            MReportViewer.LocalReport.DataSources.Add(rds);

            MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });
            MReportViewer.LocalReport.Refresh();
        }

        private DataTable GetData(string districtFilter, string teacherIds, string buildingID, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ReEvalConsentSigned", typeof(DateTime));
            dt.Columns.Add("InitialEvalDetermination", typeof(DateTime));
            dt.Columns.Add("InitialEvalConsentSigned", typeof(DateTime));
            dt.Columns.Add("DaysSinceSigned", typeof(int));
            dt.Columns.Add("StudentFirstName", typeof(string));
            dt.Columns.Add("StudentLastName", typeof(string));
            dt.Columns.Add("StudentMiddleName", typeof(string));
            dt.Columns.Add("TeacherFirstName", typeof(string));
            dt.Columns.Add("TeacherLastName", typeof(string));
            dt.Columns.Add("TeacherID", typeof(string));
            dt.Columns.Add("USD", typeof(string));
            dt.Columns.Add("BuildingName", typeof(string));
            dt.Columns.Add("Teachers", typeof(string));



            using (IndividualizedEducationProgramEntities ctx = new IndividualizedEducationProgramEntities())
            {
                //Execute stored procedure as a function
                System.Data.Entity.Core.Objects.ObjectResult<up_ReportProceduralDatesTracking_Result> list = ctx.up_ReportProceduralDatesTracking(districtFilter, teacherIds, buildingID, startDate, endDate);

                foreach (up_ReportProceduralDatesTracking_Result cs in list)
                {
                    dt.Rows.Add(cs.ReEvalConsentSigned, cs.InitialEvalDetermination, cs.InitialEvalConsentSigned, cs.DaysSinceSigned
                        , cs.StudentFirstName, cs.StudentLastName, cs.StudentMiddleName, cs.TeacherFirstName, cs.TeacherLastName
                        , cs.TeacherID, cs.USD, cs.BuildingName, cs.Teachers);
                }
            }

            return dt;
        }
    }
}