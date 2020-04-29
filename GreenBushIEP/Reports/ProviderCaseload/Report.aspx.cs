﻿using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProviderCaseload
{
    public partial class Report1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account/Login");
            }

            if (!IsPostBack)
            {
                GreenBushIEP.Report.ReportMaster.ProviderList(providerDD);
                GreenBushIEP.Report.ReportMaster.DistrictList(districtDD);
                GreenBushIEP.Report.ReportMaster.BuildingList(buildingDD);
                GreenBushIEP.Report.ReportMaster.StudentList(studentDD);
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
            string providerIds = "";
            string providerNames = "";
            string fiscalYears = "";
            string fiscalYearsNames = "";
            string buildingID = buildingDD.Value;
            string buildingName = buildingDD.Value == "-1" ? "All" : buildingDD.Items[buildingDD.SelectedIndex].Text;
            string teacher = "";
            string districtID = districtDD.Value;
            string districtName = districtDD.Value == "-1" ? "All" : districtDD.Items[districtDD.SelectedIndex].Text;

            string districtFilter = GreenBushIEP.Report.ReportMaster.GetDistrictFilter(districtDD, districtID);
            string buildingFilter = GreenBushIEP.Report.ReportMaster.GetBuildingFilter(districtDD, buildingID, districtID);

            string studentFilter = studentDD.Value == "-1" ? "" : studentDD.Value;

            foreach (ListItem li in providerDD.Items)
            {
                if (li.Selected)
                {
                    providerNames += string.Format("{0},", li.Text);
                }
            }

            foreach (ListItem li in providerDD.Items)
            {
                if (li.Selected)
                {
                    providerIds += string.Format("{0},", li.Value);
                }
            }

            foreach (ListItem li in fiscalYear.Items)
            {
                if (li.Selected)
                {
                    fiscalYearsNames += string.Format("{0},", li.Text);
                }
            }

            foreach (ListItem li in fiscalYear.Items)
            {
                if (li.Selected)
                {
                    fiscalYears += string.Format("{0},", li.Value);
                }
            }


            if (string.IsNullOrEmpty(providerIds))
            {
                //get all, but limit list
                System.Collections.Generic.List<ProviderViewModel> providerList = GreenBushIEP.Report.ReportMaster.GetProviders(User.Identity.Name);
                providerIds = string.Join(",", providerList.Select(o => o.ProviderID));
            }

            if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher || user.RoleID == GreenBushIEP.Report.ReportMaster.nurse)
            {
                teacher = user.UserID.ToString();
            }


            DataTable dt = GetData(districtFilter, providerIds, fiscalYears, teacher, buildingFilter, studentFilter);
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
            ReportParameter p1 = new ReportParameter("ProviderNames", providerNames.Trim().Trim(','));
            ReportParameter p2 = new ReportParameter("FiscalYears", fiscalYearsNames.Trim().Trim(','));
            ReportParameter p3 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));
            ReportParameter p4 = new ReportParameter("Building", buildingName);
            MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProviderCaseload/rptProviderCaseload.rdlc");
            MReportViewer.LocalReport.DataSources.Add(rds);
            MReportViewer.LocalReport.DataSources.Add(rds2);
            MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });
            MReportViewer.LocalReport.Refresh();
        }

        private DataTable GetData(string districtFilter, string providerId, string fiscalYear, string teacher, string buildingID, string studentIds)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LastName", typeof(string));
            dt.Columns.Add("FirstName", typeof(string));
            dt.Columns.Add("ProviderName", typeof(string));
            dt.Columns.Add("GoalTitle", typeof(string));
            dt.Columns.Add("Location", typeof(string));
            dt.Columns.Add("Minutes", typeof(string));
            dt.Columns.Add("DaysPerWeek", typeof(string));
            dt.Columns.Add("Frequency", typeof(string));
            dt.Columns.Add("ServiceType", typeof(string));
            dt.Columns.Add("USD", typeof(string));
            dt.Columns.Add("BuildingName", typeof(string));
            dt.Columns.Add("FrequencyDesc", typeof(string));
            using (IndividualizedEducationProgramEntities ctx = new IndividualizedEducationProgramEntities())
            {
                //Execute stored procedure as a function
                System.Data.Entity.Core.Objects.ObjectResult<up_ReportProviderCaseload_Result> list = ctx.up_ReportProviderCaseload(districtFilter, providerId, fiscalYear, teacher, buildingID, studentIds);

                foreach (up_ReportProviderCaseload_Result cs in list)
                {
                    dt.Rows.Add(cs.LastName, cs.FirstName, cs.ProviderName, cs.GoalTitle
                        , cs.Location, cs.Minutes, cs.DaysPerWeek, cs.Frequency, cs.ServiceType
                        , cs.USD, cs.BuildingName, cs.FrequencyDesc);
                }
            }

            return dt;
        }


    }
}