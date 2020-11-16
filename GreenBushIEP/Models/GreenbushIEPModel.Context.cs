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
    
        public virtual DbSet<tblAccommodationModule> tblAccommodationModules { get; set; }
        public virtual DbSet<tblAccommodation> tblAccommodations { get; set; }
        public virtual DbSet<tblArchiveCalendar> tblArchiveCalendars { get; set; }
        public virtual DbSet<tblArchiveEvaluationDate> tblArchiveEvaluationDates { get; set; }
        public virtual DbSet<tblArchiveEvaluationDateSigned> tblArchiveEvaluationDateSigneds { get; set; }
        public virtual DbSet<tblArchiveIEPExit> tblArchiveIEPExits { get; set; }
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
        public virtual DbSet<tblBuildingType> tblBuildingTypes { get; set; }
        public virtual DbSet<tblCalendar> tblCalendars { get; set; }
        public virtual DbSet<tblCalendarReporting> tblCalendarReportings { get; set; }
        public virtual DbSet<tblCalendarTemplate> tblCalendarTemplates { get; set; }
        public virtual DbSet<tblCareerPath> tblCareerPaths { get; set; }
        public virtual DbSet<tblContact> tblContacts { get; set; }
        public virtual DbSet<tblContingencyPlan> tblContingencyPlans { get; set; }
        public virtual DbSet<tblCounty> tblCounties { get; set; }
        public virtual DbSet<tblDisability> tblDisabilities { get; set; }
        public virtual DbSet<tblDistrict> tblDistricts { get; set; }
        public virtual DbSet<tblErrorLog> tblErrorLogs { get; set; }
        public virtual DbSet<tblEvaluationProcedure> tblEvaluationProcedures { get; set; }
        public virtual DbSet<tblFormArchive> tblFormArchives { get; set; }
        public virtual DbSet<tblFormChildOutcome> tblFormChildOutcomes { get; set; }
        public virtual DbSet<tblFormChildOutcomes_PersonsInvolved> tblFormChildOutcomes_PersonsInvolved { get; set; }
        public virtual DbSet<tblFormChildOutcomes_SupportingEvidence> tblFormChildOutcomes_SupportingEvidence { get; set; }
        public virtual DbSet<tblFormConferenceSummary> tblFormConferenceSummaries { get; set; }
        public virtual DbSet<tblFormContinuousLearningPlan> tblFormContinuousLearningPlans { get; set; }
        public virtual DbSet<tblFormIEPAmendment> tblFormIEPAmendments { get; set; }
        public virtual DbSet<tblFormIEPMeetingConsentToInvite> tblFormIEPMeetingConsentToInvites { get; set; }
        public virtual DbSet<tblFormIEPMeetingExcusal> tblFormIEPMeetingExcusals { get; set; }
        public virtual DbSet<tblFormIEPTeamConsideration> tblFormIEPTeamConsiderations { get; set; }
        public virtual DbSet<tblFormManifestationDeterminiation> tblFormManifestationDeterminiations { get; set; }
        public virtual DbSet<tblFormManifestDeterm_TeamMembers> tblFormManifestDeterm_TeamMembers { get; set; }
        public virtual DbSet<tblFormNoticeOfMeeting> tblFormNoticeOfMeetings { get; set; }
        public virtual DbSet<tblFormParentConsent> tblFormParentConsents { get; set; }
        public virtual DbSet<tblFormPhysicianScript> tblFormPhysicianScripts { get; set; }
        public virtual DbSet<tblFormPriorWritten_Eval> tblFormPriorWritten_Eval { get; set; }
        public virtual DbSet<tblFormPriorWritten_Ident> tblFormPriorWritten_Ident { get; set; }
        public virtual DbSet<tblFormPriorWritten_ReokeAll> tblFormPriorWritten_ReokeAll { get; set; }
        public virtual DbSet<tblFormPriorWritten_ReokePart> tblFormPriorWritten_ReokePart { get; set; }
        public virtual DbSet<tblFormPublicNotice> tblFormPublicNotices { get; set; }
        public virtual DbSet<tblFormRevokeConsentAll> tblFormRevokeConsentAlls { get; set; }
        public virtual DbSet<tblFormRevokeConsentPart> tblFormRevokeConsentParts { get; set; }
        public virtual DbSet<tblFormSummaryPerformance> tblFormSummaryPerformances { get; set; }
        public virtual DbSet<tblFormTeamEval> tblFormTeamEvals { get; set; }
        public virtual DbSet<tblFormTransportationRequest> tblFormTransportationRequests { get; set; }
        public virtual DbSet<tblGoalBenchmarkMethod> tblGoalBenchmarkMethods { get; set; }
        public virtual DbSet<tblGoalBenchmark> tblGoalBenchmarks { get; set; }
        public virtual DbSet<tblGoalEvaluationProcedure> tblGoalEvaluationProcedures { get; set; }
        public virtual DbSet<tblGoal> tblGoals { get; set; }
        public virtual DbSet<tblGrade> tblGrades { get; set; }
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
        public virtual DbSet<tblModule> tblModules { get; set; }
        public virtual DbSet<tblOrganizationMapping> tblOrganizationMappings { get; set; }
        public virtual DbSet<tblOtherConsideration> tblOtherConsiderations { get; set; }
        public virtual DbSet<tblPermission> tblPermissions { get; set; }
        public virtual DbSet<tblPlacementCode> tblPlacementCodes { get; set; }
        public virtual DbSet<tblProviderDistrict> tblProviderDistricts { get; set; }
        public virtual DbSet<tblProvider> tblProviders { get; set; }
        public virtual DbSet<tblRace> tblRaces { get; set; }
        public virtual DbSet<tblReferralInfo> tblReferralInfoes { get; set; }
        public virtual DbSet<tblReferralRelationship> tblReferralRelationships { get; set; }
        public virtual DbSet<tblReferralRequest> tblReferralRequests { get; set; }
        public virtual DbSet<tblRolePermission> tblRolePermissions { get; set; }
        public virtual DbSet<tblRole> tblRoles { get; set; }
        public virtual DbSet<tblServiceFrequency> tblServiceFrequencies { get; set; }
        public virtual DbSet<tblService> tblServices { get; set; }
        public virtual DbSet<tblServiceType> tblServiceTypes { get; set; }
        public virtual DbSet<tblStatusCode> tblStatusCodes { get; set; }
        public virtual DbSet<tblStudentInfo> tblStudentInfoes { get; set; }
        public virtual DbSet<tblStudentNote> tblStudentNotes { get; set; }
        public virtual DbSet<tblStudentNotes_MIS> tblStudentNotes_MIS { get; set; }
        public virtual DbSet<tblStudentRelationship> tblStudentRelationships { get; set; }
        public virtual DbSet<tblTransitionAssessment> tblTransitionAssessments { get; set; }
        public virtual DbSet<tblTransitionGoal> tblTransitionGoals { get; set; }
        public virtual DbSet<tblTransition> tblTransitions { get; set; }
        public virtual DbSet<tblTransitionService> tblTransitionServices { get; set; }
        public virtual DbSet<tblUserPermission> tblUserPermissions { get; set; }
        public virtual DbSet<tblUser> tblUsers { get; set; }
        public virtual DbSet<tblVersionLog> tblVersionLogs { get; set; }
        public virtual DbSet<vw_BuildingList> vw_BuildingList { get; set; }
        public virtual DbSet<vw_BuildingsForAttendance> vw_BuildingsForAttendance { get; set; }
        public virtual DbSet<vw_GoalExport> vw_GoalExport { get; set; }
        public virtual DbSet<vw_ModuleAccommodationFlags> vw_ModuleAccommodationFlags { get; set; }
        public virtual DbSet<vw_ModuleGoalFlags> vw_ModuleGoalFlags { get; set; }
        public virtual DbSet<vw_ModuleOtherFlags> vw_ModuleOtherFlags { get; set; }
        public virtual DbSet<vw_PrimaryDisabilities> vw_PrimaryDisabilities { get; set; }
        public virtual DbSet<vw_SecondaryDisabilities> vw_SecondaryDisabilities { get; set; }
        public virtual DbSet<vw_ServiceExport> vw_ServiceExport { get; set; }
        public virtual DbSet<vw_StudentExport> vw_StudentExport { get; set; }
        public virtual DbSet<vw_UserList> vw_UserList { get; set; }
        public virtual DbSet<tblFormTransitionReferral> tblFormTransitionReferrals { get; set; }
        public virtual DbSet<tblBook> tblBooks { get; set; }
        public virtual DbSet<tblUserRole> tblUserRoles { get; set; }
    
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
    
        public virtual ObjectResult<up_ReportBuildings_Result> up_ReportBuildings(Nullable<int> buildingID)
        {
            var buildingIDParameter = buildingID.HasValue ?
                new ObjectParameter("BuildingID", buildingID) :
                new ObjectParameter("BuildingID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportBuildings_Result>("up_ReportBuildings", buildingIDParameter);
        }
    
        public virtual ObjectResult<up_ReportDraftIEPS_Result> up_ReportDraftIEPS(string districtId, string teacherId, string buildingId, string studentIds)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var studentIdsParameter = studentIds != null ?
                new ObjectParameter("StudentIds", studentIds) :
                new ObjectParameter("StudentIds", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportDraftIEPS_Result>("up_ReportDraftIEPS", districtIdParameter, teacherIdParameter, buildingIdParameter, studentIdsParameter);
        }
    
        public virtual ObjectResult<up_ReportExcessCost_Result> up_ReportExcessCost(string districtId, string buildingId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportExcessCost_Result>("up_ReportExcessCost", districtIdParameter, buildingIdParameter);
        }
    
        public virtual ObjectResult<up_ReportIEPSDue_Result> up_ReportIEPSDue(string districtId, string teacherId, string buildingId, string studentIds)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var studentIdsParameter = studentIds != null ?
                new ObjectParameter("StudentIds", studentIds) :
                new ObjectParameter("StudentIds", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportIEPSDue_Result>("up_ReportIEPSDue", districtIdParameter, teacherIdParameter, buildingIdParameter, studentIdsParameter);
        }
    
        public virtual ObjectResult<up_ReportProceduralDates_Result> up_ReportProceduralDates(string districtId, string teacherId, string buildingId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProceduralDates_Result>("up_ReportProceduralDates", districtIdParameter, teacherIdParameter, buildingIdParameter);
        }
    
        public virtual ObjectResult<up_ReportProceduralDatesTracking_Result> up_ReportProceduralDatesTracking(string districtId, string teacherId, string buildingId, Nullable<System.DateTime> reportStartDate, Nullable<System.DateTime> reportEndDate)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var reportStartDateParameter = reportStartDate.HasValue ?
                new ObjectParameter("ReportStartDate", reportStartDate) :
                new ObjectParameter("ReportStartDate", typeof(System.DateTime));
    
            var reportEndDateParameter = reportEndDate.HasValue ?
                new ObjectParameter("ReportEndDate", reportEndDate) :
                new ObjectParameter("ReportEndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProceduralDatesTracking_Result>("up_ReportProceduralDatesTracking", districtIdParameter, teacherIdParameter, buildingIdParameter, reportStartDateParameter, reportEndDateParameter);
        }
    
        public virtual ObjectResult<up_ReportProgress_Result> up_ReportProgress(string districtId, string status, string buildingId, string providerId, string teacherId, string studentId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var statusParameter = status != null ?
                new ObjectParameter("Status", status) :
                new ObjectParameter("Status", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var providerIdParameter = providerId != null ?
                new ObjectParameter("ProviderId", providerId) :
                new ObjectParameter("ProviderId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProgress_Result>("up_ReportProgress", districtIdParameter, statusParameter, buildingIdParameter, providerIdParameter, teacherIdParameter, studentIdParameter);
        }
    
        public virtual ObjectResult<up_ReportProviderCaseload_Result> up_ReportProviderCaseload(string districtId, string providerId, string fiscalYear, string teacherId, string buildingId, string studentId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
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
    
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportProviderCaseload_Result>("up_ReportProviderCaseload", districtIdParameter, providerIdParameter, fiscalYearParameter, teacherIdParameter, buildingIdParameter, studentIdParameter);
        }
    
        public virtual ObjectResult<up_ReportServices_Result> up_ReportServices(string districtId, string serviceId, string buildingId, Nullable<System.DateTime> reportStartDate, Nullable<System.DateTime> reportEndDate, string teacherId, string providerId, Nullable<int> gifted)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
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
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var providerIdParameter = providerId != null ?
                new ObjectParameter("ProviderId", providerId) :
                new ObjectParameter("ProviderId", typeof(string));
    
            var giftedParameter = gifted.HasValue ?
                new ObjectParameter("Gifted", gifted) :
                new ObjectParameter("Gifted", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportServices_Result>("up_ReportServices", districtIdParameter, serviceIdParameter, buildingIdParameter, reportStartDateParameter, reportEndDateParameter, teacherIdParameter, providerIdParameter, giftedParameter);
        }
    
        public virtual ObjectResult<up_ReportStudentsByBuilding_Result> up_ReportStudentsByBuilding(string usd, string buildingId, Nullable<System.DateTime> reportStartDate, Nullable<System.DateTime> reportEndDate, string statusCode, string teacherId)
        {
            var usdParameter = usd != null ?
                new ObjectParameter("Usd", usd) :
                new ObjectParameter("Usd", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var reportStartDateParameter = reportStartDate.HasValue ?
                new ObjectParameter("ReportStartDate", reportStartDate) :
                new ObjectParameter("ReportStartDate", typeof(System.DateTime));
    
            var reportEndDateParameter = reportEndDate.HasValue ?
                new ObjectParameter("ReportEndDate", reportEndDate) :
                new ObjectParameter("ReportEndDate", typeof(System.DateTime));
    
            var statusCodeParameter = statusCode != null ?
                new ObjectParameter("StatusCode", statusCode) :
                new ObjectParameter("StatusCode", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportStudentsByBuilding_Result>("up_ReportStudentsByBuilding", usdParameter, buildingIdParameter, reportStartDateParameter, reportEndDateParameter, statusCodeParameter, teacherIdParameter);
        }
    
        public virtual ObjectResult<uspCopyIEP_Result> uspCopyIEP(Nullable<int> fromIEP, Nullable<int> byUserID, Nullable<bool> ammend)
        {
            var fromIEPParameter = fromIEP.HasValue ?
                new ObjectParameter("fromIEP", fromIEP) :
                new ObjectParameter("fromIEP", typeof(int));
    
            var byUserIDParameter = byUserID.HasValue ?
                new ObjectParameter("byUserID", byUserID) :
                new ObjectParameter("byUserID", typeof(int));
    
            var ammendParameter = ammend.HasValue ?
                new ObjectParameter("ammend", ammend) :
                new ObjectParameter("ammend", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspCopyIEP_Result>("uspCopyIEP", fromIEPParameter, byUserIDParameter, ammendParameter);
        }
    
        public virtual ObjectResult<uspUserList_Result> uspUserList(Nullable<int> userID, string uSD, string buildingID, Nullable<bool> isAssgined, Nullable<bool> isArchived)
        {
            var userIDParameter = userID.HasValue ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(int));
    
            var uSDParameter = uSD != null ?
                new ObjectParameter("USD", uSD) :
                new ObjectParameter("USD", typeof(string));
    
            var buildingIDParameter = buildingID != null ?
                new ObjectParameter("BuildingID", buildingID) :
                new ObjectParameter("BuildingID", typeof(string));
    
            var isAssginedParameter = isAssgined.HasValue ?
                new ObjectParameter("isAssgined", isAssgined) :
                new ObjectParameter("isAssgined", typeof(bool));
    
            var isArchivedParameter = isArchived.HasValue ?
                new ObjectParameter("isArchived", isArchived) :
                new ObjectParameter("isArchived", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspUserList_Result>("uspUserList", userIDParameter, uSDParameter, buildingIDParameter, isAssginedParameter, isArchivedParameter);
        }
    
        public virtual ObjectResult<uspUserListByUserID_Result> uspUserListByUserID(Nullable<int> userID, Nullable<bool> isAssgined)
        {
            var userIDParameter = userID.HasValue ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(int));
    
            var isAssginedParameter = isAssgined.HasValue ?
                new ObjectParameter("isAssgined", isAssgined) :
                new ObjectParameter("isAssgined", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspUserListByUserID_Result>("uspUserListByUserID", userIDParameter, isAssginedParameter);
        }
    
        public virtual ObjectResult<uspServiceProviders_Result> uspServiceProviders(string uSD)
        {
            var uSDParameter = uSD != null ?
                new ObjectParameter("USD", uSD) :
                new ObjectParameter("USD", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspServiceProviders_Result>("uspServiceProviders", uSDParameter);
        }
    
        public virtual ObjectResult<uspServiceProviderStudents_Result> uspServiceProviderStudents(Nullable<int> providerID)
        {
            var providerIDParameter = providerID.HasValue ?
                new ObjectParameter("ProviderID", providerID) :
                new ObjectParameter("ProviderID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspServiceProviderStudents_Result>("uspServiceProviderStudents", providerIDParameter);
        }
    
        public virtual ObjectResult<uspUserListByProvider_Result> uspUserListByProvider(Nullable<int> userID, string uSD, string buildingID, Nullable<int> providerID)
        {
            var userIDParameter = userID.HasValue ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(int));
    
            var uSDParameter = uSD != null ?
                new ObjectParameter("USD", uSD) :
                new ObjectParameter("USD", typeof(string));
    
            var buildingIDParameter = buildingID != null ?
                new ObjectParameter("BuildingID", buildingID) :
                new ObjectParameter("BuildingID", typeof(string));
    
            var providerIDParameter = providerID.HasValue ?
                new ObjectParameter("ProviderID", providerID) :
                new ObjectParameter("ProviderID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspUserListByProvider_Result>("uspUserListByProvider", userIDParameter, uSDParameter, buildingIDParameter, providerIDParameter);
        }
    
        public virtual ObjectResult<up_ReportStudentInfo_Result> up_ReportStudentInfo(string districtId, string buildingId, string providerId, string teacherId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var providerIdParameter = providerId != null ?
                new ObjectParameter("ProviderId", providerId) :
                new ObjectParameter("ProviderId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportStudentInfo_Result>("up_ReportStudentInfo", districtIdParameter, buildingIdParameter, providerIdParameter, teacherIdParameter);
        }
    
        public virtual ObjectResult<uspUserAssignedList_Result> uspUserAssignedList(Nullable<int> userID, string uSD, string buildingID, Nullable<bool> noBuilding, Nullable<bool> isArchived)
        {
            var userIDParameter = userID.HasValue ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(int));
    
            var uSDParameter = uSD != null ?
                new ObjectParameter("USD", uSD) :
                new ObjectParameter("USD", typeof(string));
    
            var buildingIDParameter = buildingID != null ?
                new ObjectParameter("BuildingID", buildingID) :
                new ObjectParameter("BuildingID", typeof(string));
    
            var noBuildingParameter = noBuilding.HasValue ?
                new ObjectParameter("noBuilding", noBuilding) :
                new ObjectParameter("noBuilding", typeof(bool));
    
            var isArchivedParameter = isArchived.HasValue ?
                new ObjectParameter("isArchived", isArchived) :
                new ObjectParameter("isArchived", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<uspUserAssignedList_Result>("uspUserAssignedList", userIDParameter, uSDParameter, buildingIDParameter, noBuildingParameter, isArchivedParameter);
        }
    
        public virtual int uspYearlyAutoAdvance()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("uspYearlyAutoAdvance");
        }
    
        public virtual ObjectResult<up_ReportBehaviorPlan_Result> up_ReportBehaviorPlan(string districtId, string buildingId, string teacherId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportBehaviorPlan_Result>("up_ReportBehaviorPlan", districtIdParameter, buildingIdParameter, teacherIdParameter);
        }
    
        public virtual ObjectResult<up_ReportESY_Result> up_ReportESY(string districtId, string buildingId, Nullable<System.DateTime> reportStartDate, Nullable<System.DateTime> reportEndDate, string teacherId, string providerId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var reportStartDateParameter = reportStartDate.HasValue ?
                new ObjectParameter("ReportStartDate", reportStartDate) :
                new ObjectParameter("ReportStartDate", typeof(System.DateTime));
    
            var reportEndDateParameter = reportEndDate.HasValue ?
                new ObjectParameter("ReportEndDate", reportEndDate) :
                new ObjectParameter("ReportEndDate", typeof(System.DateTime));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            var providerIdParameter = providerId != null ?
                new ObjectParameter("ProviderId", providerId) :
                new ObjectParameter("ProviderId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportESY_Result>("up_ReportESY", districtIdParameter, buildingIdParameter, reportStartDateParameter, reportEndDateParameter, teacherIdParameter, providerIdParameter);
        }
    
        public virtual ObjectResult<up_ReportKIDSID_Result> up_ReportKIDSID(string districtId, string buildingId, string teacherId)
        {
            var districtIdParameter = districtId != null ?
                new ObjectParameter("DistrictId", districtId) :
                new ObjectParameter("DistrictId", typeof(string));
    
            var buildingIdParameter = buildingId != null ?
                new ObjectParameter("BuildingId", buildingId) :
                new ObjectParameter("BuildingId", typeof(string));
    
            var teacherIdParameter = teacherId != null ?
                new ObjectParameter("TeacherId", teacherId) :
                new ObjectParameter("TeacherId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<up_ReportKIDSID_Result>("up_ReportKIDSID", districtIdParameter, buildingIdParameter, teacherIdParameter);
        }
    
        public virtual ObjectResult<string> uspGetUserBooks(Nullable<int> userID, string email)
        {
            var userIDParameter = userID.HasValue ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(int));
    
            var emailParameter = email != null ?
                new ObjectParameter("Email", email) :
                new ObjectParameter("Email", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("uspGetUserBooks", userIDParameter, emailParameter);
        }
    }
}
