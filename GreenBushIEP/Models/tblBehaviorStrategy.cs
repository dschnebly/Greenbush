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
    
    public partial class tblBehaviorStrategy
    {
        public int BehaviorStrategyID { get; set; }
        public int IEPid { get; set; }
        public int BehaviorID { get; set; }
        public int BehaviorStrategyTypeID { get; set; }
        public string OtherDescription { get; set; }
        public System.DateTime Create_Date { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public System.DateTime Update_Date { get; set; }
    }
}
