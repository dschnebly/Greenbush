﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Reports/ReportMaster.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GreenBushIEP.Reports.ProviderCaseload.Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<h2>Caseloads by Provider</h2>

	<div class="row"  style="margin-bottom: 15px;">
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
				<label for="fiscalYear">Provider</label>
			</div>
			<div class="col-md-6" >
				<select id="providerDD" runat="server" multiple="true" class="chosen-select" data-placeholder="Select Provider">
					<option value="">Select</option>
				</select>
			</div>
		</div>
		<div class="col-md-12">
				<div class="col-md-2" >
				<label for="fiscalYear">Fiscal Year</label>
				</div>
			<div class="col-md-6" >
					<select id="fiscalYear" style="border-color: #ccc; display: inline-block;"  multiple="true" class="chosen-select" runat="server"  data-placeholder="Select Fiscal Year">
						<option value="2018">2017 - 2018</option>
						<option value="2019">2018 - 2019</option>
						<option value="2020">2019 - 2020</option>
						<option value="2021">2020 - 2021</option>
						<option value="2022">2021 - 2022</option>
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
