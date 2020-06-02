<%@ Page Title="" Language="C#" MasterPageFile="~/Reports/ReportMaster.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GreenBushIEP.Reports.ESY.Report" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	
	<div class="row"  style="margin-bottom: 15px;">
		<h2>Extended School Year</h2>
	</div>
	<div class="row"  style="margin-bottom: 15px;">
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="districtDD">District</label>
			</div>
			<div class="col-md-6" >
				<select id="districtDD" runat="server" class="chosen-select" data-placeholder="Select District">
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="building">Attendance Building</label>
			</div>
			<div class="col-md-6" >
				<select id="buildingDD" runat="server" class="chosen-select" data-placeholder="Select Building">
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<%--<div class="col-md-12" <%=toggleElement %>>
			<div class="col-md-2" >
				<label for="teacherDD">Teacher</label>
			</div>
			<div class="col-md-6" >
				<select id="teacherDD" runat="server" multiple="true" class="chosen-select" data-placeholder="All Teachers">
					<option value="">Select</option>
				</select>
				<input type="hidden" id="teacherVals" runat="server" />
			</div>
		</div>--%>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="providerDD">Provider</label>
			</div>
			<div class="col-md-6" >
				<select id="providerDD" runat="server" multiple="true" class="chosen-select" data-placeholder="All Providers">
					<option value="">Select</option>
				</select>
				<input type="hidden" id="providerVals" runat="server" />
			</div>
		</div>
	</div>		
	<div class="row">
		<div class="col-md-12" style="margin-left:15px;margin-bottom: 12px;">
			<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="View Report" CssClass="btn btn-default" />
		</div>
	</div>
</asp:Content>
