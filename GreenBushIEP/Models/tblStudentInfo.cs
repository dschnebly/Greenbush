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
    
    public partial class tblStudentInfo
    {
        public int UserID { get; set; }
        public long KIDSID { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public string USD { get; set; }
        public string BuildingID { get; set; }
        public string Status { get; set; }
        public string StudentLanguage { get; set; }
        public string ParentLanguage { get; set; }
        public string Ethicity { get; set; }
        public string Race { get; set; }
        public string FundSource { get; set; }
        public Nullable<bool> FullDayKG { get; set; }
        public string Gender { get; set; }
        public string NeighborhoodBuildingID { get; set; }
        public string AssignedUSD { get; set; }
        public string County { get; set; }
        public Nullable<int> Grade { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public string Primary_DisabilityCode { get; set; }
        public string Secondary_DisabilityCode { get; set; }
        public bool ClaimingCode { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string PlacementCode { get; set; }
        public Nullable<bool> StatusCode { get; set; }
        public Nullable<System.DateTime> ExitDate { get; set; }
        public Nullable<System.DateTime> InitialIEPDate { get; set; }
        public Nullable<System.DateTime> InitialEvalConsentSigned { get; set; }
        public Nullable<System.DateTime> InitialEvalDetermination { get; set; }
        public Nullable<System.DateTime> ReEvalConsentSigned { get; set; }
    }
}
