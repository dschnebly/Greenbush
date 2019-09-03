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
    
    public partial class tblService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblService()
        {
            this.tblGoals = new HashSet<tblGoal>();
        }
    
        public int ServiceID { get; set; }
        public int IEPid { get; set; }
        public int SchoolYear { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string ServiceCode { get; set; }
        public int Frequency { get; set; }
        public byte DaysPerWeek { get; set; }
        public short Minutes { get; set; }
        public Nullable<int> ProviderID { get; set; }
        public string LocationCode { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<System.DateTime> FiledOn { get; set; }
        public bool Completed { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string USD { get; set; }
        public string BuildingID { get; set; }
    
        public virtual tblProvider tblProvider { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblGoal> tblGoals { get; set; }
    }
}
