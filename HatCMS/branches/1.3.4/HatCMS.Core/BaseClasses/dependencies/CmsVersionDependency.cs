using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Hatfield.Web.Portal;

namespace HatCMS
{
    /// <summary>
    /// Checks if the HatCMS core library is a particular version. If not, returns a CmsDependecyMessage.Error.
    /// <remarks>Note: this checks the AssemblyVersion attribute - not the AssemblyFileVersion, nor the AssemblyInformationalVersion.
    /// <para>See http://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyinf for more information</para>
    /// </remarks>
    /// 
    /// </summary>
    public class CmsVersionDependency: CmsDependency
    {
        System.Version[] AcceptedVersions;
        string callerName;

        public CmsVersionDependency(string ModuleName, System.Version requiredHatCmsVersionSupported)
        {
            callerName = ModuleName;
            AcceptedVersions = new Version[] { requiredHatCmsVersionSupported };
        }

        public CmsVersionDependency(string ModuleName, System.Version[] HatCmsVersionsSupported)
        {
            callerName = ModuleName;
            AcceptedVersions = HatCmsVersionsSupported;
        }
        
        public override CmsDependencyMessage[] ValidateDependency()
        {
            System.Version hatCmsVersion = CmsContext.currentHatCMSCoreVersion;
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            List<string> reqVersionStrings = new List<string>();
            foreach (System.Version ver in AcceptedVersions)
            {
                int levelToCompare = ver.ToString().Split(new char[] { '.' }).Length;
                string verStr = ver.ToString(levelToCompare);
                reqVersionStrings.Add(verStr);
                if (hatCmsVersion.ToString(levelToCompare).CompareTo(verStr) == 0)
                    return ret.ToArray(); // no errors if this version is a match
            }

            ret.Add(CmsDependencyMessage.Error(callerName + " requires HatCMS version " + StringUtils.Join(", ", " or ", reqVersionStrings.ToArray())));
            return ret.ToArray();
        }

        public override string GetContentHash()
        {
            StringBuilder hash = new StringBuilder(callerName);
            foreach (System.Version v in AcceptedVersions)
            {
                hash.Append(v.ToString());
            }
            return hash.ToString();
        }
    }
}
