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
    
    public partial class tblAuditLog
    {
        public int AuditLogID { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> IEPid { get; set; }
        public string Value { get; set; }
        public int ModifiedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public string SessionID { get; set; }
        public string BookID { get; set; }
    }
}
