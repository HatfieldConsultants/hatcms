using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    /// <summary>
    /// A placeholder that stores HTML content.
    /// Parameters allowed include: width="100px" height="200px" OutputTemplate="{0}" OutputOnlyIfHasContent="true|false" OutputOnlyInEditMode="true|false" 
    /// width and height parameters are only used in EditMode.    
    /// The renderTemplate only has one formatting item ({0}) which is the HTML stored in the placeholder.
    /// if "OutputOnlyIfHasContent" is true will only output if the html has content otherwise will output nothing. Note: tags are stripped to determin if placeholder has any content.
    /// if "OutputOnlyInEditMode" is true will only output if the current user is in Edit Mode otherwise will never output anything. (useful for notes to other authors)
    /// </summary>
	public class HtmlContent: BaseCmsPlaceholder
	{
        
        private class RenderParameters
        {
            public static RenderParameters fromParamList(string[] paramList)
            {
                return new RenderParameters(paramList);
            }

            public string renderWidth = "100%";
            public string renderHeight = "200px";
            public string renderTemplate = "{0}";
            public bool outputOnlyIfHasContent = false;
            public bool outputOnlyInEditMode = false;

            public RenderParameters(string[] paramList)
            {
                if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1)
                {
                    if (paramList.Length > 0 && paramList[0].Trim() != "")
                        renderWidth = paramList[0];

                    if (paramList.Length > 1 && paramList[1].Trim() != "")
                        renderHeight = paramList[1];

                    if (paramList.Length > 2 && paramList[2].Trim() != "")
                        renderTemplate = paramList[2];

                    if (paramList.Length > 3 && paramList[3].Trim() != "" && String.Compare(paramList[3].Trim(), "OutputOnlyIfHasContent", true) == 0)
                    {
                        outputOnlyIfHasContent = true;
                    }

                    if (paramList.Length > 3 && paramList[3].Trim() != "" && String.Compare(paramList[3].Trim(), "OutputOnlyInEditMode", true) == 0)
                    {
                        outputOnlyInEditMode = true;
                    }
                }
                else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                {
                    renderWidth = PlaceholderUtils.getParameterValue("width", renderWidth, paramList);
                    renderHeight = PlaceholderUtils.getParameterValue("height", renderHeight, paramList);
                    renderTemplate = PlaceholderUtils.getParameterValue("OutputTemplate", renderTemplate, paramList);
                    outputOnlyIfHasContent = PlaceholderUtils.getParameterValue("OutputOnlyIfHasContent", outputOnlyIfHasContent, paramList);
                    outputOnlyInEditMode = PlaceholderUtils.getParameterValue("OutputOnlyInEditMode", outputOnlyInEditMode, paramList);
                }
            }

        } // RenderParameters
        
        public HtmlContent()
		{	
            // constructor (do nothing)
		}

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- writable directories
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("UserFiles/Image"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("UserFiles/File"));

            // -- config
            if (CmsConfig.Languages.Length > 1)
            {
                ret.Add(new CmsConfigItemDependency("LinkMacrosIncludeLanguage"));
            }

            // -- ckeditor
            ret.AddRange(CKEditorHelpers.CKEditorDependencies);

            // -- Database tables
            ret.Add(new CmsDatabaseTableDependency("htmlcontent", new CmsDatabaseTableDependency.DBColumnDescription[]
                        {   new CmsDatabaseTableDependency.DBColumnDescription("HtmlContentId"),
                            new CmsDatabaseTableDependency.DBColumnDescription("PageId"),
                            new CmsDatabaseTableDependency.DBColumnDescription("Identifier"),
                            new CmsDatabaseTableDependency.DBColumnDescription("RevisionNumber"),
                            new CmsDatabaseTableDependency.DBColumnDescription("html"),
                            new CmsDatabaseTableDependency.DBColumnDescription("Deleted")
                        }));
            return ret.ToArray();
        }        

        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            HtmlContentDb htmlDb = new HtmlContentDb();
            foreach (int identifier in identifiers)
            {
                string oldHtml = htmlDb.getHtmlContent(oldPage, identifier, language, false);
                bool b = htmlDb.saveUpdatedHtmlContent(currentPage, identifier, language, oldHtml);
                if (!b)
                    return RevertToRevisionResult.Failure;
            } // foreach

            return RevertToRevisionResult.Success;
        }
        		

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {

            HtmlContentDb db = new HtmlContentDb();
            RenderParameters param = RenderParameters.fromParamList(paramList);

            string htmlContent = "";

            string editorId = "htmlcontent_" + page.ID.ToString() + "_" + identifier.ToString() +"_"+ langToRenderFor.shortCode;

            // ------- CHECK THE FORM FOR ACTIONS
            string action = PageUtils.getFromForm(editorId + "_Action", "");
            if (action.Trim().ToLower() == "update")
            {
                htmlContent = PageUtils.getFromForm("name_" + editorId, "");
                bool b = db.saveUpdatedHtmlContent(page, identifier, langToRenderFor, htmlContent);                
            }
            else
            {
                htmlContent = db.getHtmlContent(page, identifier, langToRenderFor, true);
            }

            // ------- START RENDERING
            // -- get the Javascript
            StringBuilder html = new StringBuilder();

            string EOL = Environment.NewLine;

            // -- render the Control			

            // Add the javascript references
            CKEditorHelpers.AddPageJavascriptStatements(page, editorId, param.renderWidth, param.renderHeight, langToRenderFor);

            StringBuilder arg0 = new StringBuilder();


            arg0.Append("<div style=\"width: 100%\">");

            arg0.Append("<textarea name=\"name_" + editorId + "\" id=\"" + editorId + "\" style=\"width: " + param.renderWidth + "; height: " + param.renderHeight + ";\">");
            arg0.Append(htmlContent);
            arg0.Append("</textarea>" + EOL);

            arg0.Append("<input type=\"hidden\" name=\"" + editorId + "_Action\" value=\"update\">");

            arg0.Append("</div>");

            string formattedOutput = String.Format(param.renderTemplate, arg0.ToString());

            html.Append(formattedOutput);

            writer.WriteLine(html.ToString());

        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
            RenderParameters param = RenderParameters.fromParamList(paramList);
            if (param.outputOnlyInEditMode)
                return; // output nothing

            HtmlContentDb db = new HtmlContentDb();
            string arg0 = db.getHtmlContent(page, identifier, langToRenderFor, true);

            bool doOutput = true;
            if (param.outputOnlyIfHasContent)
            {
                if (arg0.Trim() == "")
                    return; // output nothing (and short-circuit the StripHTMLTags function)
                string textOnly = StringUtils.StripHTMLTags(arg0);
                if (textOnly.Trim() != "")
                    doOutput = true;
                else
                    return; // output nothing (short-circuit everything else)
            }

            if (doOutput)
            {
                string formattedOutput = String.Format(param.renderTemplate, arg0.ToString());

                writer.WriteLine(formattedOutput);
            }
        } // RenderView
	}
}
