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
    
    public partial class tblFormIEPMeetingExcusal
    {
        public int FormIEPMeetingExcusalId { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public string ParentName { get; set; }
        public string SchoolRepresentative { get; set; }
        public string PositionOfRepresentative { get; set; }
        public string PositionOfMemberNotAttending { get; set; }
        public Nullable<bool> Services_Not_ModOrDisc_NonAttend { get; set; }
        public Nullable<bool> Services_Not_ModOrDisc_PartialAttend { get; set; }
        public string Services_Not_ModOrDisc_IssueDiscussed { get; set; }
        public Nullable<bool> Services_Not_ModOrDisc_Agree { get; set; }
        public Nullable<bool> Services_Not_ModOrDisc_Disagree { get; set; }
        public Nullable<bool> Services_MayBe_ModOrDisc_NonAttend { get; set; }
        public Nullable<bool> Services_MayBe_ModOrDisc_PartialAttend { get; set; }
        public string Services_MayBe_ModOrDisc_IssueDiscussed { get; set; }
        public Nullable<bool> Services_MayBe_ModOrDisc_Agree { get; set; }
        public Nullable<bool> Services_MayBe_ModOrDisc_Disagree { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
        public Nullable<System.DateTime> IEPDate { get; set; }
    }
}
