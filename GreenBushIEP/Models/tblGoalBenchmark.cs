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
    
    public partial class tblGoalBenchmark
    {
        public int goalBenchmarkID { get; set; }
        public int goalID { get; set; }
        public int Method { get; set; }
        public string ObjectiveBenchmark { get; set; }
        public bool TransitionActivity { get; set; }
        public string Progress_Quarter1 { get; set; }
        public string Progress_Quarter2 { get; set; }
        public string Progress_Quarter3 { get; set; }
        public string Progress_Quarter4 { get; set; }
        public Nullable<System.DateTime> ProgressDate_Quarter1 { get; set; }
        public Nullable<System.DateTime> ProgressDate_Quarter2 { get; set; }
        public Nullable<System.DateTime> ProgressDate_Quarter3 { get; set; }
        public Nullable<System.DateTime> ProgressDate_Quarter4 { get; set; }
        public string ProgressDescription_Quarter1 { get; set; }
        public string ProgressDescription_Quarter2 { get; set; }
        public string ProgressDescription_Quarter3 { get; set; }
        public string ProgressDescription_Quarter4 { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
    }
}
