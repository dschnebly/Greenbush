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
    
    public partial class tblFormRevokeConsentAll
    {
        public int FormRevokeConsentAllId { get; set; }
        public int StudentId { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public string AuthorityName { get; set; }
        public Nullable<bool> OnBehalfOfStudent { get; set; }
        public Nullable<bool> OnMyOwnBehalf { get; set; }
        public Nullable<System.DateTime> RevokeConsentDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
    }
}
