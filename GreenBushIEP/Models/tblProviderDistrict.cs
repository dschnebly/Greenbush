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
    
    public partial class tblProviderDistrict
    {
        public int ProviderDistrictID { get; set; }
        public int ProviderID { get; set; }
        public string USD { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
    
        public virtual tblProvider tblProvider { get; set; }
    }
}
