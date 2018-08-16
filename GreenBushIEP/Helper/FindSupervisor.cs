using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GreenBushIEP.Helper
{
    public class FindSupervisor
    {
        private static IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public static tblUser GetByRole(string roleId, tblUser user)
        {
            tblUser supervisor = (from o in db.tblOrganizationMappings
                                  join u in db.tblUsers on o.AdminID equals u.UserID
                                  where o.UserID == user.UserID
                                  select u).FirstOrDefault();

            if (supervisor.RoleID != "2")
            {
                supervisor = GetByRole(roleId, supervisor);
            }

            return supervisor;
        }

    }
}