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
    
    public partial class tblRolePermission
    {
        public int RolePermissionID { get; set; }
        public int RoleID { get; set; }
        public string PermissionID { get; set; }
    
        public virtual tblPermission tblPermission { get; set; }
    }
}
