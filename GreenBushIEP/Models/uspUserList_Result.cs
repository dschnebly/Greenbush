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
    
    public partial class uspUserList_Result
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string RoleID { get; set; }
        public Nullable<bool> hasIEP { get; set; }
        public Nullable<long> KIDSID { get; set; }
        public Nullable<bool> isAssgined { get; set; }
        public string StatusCode { get; set; }
        public int StatusActive { get; set; }
    }
}
