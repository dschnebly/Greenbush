using GreenBushIEP.Models;
using System.Linq;

namespace GreenBushIEP.Helper
{
    public class VersionCompare
    {
        public static string GetVersionCount(tblUser user)
        {
            int updateCount = 0;

            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            user.LastVersionNumberSeen = (user.LastVersionNumberSeen == null ? user.LastVersionNumberSeen = fileVersion : user.LastVersionNumberSeen);


            System.Version lastSeen = new System.Version(user.LastVersionNumberSeen);
            System.Collections.Generic.List<tblVersionLog> listVersion = new IndividualizedEducationProgramEntities().tblVersionLogs.ToList();

            foreach (tblVersionLog version in listVersion)
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