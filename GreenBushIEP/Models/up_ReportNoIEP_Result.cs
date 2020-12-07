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
    
    public partial class up_ReportNoIEP_Result
    {
        public int StudentId { get; set; }
        public string StudentLastName { get; set; }
        public string StudentFirstName { get; set; }
        public long KIDSID { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string NeighborhoodBuildingID { get; set; }
        public string AssignedUSD { get; set; }
        public string Grade { get; set; }
        public string Primary_DisabilityCode { get; set; }
        public string Secondary_DisabilityCode { get; set; }
        public bool ClaimingCode { get; set; }
        public string County { get; set; }
        public string PlacementCode { get; set; }
        public string StatusCode { get; set; }
        public Nullable<System.DateTime> ExitDate { get; set; }
        public Nullable<System.DateTime> InitialIEPDate { get; set; }
        public Nullable<System.DateTime> InitialEvalConsentSigned { get; set; }
        public Nullable<System.DateTime> InitialEvalDetermination { get; set; }
        public Nullable<System.DateTime> ReEvalConsentSigned { get; set; }
        public string isGifted { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ReEvalCompleted { get; set; }
        public string USD { get; set; }
        public string BuildingName { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
