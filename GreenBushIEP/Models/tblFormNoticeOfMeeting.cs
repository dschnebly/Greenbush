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
    
    public partial class tblFormNoticeOfMeeting
    {
        public int FormNoticeOfMeetingId { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public string Parentname { get; set; }
        public string OnBehalf { get; set; }
        public string Address { get; set; }
        public string CityStateZip { get; set; }
        public string ProposedMeetingInfo { get; set; }
        public Nullable<bool> MeetingToReviewEvaluation { get; set; }
        public Nullable<bool> DevelopIEP { get; set; }
        public Nullable<bool> DiscussIEPChanges { get; set; }
        public Nullable<bool> AnnualIEPReview { get; set; }
        public Nullable<bool> TransitionAssesment { get; set; }
        public Nullable<bool> Other { get; set; }
        public string SpecialExpertise1 { get; set; }
        public string SpecialExpertise2 { get; set; }
        public string SpecialExpertise3 { get; set; }
        public string SpecialExpertise4 { get; set; }
        public string SpecialExpertise5 { get; set; }
        public string SpecialExpertise6 { get; set; }
        public string AgencyStaff { get; set; }
        public string SchoolContactName { get; set; }
        public string SchoolContactPhone { get; set; }
        public string DeliveriedByWho { get; set; }
        public string DeliveriedTo { get; set; }
        public Nullable<bool> DelieveredByHand { get; set; }
        public Nullable<bool> DelieveredByMail { get; set; }
        public Nullable<bool> DelieveredByOther { get; set; }
        public string DelieveredByOtherDesc { get; set; }
        public Nullable<bool> PlanToAttend { get; set; }
        public Nullable<bool> RescheduleMeeting { get; set; }
        public Nullable<bool> AvaliableToAttend_flag { get; set; }
        public string AvailableToAttend_desc { get; set; }
        public Nullable<bool> WaiveRightToNotice { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
        public Nullable<System.DateTime> DelieveredDate { get; set; }
    }
}
