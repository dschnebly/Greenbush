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
    
    public partial class tblFormManifestDeterm_TeamMembers
    {
        public int FormManifestDeterm_TeamMembersId { get; set; }
        public int StudentId { get; set; }
        public Nullable<int> FormManifestationDeterminiationId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Nullable<bool> Dissenting { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
    
        public virtual tblFormManifestationDeterminiation tblFormManifestationDeterminiation { get; set; }
    }
}
