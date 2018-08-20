using GreenBushIEP.Models;
using System.Linq;

namespace GreenBushIEP.Helper
{
    public class FindSupervisor
    {
        private static IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public static tblUser GetByRole(string roleId, tblUser user)
        {
            if (user.RoleID == roleId)
            {
                return user;
            }

            tblUser supervisor = (from o in db.tblOrganizationMappings
                                  join u in db.tblUsers on o.AdminID equals u.UserID
                                  where o.UserID == user.UserID
                                  select u).FirstOrDefault();

            return GetByRole(roleId, supervisor);
        }

    }
}