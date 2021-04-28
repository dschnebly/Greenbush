using System;
using System.Data.Entity;

namespace GreenBushIEP.Models
{
    public class AuditLog : DbContext
    {
        private readonly IndividualizedEducationProgramEntities Context;
        private const string IEPBook = "_IEP_";

        private int StudentId { get; set; }
        private int IEPid { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public string SessionID { get; set; }


        private int ModifiedBy { get; set; }
        private tblAuditLog log { get; set; }

        public AuditLog(int studentId, int iepId, int modifiedBy, IndividualizedEducationProgramEntities db)
        {

            this.Context = db;
            this.StudentId = studentId;
            this.IEPid = iepId;
            this.ModifiedBy = modifiedBy;

            log = new tblAuditLog
            {
                IEPid = IEPid,
                UserID = StudentId,
                ModifiedBy = ModifiedBy,
                BookID = IEPBook,
                ColumnName = string.Empty,
                TableName = string.Empty,
                Value = "Intiated",
                SessionID = string.Empty,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now,
            };
        }

        public override int SaveChanges()
        {
            log.ColumnName = ColumnName;
            log.TableName = TableName;
            log.Value = Value;
            log.SessionID = SessionID;
            log.ModifiedBy = ModifiedBy;
            log.Update_Date = DateTime.Now;

            this.Context.tblAuditLogs.Add(log);
            return this.Context.SaveChanges();
        }
    }
}