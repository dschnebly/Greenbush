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
    
    public partial class tblBehavior
    {
        public int BehaviorID { get; set; }
        public int IEPid { get; set; }
        public string StrengthMotivator { get; set; }
        public string BehaviorConcern { get; set; }
        public string Crisis_Escalation { get; set; }
        public string Crisis_Description { get; set; }
        public string Crisis_Implementation { get; set; }
        public string Crisis_Other { get; set; }
        public string ReviewedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public bool Completed { get; set; }
    }
}
