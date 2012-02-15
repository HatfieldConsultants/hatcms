using System;
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
    /// A dependecy that requires that a template exists on the file system.
    /// </summary>
    public class CmsTemplateDependency: CmsDependency
    {
        private string templateName = "";
        private string dependencySource ="";

        public CmsTemplateDependency(string TemplateName, string DependencySource)
        {
            templateName = TemplateName;
            dependencySource = DependencySource;
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            return testTemplate(templateName, dependencySource);
        }

        public override string GetContentHash()
        {
            return templateName.Trim().ToLower()+dependencySource.Trim().ToLower();
        }

        public static CmsDependencyMessage[] testTemplate(string _templateName, string _dependencySource)
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();

            try
            {
                if (!CmsContext.currentPage.TemplateEngine.templateExists(_templateName))
                {
                    ret.Add(CmsDependencyMessage.Error("Error: the template \"" + _templateName + "\" was NOT found!! (source: " + _dependencySource + ") "));
                }
            }
            catch (Exception e)
            {
                ret.Add(CmsDependencyMessage.Error("Error validating template  '" + _templateName + "' : " + e.Message));
            }

            return ret.ToArray();

        } // testTemplate
    }
}
