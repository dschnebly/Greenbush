﻿using GreenBushIEP.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GreenBushIEP.Reports.ProceduralDates
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{

				var teacherList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
				this.teacherDD.DataSource = teacherList;
				this.teacherDD.DataTextField = "Name";
				this.teacherDD.DataValueField = "UserID";
				this.teacherDD.DataBind();

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
			string teacherIds = "";
			string teacherNames = "";
			string buildingID = this.buildingDD.Value;
			string teacher = "";

			foreach (ListItem li in teacherDD.Items)
			{
				if (li.Selected)
				{
					teacherNames += string.Format("{0}, ", li.Text);
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
				var providerList = GreenBushIEP.Report.ReportMaster.GetTeachers(User.Identity.Name);
				teacherIds = string.Join(",", providerList.Select(o => o.UserID));
			}

			if (user.RoleID == GreenBushIEP.Report.ReportMaster.teacher)
			{
				teacher = user.UserID.ToString();
			}

			DataTable dt = GetData(teacherIds, buildingID);
			ReportDataSource rds = new ReportDataSource("DataSet1", dt);
			DataTable dt2 = GreenBushIEP.Report.ReportMaster.GetBuildingData(buildingID);
			ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2);
			ReportParameter p1 = new ReportParameter("TeacherNames", teacherNames.Trim().Trim(','));
			ReportParameter p2 = new ReportParameter("PrintedBy", GreenBushIEP.Report.ReportMaster.CurrentUser(User.Identity.Name));

			MReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProceduralDates/rptProceduralDates.rdlc");
			MReportViewer.LocalReport.DataSources.Add(rds);
			MReportViewer.LocalReport.DataSources.Add(rds2);
			MReportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2 });
			MReportViewer.LocalReport.Refresh();
		}

		private DataTable GetData(string teacherIds, string buildingID)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("DateType", typeof(string));
			dt.Columns.Add("EvalDate", typeof(string));
			dt.Columns.Add("StudentFirstName", typeof(string));
			dt.Columns.Add("StudentLastName", typeof(string));
			dt.Columns.Add("TeacherFirstName", typeof(string));
			dt.Columns.Add("TeacherLastName", typeof(string));
			dt.Columns.Add("TeacherID", typeof(string));
			
			using (var ctx = new IndividualizedEducationProgramEntities())
			{
				//Execute stored procedure as a function
				var list = ctx.up_ReportProceduralDates(teacherIds, buildingID);

				foreach (var cs in list)
					dt.Rows.Add(cs.DateType, cs.EvalDate, cs.StudentFirstName, cs.StudentLastName, cs.TeacherFirstName, cs.TeacherLastName
						, cs.TeacherID);
			}

			return dt;
		}

	}
}