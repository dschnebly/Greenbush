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
    
    public partial class up_ReportProviderCaseload_Result
    {
        public int ServiceID { get; set; }
        public int SchoolYear { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string ServiceType { get; set; }
        public int Frequency { get; set; }
        public byte DaysPerWeek { get; set; }
        public short Minutes { get; set; }
        public string Location { get; set; }
        public string GoalTitle { get; set; }
        public Nullable<int> goalID { get; set; }
        public string ProviderName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string BuildingID { get; set; }
        public Nullable<int> ProviderID { get; set; }
        public string BuildingName { get; set; }
        public string USD { get; set; }
    }
}
