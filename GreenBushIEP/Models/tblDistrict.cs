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
    
    public partial class tblDistrict
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblDistrict()
        {
            this.tblContacts = new HashSet<tblContact>();
        }
    
        public string USD { get; set; }
        public string DistrictName { get; set; }
        public Nullable<int> Active { get; set; }
        public bool DOC { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public bool isGBMedicaid { get; set; }
        public string KSDECode { get; set; }
        public bool isTest { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblContact> tblContacts { get; set; }
    }
}
