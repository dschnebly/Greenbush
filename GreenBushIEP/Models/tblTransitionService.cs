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
    
    public partial class tblTransitionService
    {
        public int TransitionServiceID { get; set; }
        public int TransitionID { get; set; }
        public int IEPid { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public string ServiceType { get; set; }
        public string ServiceDescription { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
