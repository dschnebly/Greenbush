﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GreenBushIEP.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class IndividualizedEducationProgramEntities : DbContext
    {
        public IndividualizedEducationProgramEntities()
            : base("name=IndividualizedEducationProgramEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<tblAccommodation> tblAccommodations { get; set; }
        public virtual DbSet<tblAuditLog> tblAuditLogs { get; set; }
        public virtual DbSet<tblBehaviorBaseline> tblBehaviorBaselines { get; set; }
        public virtual DbSet<tblBehaviorHypothesi> tblBehaviorHypothesis { get; set; }
        public virtual DbSet<tblBehaviorHypothesisType> tblBehaviorHypothesisTypes { get; set; }
        public virtual DbSet<tblBehavior> tblBehaviors { get; set; }
        public virtual DbSet<tblBehaviorStrategy> tblBehaviorStrategies { get; set; }
        public virtual DbSet<tblBehaviorStrategyType> tblBehaviorStrategyTypes { get; set; }
        public virtual DbSet<tblBehaviorTrigger> tblBehaviorTriggers { get; set; }
        public virtual DbSet<tblBehaviorTriggerType> tblBehaviorTriggerTypes { get; set; }
        public virtual DbSet<tblBuildingMapping> tblBuildingMappings { get; set; }
        public virtual DbSet<tblBuilding> tblBuildings { get; set; }
        public virtual DbSet<tblCalendar> tblCalendars { get; set; }
        public virtual DbSet<tblCalendarReporting> tblCalendarReportings { get; set; }
        public virtual DbSet<tblCalendarTemplate> tblCalendarTemplates { get; set; }
        public virtual DbSet<tblCounty> tblCounties { get; set; }
        public virtual DbSet<tblDisability> tblDisabilities { get; set; }
        public virtual DbSet<tblDistrict> tblDistricts { get; set; }
        public virtual DbSet<tblEvaluationProcedure> tblEvaluationProcedures { get; set; }
        public virtual DbSet<tblFormArchive> tblFormArchives { get; set; }
        public virtual DbSet<tblGoalBenchmark> tblGoalBenchmarks { get; set; }
        public virtual DbSet<tblGoalEvaluationProcedure> tblGoalEvaluationProcedures { get; set; }
        public virtual DbSet<tblGoal> tblGoals { get; set; }
        public virtual DbSet<tblIEPAcademic> tblIEPAcademics { get; set; }
        public virtual DbSet<tblIEPCommunication> tblIEPCommunications { get; set; }
        public virtual DbSet<tblIEPHealth> tblIEPHealths { get; set; }
        public virtual DbSet<tblIEPIntelligence> tblIEPIntelligences { get; set; }
        public virtual DbSet<tblIEPMath> tblIEPMaths { get; set; }
        public virtual DbSet<tblIEPMotor> tblIEPMotors { get; set; }
        public virtual DbSet<tblIEPReading> tblIEPReadings { get; set; }
        public virtual DbSet<tblIEP> tblIEPs { get; set; }
        public virtual DbSet<tblIEPSocial> tblIEPSocials { get; set; }
        public virtual DbSet<tblIEPWritten> tblIEPWrittens { get; set; }
        public virtual DbSet<tblLanguage> tblLanguages { get; set; }
        public virtual DbSet<tblLocation> tblLocations { get; set; }
        public virtual DbSet<tblOrganizationMapping> tblOrganizationMappings { get; set; }
        public virtual DbSet<tblOtherConsideration> tblOtherConsiderations { get; set; }
        public virtual DbSet<tblPlacementCode> tblPlacementCodes { get; set; }
        public virtual DbSet<tblProviderDistrict> tblProviderDistricts { get; set; }
        public virtual DbSet<tblRole> tblRoles { get; set; }
        public virtual DbSet<tblServiceFrequency> tblServiceFrequencies { get; set; }
        public virtual DbSet<tblService> tblServices { get; set; }
        public virtual DbSet<tblServiceType> tblServiceTypes { get; set; }
        public virtual DbSet<tblStatusCode> tblStatusCodes { get; set; }
        public virtual DbSet<tblStudentInfo> tblStudentInfoes { get; set; }
        public virtual DbSet<tblStudentRelationship> tblStudentRelationships { get; set; }
        public virtual DbSet<tblTransitionAssessment> tblTransitionAssessments { get; set; }
        public virtual DbSet<tblTransitionGoal> tblTransitionGoals { get; set; }
        public virtual DbSet<tblTransition> tblTransitions { get; set; }
        public virtual DbSet<tblTransitionService> tblTransitionServices { get; set; }
        public virtual DbSet<tblUser> tblUsers { get; set; }
        public virtual DbSet<tblVersionLog> tblVersionLogs { get; set; }
        public virtual DbSet<vw_GoalExport> vw_GoalExport { get; set; }
        public virtual DbSet<vw_PrimaryDisabilities> vw_PrimaryDisabilities { get; set; }
        public virtual DbSet<vw_SecondaryDisabilities> vw_SecondaryDisabilities { get; set; }
        public virtual DbSet<vw_ServiceExport> vw_ServiceExport { get; set; }
        public virtual DbSet<vw_StudentExport> vw_StudentExport { get; set; }
        public virtual DbSet<tblProvider> tblProviders { get; set; }
    
        public virtual int sp_alterdiagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_alterdiagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_creatediagram(string diagramname, Nullable<int> owner_id, Nullable<int> version, byte[] definition)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var versionParameter = version.HasValue ?
                new ObjectParameter("version", version) :
                new ObjectParameter("version", typeof(int));
    
            var definitionParameter = definition != null ?
                new ObjectParameter("definition", definition) :
                new ObjectParameter("definition", typeof(byte[]));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_creatediagram", diagramnameParameter, owner_idParameter, versionParameter, definitionParameter);
        }
    
        public virtual int sp_dropdiagram(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_dropdiagram", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagramdefinition_Result> sp_helpdiagramdefinition(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagramdefinition_Result>("sp_helpdiagramdefinition", diagramnameParameter, owner_idParameter);
        }
    
        public virtual ObjectResult<sp_helpdiagrams_Result> sp_helpdiagrams(string diagramname, Nullable<int> owner_id)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_helpdiagrams_Result>("sp_helpdiagrams", diagramnameParameter, owner_idParameter);
        }
    
        public virtual int sp_renamediagram(string diagramname, Nullable<int> owner_id, string new_diagramname)
        {
            var diagramnameParameter = diagramname != null ?
                new ObjectParameter("diagramname", diagramname) :
                new ObjectParameter("diagramname", typeof(string));
    
            var owner_idParameter = owner_id.HasValue ?
                new ObjectParameter("owner_id", owner_id) :
                new ObjectParameter("owner_id", typeof(int));
    
            var new_diagramnameParameter = new_diagramname != null ?
                new ObjectParameter("new_diagramname", new_diagramname) :
                new ObjectParameter("new_diagramname", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_renamediagram", diagramnameParameter, owner_idParameter, new_diagramnameParameter);
        }
    
        public virtual int sp_upgraddiagrams()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_upgraddiagrams");
        }
    
        public virtual ObjectResult<up_ReportProviderCaseload_Result> up_ReportProviderCaseload(string providerId, string fiscalYear, string teacherId, string buildingId)
        {
            var providerIdParameter = providerId != null ?
                new ObjectParameter("ProviderId", providerId) :
                new ObjectParameter("ProviderId", typeof(string));
    
            var fiscalYearParameter = fiscalYear != null ?
                new ObjectParameter("FiscalYear", fiscalYear) :
                new ObjectParameter("FiscalYear", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProviderCaseload_Result>("up_ReportProviderCaseload", providerIdParameter, fiscalYearParameter, teacherIdParameter, buildingIdParameter);
        }
    
        public virtual ObjectResult<up_ReportProceduralDates_Result1> up_ReportProceduralDates(string teacherId, string buildingId)
        {
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProceduralDates_Result1>("up_ReportProceduralDates", teacherIdParameter, buildingIdParameter);
        }
    
        [DbFunction("IndividualizedEducationProgramEntities", "uf_Split")]
        public virtual IQueryable<uf_Split_Result> uf_Split(string mYSTR, string dELIMITER)
        {
            var mYSTRParameter = mYSTR != null ?
                new ObjectParameter("MYSTR", mYSTR) :
                new ObjectParameter("MYSTR", typeof(string));
    
            var dELIMITERParameter = dELIMITER != null ?
                new ObjectParameter("DELIMITER", dELIMITER) :
                new ObjectParameter("DELIMITER", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<uf_Split_Result>("[IndividualizedEducationProgramEntities].[uf_Split](@MYSTR, @DELIMITER)", mYSTRParameter, dELIMITERParameter);
        }
    
        public virtual ObjectResult<up_ReportBuildings_Result2> up_ReportBuildings(Nullable<int> buildingID)
        {
            var buildingIDParameter = buildingID.HasValue ?
                new ObjectParameter("BuildingID", buildingID) :
                new ObjectParameter("BuildingID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportBuildings_Result2>("up_ReportBuildings", buildingIDParameter);
        }
    
        public virtual ObjectResult<up_ReportServices_Result1> up_ReportServices(string serviceId, string buildingId, Nullable<System.DateTime> reportStartDate, Nullable<System.DateTime> reportEndDate)
        {
            var serviceIdParameter = serviceId != null ?
                new ObjectParameter("ServiceId", serviceId) :
                new ObjectParameter("ServiceId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var reportStartDateParameter = reportStartDate.HasValue ?
                new ObjectParameter("ReportStartDate", reportStartDate) :
                new ObjectParameter("ReportStartDate", typeof(System.DateTime));
    
            var reportEndDateParameter = reportEndDate.HasValue ?
                new ObjectParameter("ReportEndDate", reportEndDate) :
                new ObjectParameter("ReportEndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportServices_Result1>("up_ReportServices", serviceIdParameter, buildingIdParameter, reportStartDateParameter, reportEndDateParameter);
        }
    
        public virtual ObjectResult<up_ReportExcessCost_Result> up_ReportExcessCost(string buildingId)
        {
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportExcessCost_Result>("up_ReportExcessCost", buildingIdParameter);
        }
    }
}
