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
    
    public partial class tblTransitionAssessment
    {
        public int TransitionAessementID { get; set; }
        public int TransitionID { get; set; }
        public int IEPid { get; set; }
        public string Narrative { get; set; }
        public Nullable<System.DateTime> CompletedOn { get; set; }
        public string Performance { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
    }
}
