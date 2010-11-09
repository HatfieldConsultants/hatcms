using System;
using System.IO;
using System.Collections.Generic;
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
    /// A dependecy that requires that a directory exist on the file system, and be writable by the webserver .
    /// </summary>
    public class CmsWritableDirectoryDependency: CmsDependency
    {
        public string DirectoryPath = "";

        public CmsWritableDirectoryDependency(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        public static CmsWritableDirectoryDependency UnderAppPath(string pathUnderAppPath)
        {
            string fullFilePath = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + pathUnderAppPath);
            return new CmsWritableDirectoryDependency(fullFilePath);
        }

        public override string GetContentHash()
        {
            return DirectoryPath.Trim().ToLower();
        }

        
        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                ret.Add(CmsDependencyMessage.Error("Error: required directory was NOT found!! (\"" + DirectoryPath + "\") "));
            }
            else
            {
                try
                {
                    if (!DirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        DirectoryPath += Path.DirectorySeparatorChar;
                    string testFilename = DirectoryPath + Guid.NewGuid().ToString() + ".txt";
                    string[] testFileContents = new string[] { "Test file to check if the folder is writable" };
                    try
                    {
                        System.IO.File.WriteAllLines(testFilename, testFileContents);                        
                    }
                    finally
                    {
                        System.IO.File.Delete(testFilename);
                    }
                }
                catch (Exception ex)
                {
                    ret.Add(CmsDependencyMessage.Error("Error: directory \"" + DirectoryPath + "\" is not writable by the web application!. "));
                }
            }

            return ret.ToArray();
        }
    }
}
