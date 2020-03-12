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
    
    public partial class tblFormManifestationDeterminiation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblFormManifestationDeterminiation()
        {
            this.tblFormManifestDeterm_TeamMembers = new HashSet<tblFormManifestDeterm_TeamMembers>();
        }
    
        public int FormManifestationDeterminiationId { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public string StudentBehavior { get; set; }
        public string StudentIEP { get; set; }
        public string TeacherObservation { get; set; }
        public string ParentInformation { get; set; }
        public string OtherInformation { get; set; }
        public Nullable<bool> ConductCausedByDisability { get; set; }
        public Nullable<bool> IsManifestationOfDisability { get; set; }
        public Nullable<bool> StudentWillReturn { get; set; }
        public Nullable<bool> BehaviorPlan_IsManifest_Develop { get; set; }
        public Nullable<bool> ReviewBehaviorPlan { get; set; }
        public Nullable<bool> IsNotManifestationOfDisability { get; set; }
        public Nullable<bool> DisciplinaryRemovalMayOccur { get; set; }
        public Nullable<bool> BehaviorPlan_NotManifest_Develop { get; set; }
        public Nullable<bool> Attachments { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblFormManifestDeterm_TeamMembers> tblFormManifestDeterm_TeamMembers { get; set; }
    }
}