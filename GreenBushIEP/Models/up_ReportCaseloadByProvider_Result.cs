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
    
    public partial class up_ReportCaseloadByProvider_Result
    {
        public string ProviderName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int ProviderID { get; set; }
        public string BuildingName { get; set; }
        public string USD { get; set; }
        public string PrimaryExceptionality { get; set; }
        public string SecondaryExceptionality { get; set; }
        public bool isGifted { get; set; }
        public Nullable<System.DateTime> MeetingDate { get; set; }
        public Nullable<System.DateTime> ReEvalCompleted { get; set; }
    }
}