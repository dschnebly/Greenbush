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
    
    public partial class tblIEPReading
    {
        public int IEPReadingID { get; set; }
        public int IEPid { get; set; }
        public bool NoConcerns { get; set; }
        public bool ProgressTowardGenEd { get; set; }
        public Nullable<bool> InstructionalTier1 { get; set; }
        public Nullable<bool> InstructionalTier2 { get; set; }
        public Nullable<bool> InstructionalTier3 { get; set; }
        public bool AreaOfNeed { get; set; }
        public string LevelOfPerformance { get; set; }
        public string AreaOfNeedDescription { get; set; }
        public Nullable<bool> NeedMetByGoal { get; set; }
        public Nullable<bool> NeedMetByAccommodation { get; set; }
        public Nullable<bool> NeedMetByOther { get; set; }
        public string NeedMetByOtherDescription { get; set; }
        public string PLAAFP_Strengths { get; set; }
        public string PLAAFP_Concerns { get; set; }
        public string Notes { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
