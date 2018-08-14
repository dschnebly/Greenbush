﻿using GreenBushIEP.Models;
using System.Linq;

namespace GreenBushIEP.Helper
{
    public class VersionCompare
    {
        public static string GetVersionCount(tblUser user)
        {
            int updateCount = 0;
            System.Version lastSeen = new System.Version(user.LastVersionNumberSeen);
            var listVersion = new IndividualizedEducationProgramEntities().tblVersionLogs.ToList();

            foreach (var version in listVersion)
            {
                System.Version versioning = new System.Version(version.VersionNumber);
                if (versioning.CompareTo(lastSeen) > 0)
                {
                    updateCount += 1;
                }
            }

            if (updateCount == 0)
            {
                return "";
            }
            else
            {
                return updateCount.ToString();
            }
        }
    }
}