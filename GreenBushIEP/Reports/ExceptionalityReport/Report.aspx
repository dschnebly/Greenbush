﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Reports/ReportMaster.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GreenBushIEP.Reports.ExceptionalityReport.Report" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="row"  style="margin-bottom: 15px;">
		<h2>Exceptionality Report</h2>
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
				<label for="building">Building</label>
			</div>
			<div class="col-md-6" >
				<select id="buildingDD" runat="server" class="chosen-select" data-placeholder="Select Building">
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="startDate">Start Date</label>
			</div>
			<div class="col-md-6" >
					<input id="startDate" runat="server" name="startDate" title="Start Date" type="text" class="dtField" value="" style="z-index: 99999; " required>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="endDate">End Date</label>
			</div>
			<div class="col-md-6" >
					<input id="endDate" runat="server" name="startDate" title="Start Date" type="text" class="dtField" value="" style="z-index: 99999; " required>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				 <label   for="ServiceType">Service Setting</label>
			</div>
			<div class="col-md-6" >
				 <select id="ServiceType" runat="server" class="chosen-select">
                      <option value="">Select</option>
                </select>
			</div>
		</div>
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				 <label for="SelectGifted">Gifted</label>
			</div>
			<div class="col-md-6" >
				 <select id="SelectGifted" runat="server" class="chosen-select">
                      <option value="0">Select</option>
					  <option value="1">No</option>
					 <option value="2">Yes</option>
                </select>
			</div>
		</div>
		 
    </div>
	<div class="row">
		<div class="col-md-12" style="margin-left:15px;margin-bottom: 12px;">
			<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="View Report" CssClass="btn btn-default" />
		</div>
	</div>
</asp:Content>
