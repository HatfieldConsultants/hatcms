using System;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.Text;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// When in EditMode, outputs the text found in the parameterList for this placeHolder.
	/// This placeholder does NOT access the database.
	/// Use this placeHolder when you want to output Help text in the EditMode.
	/// </summary>
	public class EditModeComment: BaseCmsPlaceholder
	{
        public override CmsDependency[] getDependencies()
        {            
            return new CmsDependency[0]; // no dependencies
        }
       

        public override bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return true; // this placeholder doesn't implement revisions
        }
        
        public EditModeComment()
		{
			//
			// TODO: Add constructor logic here
			//
		}
				

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            // nothing to render in ViewMode
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{									
			// -- output the text in the parameters
			if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1 && paramList.Length > 0)
			{
				writer.Write("<div class=\"EditModeComment\">");
				foreach(string s in paramList)
				{
					writer.Write(s);
				} // foreach
				writer.Write("</div>");
			}
            else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                string text = PlaceholderUtils.getParameterValue("text", "", paramList);
                if (text.Trim() != "")
                {
                    writer.Write("<div class=\"EditModeComment\">" + text + "</div>");
                }
            }
		} // RenderEdit
		
	}
}
