using GreenBushIEP.Models;
using System.Linq;

namespace GreenBushIEP.Helper
{
    public class FindSupervisor
    {
        private static readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        private const string owner = "1"; //level 5
        private const string mis = "2"; //level 4
        private const string admin = "3"; //level 3
        private const string teacher = "4"; //level 2
        private const string student = "5";
        private const string nurse = "6"; //level 1

        public static tblUser GetUSersMIS(tblUser user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.RoleID == mis || user.RoleID == owner)
            {
                return user;
            }

            tblUser supervisor = (from o in db.tblOrganizationMappings
                                  join u in db.tblUsers on o.AdminID equals u.UserID
                                  where o.UserID == user.UserID
                                  select u).FirstOrDefault();

            return GetUSersMIS(supervisor);
        }
    }
}