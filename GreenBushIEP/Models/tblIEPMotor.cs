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
    
    public partial class tblIEPMotor
    {
        public int IEPMotorID { get; set; }
        public int IEPid { get; set; }
        public bool NoConcerns { get; set; }
        public bool ProgressTowardGenEd { get; set; }
        public bool Needs { get; set; }
        public int Participation { get; set; }
        public string LevelOfPerformance { get; set; }
        public string AreaOfNeedDescription { get; set; }
        public Nullable<bool> AreaOfNeed { get; set; }
        public Nullable<bool> NeedMetByGoal { get; set; }
        public Nullable<bool> NeedMetByAccommodation { get; set; }
        public Nullable<bool> NeedMetByOther { get; set; }
        public string NeedMetByOtherDescription { get; set; }
        public string PLAAFP_Strengths { get; set; }
        public string PLAAFP_Concerns { get; set; }
        public bool Completed { get; set; }
    }
}
