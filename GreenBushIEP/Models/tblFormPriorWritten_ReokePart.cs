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
    
    public partial class tblFormPriorWritten_ReokePart
    {
        public int FormPriorWritten_ReokePartId { get; set; }
        public int StudentId { get; set; }
        public string ParentName { get; set; }
        public Nullable<System.DateTime> FormDate { get; set; }
        public Nullable<System.DateTime> SubmitDate { get; set; }
        public string ServicesRevoked { get; set; }
        public Nullable<bool> ActionTaken { get; set; }
        public Nullable<System.DateTime> ActionTakenEndDate { get; set; }
        public string ActionTakenDescription { get; set; }
        public Nullable<bool> ActionRefused { get; set; }
        public string ActionRefusedDescription { get; set; }
        public string OptionsConsidered { get; set; }
        public string DataUsed { get; set; }
        public string OtherFactors { get; set; }
        public string DeliveriedByWho { get; set; }
        public Nullable<bool> DelieveredByHand { get; set; }
        public Nullable<bool> DelieveredByMail { get; set; }
        public Nullable<bool> DelieveredByOther { get; set; }
        public string DelieveredByOtherDesc { get; set; }
        public string DeliveriedTo { get; set; }
        public Nullable<System.DateTime> DelieveredDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Create_Date { get; set; }
        public Nullable<System.DateTime> Update_Date { get; set; }
    }
}