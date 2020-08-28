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
    
    public partial class vw_ServiceExport
    {
        public string AssignedUSD { get; set; }
        public string KIDSID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ServiceCode { get; set; }
        public byte Sessions { get; set; }
        public Nullable<int> Minutes { get; set; }
        public string FrequencyType { get; set; }
        public Nullable<System.DateTime> IEPStartDate { get; set; }
        public System.DateTime ServiceStartDate { get; set; }
        public Nullable<System.DateTime> ServiceEndDate { get; set; }
        public Nullable<System.DateTime> IEPEndDate { get; set; }
        public string RawFrequency { get; set; }
        public byte RawSessions { get; set; }
        public short RawMinutes { get; set; }
        public Nullable<int> MinuteMultiplier { get; set; }
        public string IepStatus { get; set; }
        public string ServiceType_Billable { get; set; }
        public string Location_Billable { get; set; }
        public string Export { get; set; }
        public bool isGBMedicaid { get; set; }
        public bool isTestDistrict { get; set; }
        public bool isTestUser { get; set; }
        public Nullable<bool> isValidKIDSID { get; set; }
    }
}
