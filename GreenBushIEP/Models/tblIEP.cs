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
    
    public partial class tblIEP
    {
        public int IEPid { get; set; }
        public int UserID { get; set; }
        public string IepStatus { get; set; }
        public Nullable<System.DateTime> begin_date { get; set; }
        public bool Amendment { get; set; }
        public Nullable<int> AmendingIEPid { get; set; }
        public System.DateTime Create_Date { get; set; }
        public string StateAssessment { get; set; }
        public string StateAssessmentDescription { get; set; }
        public Nullable<int> IEPHealthID { get; set; }
        public Nullable<int> IEPMotorID { get; set; }
        public Nullable<int> IEPCommunicationID { get; set; }
        public Nullable<int> IEPSocialID { get; set; }
        public Nullable<int> IEPIntelligenceID { get; set; }
        public Nullable<int> IEPAcademicID { get; set; }
        public Nullable<int> IEPReadingID { get; set; }
        public Nullable<int> IEPMathID { get; set; }
        public Nullable<int> IEPWrittenID { get; set; }
        public Nullable<System.DateTime> MeetingDate { get; set; }
        public Nullable<System.DateTime> FiledOn { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<System.DateTime> end_Date { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> OriginalIEPid { get; set; }
        public string StatusCode { get; set; }
    }
}
