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
    
    public partial class tblBehaviorHypothesi
    {
        public int BehaviorHypothesisID { get; set; }
        public int IEPid { get; set; }
        public int BehaviorID { get; set; }
        public int BehaviorHypothesisTypeID { get; set; }
        public string OtherDescription { get; set; }
        public System.DateTime Create_Date { get; set; }
    }
}
