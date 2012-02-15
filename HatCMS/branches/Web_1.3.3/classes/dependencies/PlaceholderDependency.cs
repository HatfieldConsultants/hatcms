using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;

namespace HatCMS
{
    public class CmsPlaceholderDependency: CmsDependency
    {
        private string placeholderType;
        private string templateName;

        private ExistsMode existsMode;

        public CmsPlaceholderDependency(string PlaceholderTypeToFind, string PlaceholderIsFoundInTemplateName)
        {
            this.placeholderType = PlaceholderTypeToFind;
            this.templateName = PlaceholderIsFoundInTemplateName;
            this.existsMode =  ExistsMode.MustExist;
        }

        public CmsPlaceholderDependency(string PlaceholderTypeToFind, string PlaceholderIsFoundInTemplateName, ExistsMode existsMode)
        {
            this.placeholderType = PlaceholderTypeToFind;
            this.templateName = PlaceholderIsFoundInTemplateName;
            this.existsMode = existsMode;
        }

        public override CmsDependencyMessage[] ValidateDependency()
        {
            bool exists = PlaceholderUtils.PlaceholderExists(placeholderType);
            if (existsMode == ExistsMode.MustExist && !exists)
                return new CmsDependencyMessage[] { CmsDependencyMessage.Error("The placeholder type \"" + placeholderType + "\" was not found. It was specified in template \"" + templateName + "\". Perhaps this placeholder has been removed from HatCMS?") };
            else if (existsMode == ExistsMode.MustNotExist && exists)
                return new CmsDependencyMessage[] { CmsDependencyMessage.Error("The placeholder type \"" + placeholderType + "\" must be removed in template \"" + templateName + "\".") };
            else
                return new CmsDependencyMessage[0];
        }

        public override string GetContentHash()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(placeholderType.ToLower());
            ret.Append(existsMode.ToString());
            return ret.ToString();
        }
    }
}
