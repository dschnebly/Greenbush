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
    
    public partial class tblGoal
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblGoal()
        {
            this.tblServices = new HashSet<tblService>();
        }
    
        public int goalID { get; set; }
        public int IEPid { get; set; }
        public string Module { get; set; }
        public string Title { get; set; }
        public string AnnualGoal { get; set; }
        public bool hasSerivce { get; set; }
        public string Baseline { get; set; }
        public string StateStandards { get; set; }
        public string Progress_Quarter1 { get; set; }
        public string Progress_Quarter2 { get; set; }
        public string Progress_Quarter3 { get; set; }
        public string Progress_Quarter4 { get; set; }
        public System.DateTime ProgressDate_Quarter1 { get; set; }
        public System.DateTime ProgressDate_Quarter2 { get; set; }
        public System.DateTime ProgressDate_Quarter3 { get; set; }
        public System.DateTime ProgressDate_Quarter4 { get; set; }
        public string ProgressDescription_Quarter1 { get; set; }
        public string ProgressDescription_Quarter2 { get; set; }
        public string ProgressDescription_Quarter3 { get; set; }
        public string ProgressDescription_Quarter4 { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<int> EvaluationProcedures { get; set; }
        public bool Completed { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblService> tblServices { get; set; }
    }
}
