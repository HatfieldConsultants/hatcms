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
using System.Collections.Generic;

namespace HatCMS
{
    /// <summary>
    /// A dependecy that requires that a file exists on the file system.
    /// </summary>
    public class CmsFileDependency : CmsDependency
    {
        private string FullFilePath = "";
        private ExistsMode _mode = ExistsMode.MustExist;

        /// <summary>
        /// set to DateTime.MinValue if no Last Modified date check is done.
        /// </summary>
        private DateTime FileShouldBeLastModifiedAfter = DateTime.MinValue;

        public CmsFileDependency(string fullfilepath)
        {
            FullFilePath = fullfilepath;
            FileShouldBeLastModifiedAfter = DateTime.MinValue;
            _mode = ExistsMode.MustExist;
        }

        public CmsFileDependency(string fullfilepath, ExistsMode existsmode)
        {
            FullFilePath = fullfilepath;
            FileShouldBeLastModifiedAfter = DateTime.MinValue;
            _mode = existsmode;
        }

        public CmsFileDependency(string fullfilepath, DateTime fileshouldbelastmodifiedafter)
        {
            FullFilePath = fullfilepath;
            FileShouldBeLastModifiedAfter = fileshouldbelastmodifiedafter;
            _mode = ExistsMode.MustExist;
        }

        public CmsFileDependency(string fullfilepath, DateTime fileshouldbelastmodifiedafter, ExistsMode existsmode)
        {
            FullFilePath = fullfilepath;
            FileShouldBeLastModifiedAfter = fileshouldbelastmodifiedafter;
            _mode = existsmode;
        }

        public static CmsFileDependency UnderAppPath(string pathUnderAppPath)
        {
            string fullFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + pathUnderAppPath);
            return new CmsFileDependency(fullFilePath);
        }

        public static CmsFileDependency UnderAppPath(string pathUnderAppPath, ExistsMode existsmode)
        {
            string fullFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + pathUnderAppPath);
            return new CmsFileDependency(fullFilePath, existsmode);
        }

        public static CmsFileDependency UnderAppPath(string pathUnderAppPath, DateTime modifiedAfter)
        {
            string fullFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + pathUnderAppPath);
            return new CmsFileDependency(fullFilePath, modifiedAfter);
        }

        public static CmsFileDependency UnderAppPath(string pathUnderAppPath, DateTime modifiedAfter, ExistsMode existsmode)
        {
            string fullFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + pathUnderAppPath);
            return new CmsFileDependency(fullFilePath, modifiedAfter, existsmode);
        }

        public override string GetContentHash()
        {
            return FullFilePath.Trim().ToLower()+_mode.ToString();
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            try
            {
                // -- .aspx files must exist in the CmsConfig.URLsToNotRemap array
                if (String.Compare(Path.GetExtension(FullFilePath), ".aspx", true) == 0)
                {
                    string appPathFullDir = System.Web.Hosting.HostingEnvironment.MapPath(CmsContext.ApplicationPath);
                    string relPath = Hatfield.Web.Portal.PathUtils.RelativePathTo(appPathFullDir, FullFilePath);
                    if (relPath.StartsWith(@"\"))
                        relPath = relPath.Substring(1); // remove first slash
                    string url = relPath.Replace(@"\", @"/"); // switch slashes

#if ! DEBUG
                    if (String.Compare(url, "setup/default.aspx", true) == 0)
                    {
                        ret.Add(CmsDependencyMessage.Error("'setup/default.aspx' should be taken out of the \"URLsToNotRemap\" configuration entry on production sites."));
                        return ret.ToArray();
                    }
#endif

                    if (String.Compare(url, "default.aspx", true) != 0 &&  
                        Hatfield.Web.Portal.StringUtils.IndexOf(CmsConfig.URLsToNotRemap, url, StringComparison.CurrentCultureIgnoreCase) < 0)
                        ret.Add(CmsDependencyMessage.Error("\"" + url + "\" is a required ASPX page, and should be listed in the \"URLsToNotRemap\" configuration entry."));
                }

                if (File.Exists(FullFilePath))
                {
                    if (_mode == ExistsMode.MustNotExist)
                    {
                        ret.Add(CmsDependencyMessage.Error("File must NOT exist: \"" + FullFilePath + "\""));
                    }
                    else if (FileShouldBeLastModifiedAfter.Ticks != DateTime.MinValue.Ticks)
                    {
                        FileInfo fi = new FileInfo(FullFilePath);
                        if (fi.LastWriteTime.Ticks < FileShouldBeLastModifiedAfter.Ticks)
                            ret.Add(CmsDependencyMessage.Error("\"" + FullFilePath + "\" should have a last modified date after " + FileShouldBeLastModifiedAfter.ToString("d MMM yyyy hh:mm")));
                    }
                }
                else
                {
                    if (_mode == ExistsMode.MustExist)
                        ret.Add(CmsDependencyMessage.Error("File does not exist: \""+FullFilePath+"\""));
                }
            }
            catch(Exception ex)
            {
                ret.Add(CmsDependencyMessage.Error(ex));
            }
            return ret.ToArray();
        }
    }
}
