<%@ Page Title="" Language="C#" MasterPageFile="~/Reports/ReportMaster.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GreenBushIEP.Reports.ProgressReport.Report" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
		<div class="row"  style="margin-bottom: 15px;">
		<h2>Progress Report</h2>
	</div>
	<div class="row"  style="margin-bottom: 15px;">
		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2" >
				<label for="statusDD">IEP Status</label>
			</div>
			<div class="col-md-6" >
				<select id="statusDD" runat="server" class="chosen-select" data-placeholder="Select Status">
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
				<label for="providerDD">Provider</label>
			</div>
			<div class="col-md-6" >
				<select id="providerDD" runat="server" multiple="true" class="chosen-select" data-placeholder="All Providers">
					<option value="">Select</option>
				</select>
			</div>
		</div>

		<div class="col-md-12" style="margin-bottom: 12px;">
			<div class="col-md-2">
				<label>Print Options</label>
			</div>
			<div class="col-md-1">
				<input type="checkbox" id="cbPrintGoal" runat="server" checked />
				Goal</div>

			<div class="col-md-2">
				<input type="checkbox" id="cbPrintGoalBenchmarks" runat="server" checked />
				Benchmarks </div>

			<div class="col-md-2">
				<select id="quarters" runat="server" class="form-control">
					<option value="0">All Quarters</option>
					<option value="1">Quarter 1</option>
					<option value="2">Quarter 2</option>
					<option value="3">Quarter 3</option>
					<option value="4">Quarter 4</option>
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
