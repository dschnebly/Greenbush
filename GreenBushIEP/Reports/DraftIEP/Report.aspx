﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Reports/ReportMaster.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GreenBushIEP.Reports.DraftIEP.Report" %>
<%@ MasterType virtualPath="~/Reports/ReportMaster.Master"%> 
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<% 
		string toggleElement = "style ='margin-bottom: 11px;'";

		if (this.Master.GetUserLevel == "4" || this.Master.GetUserLevel == "6")
		{
			toggleElement = "style='display:none'";
		}
	%>
	<div class="row"  style="margin-bottom: 15px;">
		<h2>Draft IEPs</h2>
	</div>
	<div class="row"  style="margin-bottom: 15px;">
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="districtDD">District</label>
			</div>
			<div class="col-md-6" >
				<select id="districtDD" runat="server" class="chosen-select" data-placeholder="Select District" >
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="building">Building</label>
			</div>
			<div class="col-md-6" >
				<select id="buildingDD" runat="server" class="chosen-select" data-placeholder="Select Building" >
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<div class="col-md-12" <%=toggleElement %>>
			<div class="col-md-2" >
				<label for="teacherDD">Teacher</label>
			</div>
			<div class="col-md-6" >
				<select id="teacherDD" runat="server" multiple="true" class="chosen-select" data-placeholder="All Teachers">
					<option value="">Select</option>
				</select>
				<input type="hidden" id="teacherVals" runat="server" />
			</div>
		</div>
    </div>
	<div class="row">
		<div class="col-md-12" style="margin-left:15px;margin-bottom: 12px;">
			<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="View Report" CssClass="btn btn-default" CausesValidation="false" />			
		</div>
	</div>
</asp:Content>
