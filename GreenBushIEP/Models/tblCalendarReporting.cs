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
    
    public partial class tblCalendarReporting
    {
        public int calendarReportingID { get; set; }
        public string USD { get; set; }
        public string BuildingID { get; set; }
        public int SchoolYear { get; set; }
        public int DaysPerWeek { get; set; }
        public int TotalDays { get; set; }
        public int TotalWeeks { get; set; }
        public int MinutesPerDay { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
    }
}
