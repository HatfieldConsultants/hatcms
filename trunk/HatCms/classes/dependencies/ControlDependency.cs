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
    /// A dependecy that requires that a CMS control item must be present on the file system.
    /// </summary>
    public class CmsControlDependency: CmsDependency
    {
        private string controlNameOrPath = "";
        private ExistsMode existsMode;
        /// <summary>
        /// set to DateTime.MinValue if no Last Modified date check is done.
        /// </summary>
        public DateTime FileShouldBeLastModifiedAfter = DateTime.MinValue;

        public CmsControlDependency(CmsControlDefinition controlDefinition)
        {
            controlNameOrPath = controlDefinition.ControlNameOrPath;
            existsMode = ExistsMode.MustExist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ControlPathUnderControlsDirWithoutFileExtension">        
        /// The control path never has a filename extension (ie "_system/login" is a valid control path, while "_system/login.ascx" is invalid).
        /// </param>
        public CmsControlDependency(string ControlPathUnderControlsDirWithoutFileExtension, ExistsMode existsMode)
        {
            controlNameOrPath = ControlPathUnderControlsDirWithoutFileExtension;
            this.existsMode = existsMode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ControlPathUnderControlsDirWithoutFileExtension">        
        /// The control path never has a filename extension (ie "_system/login" is a valid control path, while "_system/login.ascx" is invalid).
        /// </param>
        public CmsControlDependency(string ControlNameOrPathUnderControlsDir)
        {
            controlNameOrPath = ControlNameOrPathUnderControlsDir;
        }

        public CmsControlDependency(string ControlNameOrPathUnderControlsDir, DateTime fileshouldbelastmodifiedafter)
        {
            controlNameOrPath = ControlNameOrPathUnderControlsDir;
            FileShouldBeLastModifiedAfter = fileshouldbelastmodifiedafter;
        }

        public override string GetContentHash()
        {
            return controlNameOrPath.Trim().ToLower() + FileShouldBeLastModifiedAfter.Ticks.ToString();
        }

        public static CmsControlDependency UnderControlDir(string ControlPathUnderControlsDir, DateTime modifiedAfter)
        {
            return new CmsControlDependency(ControlPathUnderControlsDir, modifiedAfter);
        }


        public override CmsDependencyMessage[] ValidateDependency()
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            try
            {
                // -- remove the .ascx filename extension if one was provided.
                if (controlNameOrPath.EndsWith(".ascx", StringComparison.CurrentCultureIgnoreCase))
                {
                    controlNameOrPath = controlNameOrPath.Substring(0, controlNameOrPath.Length - (".ascx".Length));
                }

                string controlPathWithoutOption = controlNameOrPath.Split(new char[] { ' ' })[0];
                if (CmsContext.currentPage.TemplateEngine.controlExists(controlPathWithoutOption))
                {
                    if (existsMode == ExistsMode.MustNotExist)
                        ret.Add(CmsDependencyMessage.Error("CMS Control should not exist: '" + controlPathWithoutOption + "'"));
                    else if (existsMode == ExistsMode.MustExist)
                    {
                        DateTime lastModified = CmsContext.currentPage.TemplateEngine.getControlLastModifiedDate(controlPathWithoutOption);
                        if (lastModified != DateTime.MinValue && lastModified.Ticks < FileShouldBeLastModifiedAfter.Ticks)
                            ret.Add(CmsDependencyMessage.Error("CMS Control \"" + controlPathWithoutOption + "\" should have a last modified date after " + FileShouldBeLastModifiedAfter.ToString("d MMM yyyy hh:mm")));
                    }
                }
                else if (existsMode == ExistsMode.MustExist)
                    ret.Add(CmsDependencyMessage.Error("CMS Control was not found: '" + controlNameOrPath + "'"));

            }
            catch (Exception ex)
            {
                ret.Add(CmsDependencyMessage.Error("CMS Control could not be found: '" + controlNameOrPath + "'"));
            }
            return ret.ToArray();
        }
    }
}
