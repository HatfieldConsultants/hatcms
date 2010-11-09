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
        public CmsTemplateDependency(string TemplateName)
        {
            templateName = TemplateName;
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            return testTemplate(templateName, System.Web.HttpContext.Current);
        }

        public override string GetContentHash()
        {
            return templateName.Trim().ToLower();
        }

        public static CmsDependencyMessage[] testTemplate(string _templateName, System.Web.HttpContext Context)
        {
            List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
            
            try
            {                
                if (!CmsContext.currentPage.TemplateEngine.templateExists(_templateName))
                {
                    ret.Add(CmsDependencyMessage.Error("Error: template was NOT found!! (\"" + _templateName + "\") "));
                }
                else
                {
                    ret.Add(CmsDependencyMessage.Status("Template was found (\"" + _templateName + "\""));
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
