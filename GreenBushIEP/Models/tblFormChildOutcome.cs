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
    
    public partial class tblFormChildOutcome
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblFormChildOutcome()
        {
            this.tblFormChildOutcomes_PersonsInvolved = new HashSet<tblFormChildOutcomes_PersonsInvolved>();
            this.tblFormChildOutcomes_SupportingEvidence = new HashSet<tblFormChildOutcomes_SupportingEvidence>();
        }
    
        public int FormChildOutcomeID { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public Nullable<System.DateTime> RatingDate { get; set; }
        public Nullable<System.DateTime> ServiceDate { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<long> KIDSID { get; set; }
        public Nullable<bool> FamilyInfo_ReceivedInTeamMeeting { get; set; }
        public Nullable<bool> FamilyInfo_CollectedSeperately { get; set; }
        public Nullable<bool> FamilyInfo_IncoporatedIntoAssessment { get; set; }
        public Nullable<bool> FamilyInfo_NotIncluded { get; set; }
        public Nullable<int> SocialEmotional_ShowAgeAppropriateBehavior { get; set; }
        public Nullable<bool> SocialEmotional_ShownNewBehaviors_Yes { get; set; }
        public Nullable<bool> SocialEmotional_ShownNewBehaviors_No { get; set; }
        public string SocialEmotional_ShownNewBehaviors_YesDescription { get; set; }
        public Nullable<int> AquireUsing_ShowAgeAppropriateBehavior { get; set; }
        public Nullable<bool> AquireUsing_ShownNewBehaviors_Yes { get; set; }
        public Nullable<bool> AquireUsing_ShownNewBehaviors_No { get; set; }
        public string AquireUsing_ShownNewBehaviors_YesDescription { get; set; }
        public Nullable<int> AppropriateAction_ShowAgeAppropriateBehavior { get; set; }
        public Nullable<bool> AppropriateAction_ShownNewBehaviors_Yes { get; set; }
        public Nullable<bool> AppropriateAction_ShownNewBehaviors_No { get; set; }
        public string AppropriateAction_ShownNewBehaviors_YesDescription { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblFormChildOutcomes_PersonsInvolved> tblFormChildOutcomes_PersonsInvolved { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblFormChildOutcomes_SupportingEvidence> tblFormChildOutcomes_SupportingEvidence { get; set; }
    }
}