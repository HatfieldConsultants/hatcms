using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    /// <summary>
    /// A dependecy that requires that a directory does NOT exist on the file system.
    /// </summary>
    public class CmsDirectoryDoesNotExistDependency : CmsDependency
    {
        private string FullPathToDir;

        public CmsDirectoryDoesNotExistDependency(string fullPathToDir)
        {
            FullPathToDir = fullPathToDir;
        }

        public static CmsDirectoryDoesNotExistDependency UnderAppPath(string pathUnderAppPath)
        {
            string fullFilePath = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + pathUnderAppPath);
            return new CmsDirectoryDoesNotExistDependency(fullFilePath);
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            if (Directory.Exists(FullPathToDir))
                return new CmsDependencyMessage[] { CmsDependencyMessage.Error("The directory \"" + FullPathToDir + "\" should NOT exist") };
            else
                return new CmsDependencyMessage[0];
        }

        public override string GetContentHash()
        {
            return "DirectoryDoesNotExist" + FullPathToDir.ToLower();
        }
    }
}
