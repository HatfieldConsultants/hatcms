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
        private string controlPath = "";
        /// <summary>
        /// set to DateTime.MinValue if no Last Modified date check is done.
        /// </summary>
        public DateTime FileShouldBeLastModifiedAfter = DateTime.MinValue;

        public CmsControlDependency(string ControlPathUnderControlsDir)
        {
            controlPath = ControlPathUnderControlsDir;
        }

        public CmsControlDependency(string ControlPathUnderControlsDir, DateTime fileshouldbelastmodifiedafter)
        {
            controlPath = ControlPathUnderControlsDir;
            FileShouldBeLastModifiedAfter = fileshouldbelastmodifiedafter;
        }

        public override string GetContentHash()
        {
            return controlPath.Trim().ToLower() + FileShouldBeLastModifiedAfter.Ticks.ToString();
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
                if (controlPath.EndsWith(".ascx", StringComparison.CurrentCultureIgnoreCase))
                {
                    controlPath = controlPath.Substring(0, controlPath.Length - (".ascx".Length));
                }

                string controlPathWithoutOption = controlPath.Split(new char[] { ' ' })[0];
                if (CmsContext.currentPage.TemplateEngine.controlExists(controlPathWithoutOption))
                {
                    ret.Add(CmsDependencyMessage.Status("CMS Control was found: '" + controlPathWithoutOption + "'"));
                    DateTime lastModified = CmsContext.currentPage.TemplateEngine.getControlLastModifiedDate(controlPathWithoutOption);
                    if (lastModified != DateTime.MinValue && lastModified.Ticks < FileShouldBeLastModifiedAfter.Ticks)
                        ret.Add(CmsDependencyMessage.Error("CMS Control \"" + controlPathWithoutOption + "\" should have a last modified date after " + FileShouldBeLastModifiedAfter.ToString("d MMM yyyy hh:mm")));
                }
                else
                    ret.Add(CmsDependencyMessage.Error("CMS Control was not found: '" + controlPath + "'"));

            }
            catch (Exception ex)
            {
                ret.Add(CmsDependencyMessage.Error("CMS Control could not be loaded: '" + controlPath + "'"));
            }
            return ret.ToArray();
        }
    }
}
