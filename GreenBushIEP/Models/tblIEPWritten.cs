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
    
    public partial class tblIEPWritten
    {
        public int IEPWrittenID { get; set; }
        public int IEPid { get; set; }
        public bool NoConcerns { get; set; }
        public bool ProgressTowardGenEd { get; set; }
        public Nullable<bool> InstructionalTier1 { get; set; }
        public Nullable<bool> InstructionalTier2 { get; set; }
        public Nullable<bool> InstructionalTier3 { get; set; }
        public bool AreaOfNeed { get; set; }
        public string LevelOfPerformance { get; set; }
        public string AreaOfNeedDescription { get; set; }
        public Nullable<int> MeetNeedBy { get; set; }
        public string MeetNeedByOtherDescription { get; set; }
    }
}
