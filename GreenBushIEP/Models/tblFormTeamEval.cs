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
    
    public partial class tblFormTeamEval
    {
        public int FormTeamEvalId { get; set; }
        public int StudentId { get; set; }
        public string ReasonReferral { get; set; }
        public string MedicalFindings { get; set; }
        public string Hearing { get; set; }
        public string Vision { get; set; }
        public string RelevantBehavior { get; set; }
        public string InfoReview { get; set; }
        public string ParentInterview { get; set; }
        public string TestData { get; set; }
        public string IntellectualDevelopment { get; set; }
        public string Peformance { get; set; }
        public string Disadvantage { get; set; }
        public string DisadvantageExplain { get; set; }
        public string Regulations { get; set; }
        public Nullable<bool> Regulation_flag { get; set; }
        public string SustainedResources { get; set; }
        public Nullable<bool> SustainedResources_flag { get; set; }
        public string Strengths { get; set; }
        public string AreaOfConcern { get; set; }
        public string GeneralEducationExpectations { get; set; }
        public string Tried { get; set; }
        public string NotWorked { get; set; }
        public string GeneralDirection { get; set; }
        public string MeetEligibility { get; set; }
        public string ResourcesNeeded { get; set; }
        public string SpecificNeeds { get; set; }
        public string ConvergentData { get; set; }
        public Nullable<bool> ConvergentData_flag { get; set; }
        public string ListSources { get; set; }
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
    }
}
