//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class tblFormContinuousLearningPlan
    {
        public int FormContinuousLearningPlanId { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> ICLPDate { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.DateTime> EndingDate { get; set; }
        public string StudentName { get; set; }
        public string ResponsibleBuilding { get; set; }
        public string Grade { get; set; }
        public string PrimaryDisability { get; set; }
        public string Provider { get; set; }
        public string AttendingBuilding { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<System.DateTime> EvaluationCompletion { get; set; }
        public Nullable<System.DateTime> IEPDate { get; set; }
        public Nullable<bool> AccessToInternetBasedActivities_Yes { get; set; }
        public Nullable<bool> AccessToInternetBasedActivities_No { get; set; }
        public Nullable<bool> AccessToInternetBasedActivities_Home { get; set; }
        public Nullable<bool> AccessToInternetBasedActivities_HotSpot { get; set; }
        public Nullable<bool> AccessToServiceDelivery_Yes { get; set; }
        public Nullable<bool> AccessToServiceDelivery_No { get; set; }
        public Nullable<bool> AccessToEmailCommunication_Yes { get; set; }
        public Nullable<bool> AccessToEmailCommunication_No { get; set; }
        public Nullable<bool> AccessToWorkPacket_Yes { get; set; }
        public Nullable<bool> AccessToWorkPacket_No { get; set; }
        public string AccessToWorkPacket_DateProvided { get; set; }
        public string AccessToWorkPacket_Method { get; set; }
        public Nullable<bool> ServicesOffered { get; set; }
        public Nullable<bool> ServicesAccepted { get; set; }
        public Nullable<bool> ServicesDeclineded { get; set; }
        public Nullable<bool> Accommodations_HasNoCurrent { get; set; }
        public Nullable<bool> Accommodations_OfferedAndDeclined { get; set; }
        public string Accommodation_Description1 { get; set; }
        public string Accommodation_Implementation1 { get; set; }
        public string Accommodation_Frequency1 { get; set; }
        public string Accommodation_Description2 { get; set; }
        public string Accommodation_Implementation2 { get; set; }
        public string Accommodation_Frequency2 { get; set; }
        public string Accommodation_Description3 { get; set; }
        public string Accommodation_Implementation3 { get; set; }
        public string Accommodation_Frequency3 { get; set; }
        public string Accommodation_Description4 { get; set; }
        public string Accommodation_Implementation4 { get; set; }
        public string Accommodation_Frequency4 { get; set; }
        public Nullable<bool> Services_OfferedAndDeclined { get; set; }
        public string Services_ServiceProvided1 { get; set; }
        public string Services_Setting1 { get; set; }
        public string Services_Subject1 { get; set; }
        public string Services_Minutes1 { get; set; }
        public string Services_Frequency1 { get; set; }
        public Nullable<System.DateTime> Services_StartDate1 { get; set; }
        public Nullable<System.DateTime> Services_EndDate1 { get; set; }
        public string Services_ServiceProvided2 { get; set; }
        public string Services_Setting2 { get; set; }
        public string Services_Subject2 { get; set; }
        public string Services_Minutes2 { get; set; }
        public string Services_Frequency2 { get; set; }
        public Nullable<System.DateTime> Services_StartDate2 { get; set; }
        public Nullable<System.DateTime> Services_EndDate2 { get; set; }
        public Nullable<bool> Provider_WillContactPhone { get; set; }
        public Nullable<bool> Provider_WillContactPhone_Weekly { get; set; }
        public Nullable<bool> Provider_WillContactPhone_Biweekly { get; set; }
        public Nullable<bool> Provider_WillContactEmail { get; set; }
        public Nullable<bool> Provider_WillContactEmail_OnceWeek { get; set; }
        public Nullable<bool> Provider_WillContactEmail_TwiceWeek { get; set; }
        public Nullable<bool> Provider_WillContactEmail_Biweekly { get; set; }
        public Nullable<bool> Provider_ParentsContact { get; set; }
        public Nullable<bool> Goals_OfferedAndDeclined { get; set; }
        public string Goals_Number1 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Engagement1 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Feedback1 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Other1 { get; set; }
        public string Goals_Number2 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Engagement2 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Feedback2 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Other2 { get; set; }
        public string Goals_Number3 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Engagement3 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Feedback3 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Other3 { get; set; }
        public string Goals_Number4 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Engagement4 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Feedback4 { get; set; }
        public Nullable<bool> Goal_TrackProgress_Other4 { get; set; }
        public string ActivitiesOfferedToEnableAccess { get; set; }
        public string ProviderName { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public string Goals_Statement1 { get; set; }
        public string Goals_Statement2 { get; set; }
        public string Goals_Statement3 { get; set; }
        public string Goals_Statement4 { get; set; }
        public string District { get; set; }
    }
}
