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
    
    public partial class tblReferralRequest
    {
        public int ReferalRequestID { get; set; }
        public int UserID_Requster { get; set; }
        public string UserID_District { get; set; }
        public int ReferralID { get; set; }
        public bool Complete { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public Nullable<System.DateTime> Submit_Date { get; set; }
    
        public virtual tblReferralInfo tblReferralInfo { get; set; }
    }
}
