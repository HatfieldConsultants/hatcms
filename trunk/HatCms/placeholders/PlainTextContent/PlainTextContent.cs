using System;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;


namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for HtmlContent.
	/// </summary>
	public class PlainTextContent: BaseCmsPlaceholder
	{
        public PlainTextContent()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency("plaintextcontent"));
            return ret.ToArray();
        }        
        

        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }				

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{

            PlainTextContentDb db = new PlainTextContentDb();			
			string plainTextContent = "";			

            string width = "100%";
            string height = "200px";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1)
            {
                if (paramList.Length > 0)
                    width = paramList[0] as string;

                if (paramList.Length > 1)
                    height = paramList[1] as string;
            }
            else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
            }
            else
                throw new NotImplementedException("Error: invalid TemplateEngine version");




            string editorId = "plaintextcontent_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;

			// ------- CHECK THE FORM FOR ACTIONS
			string action = Hatfield.Web.Portal.PageUtils.getFromForm(editorId+"_Action","");
			if (action.Trim().ToLower() == "update")
			{
                plainTextContent = Hatfield.Web.Portal.PageUtils.getFromForm("name_" + editorId, "");
                db.saveUpdatedPlainTextContent(page, identifier,langToRenderFor, plainTextContent);				
			}
			else
			{
                plainTextContent = db.getPlainTextContent(page, identifier, langToRenderFor, true);
			}

			// ------- START RENDERING			
			StringBuilder html = new StringBuilder();						
			// -- render the Control			
			// note: no need to put in the <form></form> tags.

			html.Append("<textarea name=\"name_"+editorId+"\" id=\""+editorId+"\" style=\"WIDTH: "+width+"; HEIGHT: "+height+";\">");
            html.Append(plainTextContent);
			html.Append("</textarea>");

            html.Append("<input type=\"hidden\" name=\"" + editorId + "_Action\" value=\"update\">");			
			
			writer.WriteLine(html.ToString());

		}

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            PlainTextContentDb db = new PlainTextContentDb();
			string html = db.getPlainTextContent(page, identifier, langToRenderFor, true);
			writer.WriteLine(html);
		}
	}
}
